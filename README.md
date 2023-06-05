# BufferSpeedTest

This intent of this repo is to test the variances between cpu and gpu operations on a mesh in unity using the old API methods, and the new GetVertexBuffer

The following results were tested on 6 models of increasing size by turning a Quaternion into a TRS, then multiplying among every single vertex in the model to rotate it. Each of the tests were run over 10000 iterations. the following are the sizes of the models used:

8 verts

98 verts

389 verts

1.5k verts

12.2k verts 

49k verts

with varying methods of computation:

The average frame-overhead was: 19.32

Attempt | 8 | 8 - oh | 98 | 98 - oh | 386 | 386 - oh | 1.5k | 1.5k - oh | 12.2k | 12.2k - oh | 49k | 49k - oh
--- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | ---
general overhead | 19.57 | 0 | 19.43 | 0 | 19.23 | 0 | 19.28 | 0 | 19.17 | 0 | 19.24 | 0
overcopy-cpu | 21.45 | 2.13 | 22.37 | 3.05 | 23.45 | 4.13 | 27.46 | 8.14 | 38.28 | 18.96 | 1:01.32 | 42.0
cache-cpu | 21.50 | 2.18 | 21.85 | 2.53 | 23.14 | 3.86 | 26.74 | 7.42 | 36.27 | 16.95 | 56.76 | 37.44
overcopy-gpu | 34.40 | 15.08 | 34.64 | 15.32 | 34.69 | 15.37 | 34.84 | 15.52 | 36.50 | 17.18 | 39.72 | 20.4
cache-gpu | 34.34 | 15.02 | 34.22 | 14.90 | 34.27 | 14.95 | 34.29 | 14.97 | 34.73 | 15.41 | 36.35 | 17.03
direct-vbuffer | 19.47 | 0.15 | 19.49 | 0.17 | 19.48 | 0.16 | 19.50 | 0.18 | 19.49 | 0.17 | 19.50 | 0.18


general overhead - is just a simple frame ran with no computation.

overcopy-cpu - copy the vertex array from the mesh, modify it, and set the mesh

cache-cpu - copy the vertex array once from the mesh, and keey modifying it, setting the mesh after modification.

overcopy-gpu - copy the vertex array from the mesh, set it in a gpu buffer, compute over the buffer, and set the mesh

cache-gpu - copy data from the mesh once to a gpu buffer, modify the buffer, and set the mesh after modification.

direct-vbuffer - get a copy of the direct gpu buffer from the mesh, and modify the gpu buffer using the new 2021.4 mesh API.

The following is a visualized graph of the results.

![Capture](https://github.com/jszwedMT/BufferSpeedTest/assets/108739402/ebded458-3cd6-494f-ab4c-268fad328590)
