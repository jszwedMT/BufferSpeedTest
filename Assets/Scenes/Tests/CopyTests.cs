using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor.SearchService;
using UnityEngine;

public class CopyTests : MonoBehaviour
{
    public enum TestType
    {
        None = -1,
        GeneralFrameOverhead,
        RecopyCPU,
        CachedCPU,
        RecopyGPU,
        CachedGPU,
        VertexBufferGPU,
        All,
    }

    public TestType testType;
    public int IterationsPerTest;
    public GameObject[] Tests;

    public ComputeShader shader;
    public int kernelIndex;

    public ComputeShader graphicsShader;
    public int graphicsKernelIndex;

    int _onTest = -1;
    int _iteration = 0;
    Mesh[] _meshes;

    Stopwatch _totalTime = new Stopwatch();

    Vector3[] cachedVertexArray;
    ComputeBuffer cachedComputeBuffer;
    GraphicsBuffer cachedGraphicsBuffer;

    TestType _currentTestType;

    int _treadGroupx = 0;
    int _treadGroupY = 1;
    int _treadGroupZ = 1;

    private void Start()
    {
        List<Mesh> list = new List<Mesh>();
        foreach (var test in Tests)
        {
            MeshFilter filter;
            if (test.TryGetComponent<MeshFilter>(out filter))
            {
                list.Add(filter.mesh);
            }
            else
            {
                UnityEngine.Debug.LogError($"Could not test on {test.name} it is missing a mesh filter.");
            }
        }

        _meshes = list.ToArray();
        kernelIndex = shader.FindKernel("CSMain");
        graphicsKernelIndex = graphicsShader.FindKernel("CSMain");
    }

    private void Update()
    {
        if (_onTest == -1 || _iteration >= IterationsPerTest)
        {
            if (testType == TestType.None)
            {
                GameObject.Destroy(this.gameObject);
                return;
            }
            else if (testType != TestType.All)
            {
                _currentTestType = testType;
            }

            if (_onTest != -1) 
            {
                _totalTime.Stop();
                UnityEngine.Debug.Log($"{_currentTestType}: {Tests[_onTest].name} took {_totalTime.Elapsed} averaging {_totalTime.Elapsed / _iteration} per frame");
            }

            _totalTime.Reset();
            _totalTime.Start();

            _onTest++;
            if (_onTest == _meshes.Length)
            {
                if (testType == TestType.All)
                {
                    if (_currentTestType == TestType.All)
                    {
                        GameObject.Destroy(this.gameObject);
                        return;
                    }
                    else
                    {
                        _currentTestType++;
                        _onTest = 0;
                    }
                }
                else
                {
                    GameObject.Destroy(this.gameObject);
                    return;
                }
            }

            _iteration = 0;
            
            cachedVertexArray = _meshes[_onTest].vertices;

            if (cachedComputeBuffer != null)
            {
                cachedComputeBuffer.Release();
                cachedComputeBuffer = null;
            }
            cachedComputeBuffer = new ComputeBuffer(cachedVertexArray.Length, sizeof(float) * 3);
            cachedComputeBuffer.SetData(cachedVertexArray);

            if (cachedGraphicsBuffer != null)
            {
                cachedGraphicsBuffer.Release();
                cachedGraphicsBuffer = null;
            }

            Mesh mesh = _meshes[_onTest];
            mesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;
            cachedGraphicsBuffer = mesh.GetVertexBuffer(0);

            shader.SetBuffer(kernelIndex, "Input", cachedComputeBuffer);
            graphicsShader.SetBuffer(graphicsKernelIndex, "Input", cachedGraphicsBuffer);

            _treadGroupx = Mathf.CeilToInt(cachedVertexArray.Length / 1024f);
        }
        else if (_iteration < IterationsPerTest)
        {
            _iteration++;
            switch (_currentTestType)
            {
                case TestType.GeneralFrameOverhead:
                    GeneralFrameOverhead();
                    break;
                case TestType.RecopyCPU:
                    RecopyCPU();
                    break;
                case TestType.CachedCPU:
                    CacheCPU();
                    break;
                case TestType.RecopyGPU:
                    RecopyGPU();
                    break;
                case TestType.CachedGPU:
                    CachedGPU();
                    break;
                case TestType.VertexBufferGPU:
                    VertexBufferGPU();
                    break;
            }
        }
    }

    private void GeneralFrameOverhead()
    {

    }

    private void RecopyCPU()
    {
        Vector3[] verts = _meshes[_onTest].vertices;
        Quaternion rotation = Quaternion.Euler(90 * Time.deltaTime, 0, 0);
        Matrix4x4 trs = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);

        for (int i = 0; i < verts.Length; i++) 
        {
            verts[i] = trs * verts[i];
        }

        _meshes[_onTest].vertices = verts;
    }

    private void CacheCPU()
    {
        Quaternion rotation = Quaternion.Euler(90 * Time.deltaTime, 0, 0);
        Matrix4x4 trs = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
        for (int i = 0; i < cachedVertexArray.Length; i++)
        {
            cachedVertexArray[i] = trs * cachedVertexArray[i];
        }

        _meshes[_onTest].vertices = cachedVertexArray;
    }

    private void RecopyGPU()
    {
        Vector3[] verts = _meshes[_onTest].vertices;
        Quaternion rotation = Quaternion.Euler(90 * Time.deltaTime, 0, 0);
        Matrix4x4 trs = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);

        ComputeBuffer computeBuffer = new ComputeBuffer(verts.Length, sizeof(float) * 3);
        computeBuffer.SetData(verts);

        shader.SetBuffer(kernelIndex, "Input", computeBuffer);
        shader.SetInt("Count", verts.Length);
        shader.SetMatrix("TRS", trs);
        shader.Dispatch(kernelIndex, _treadGroupx, _treadGroupY, _treadGroupZ);

        computeBuffer.GetData(verts, 0, 0, verts.Length);
        computeBuffer.Release();

        _meshes[_onTest].vertices = verts;
    }

    private void CachedGPU()
    {
        Quaternion rotation = Quaternion.Euler(90 * Time.deltaTime, 0, 0);
        Matrix4x4 trs = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);

        shader.SetInt("Count", cachedVertexArray.Length);
        shader.SetMatrix("TRS", trs);
        shader.Dispatch(kernelIndex, _treadGroupx, _treadGroupY, _treadGroupZ);

        cachedComputeBuffer.GetData(cachedVertexArray, 0, 0, cachedVertexArray.Length);

        _meshes[_onTest].vertices = cachedVertexArray;
    }

    private void VertexBufferGPU()
    {
        Quaternion rotation = Quaternion.Euler(90 * Time.deltaTime, 0, 0);
        Matrix4x4 trs = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);

        graphicsShader.SetInt("Count", cachedVertexArray.Length);
        graphicsShader.SetMatrix("TRS", trs);
        graphicsShader.Dispatch(kernelIndex, _treadGroupx, _treadGroupY, _treadGroupZ);
    }
}
