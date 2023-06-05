# BufferSpeedTest

This intent of this repo is to test the variances between cpu and gpu operations on a mesh in unity using the old API methods, and the new GetVertexBuffer

The average frame-overhead was: 19.32

Attempt | 8 verts | 8 verts - oh | 98 | 98 - oh | 386 | 386 - oh | 1.5k | 1.5k - oh | 12.2k | 12.2k - oh | 49k | 49k - oh
--- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | ---
general overhead | 19.57 | 0 | 19.43 | 0 | 19.23 | 0 | 19.28 | 0 | 19.17 | 0 | 19.24 | 0
overcopy-cpu | 21.45 | 2.13 | 22.37 | 3.05 | 23.45 | 4.13 | 27.46 | 8.14 | 38.28 | 18.96 | 1:01.32 | 42.0
cache-cpu | 21.50 | 2.18 | 21.85 | 2.53 | 23.14 | 3.86 | 26.74 | 7.42 | 36.27 | 16.95 | 56.76 | 37.44
overcopy-gpu | 34.40 | 15.08 | 34.64 | 15.32 | 34.69 | 15.37 | 34.84 | 15.52 | 36.50 | 17.18 | 39.72 | 20.4
cache-gpu | 34.34 | 15.02 | 34.22 | 14.90 | 34.27 | 14.95 | 34.29 | 14.97 | 34.73 | 15.41 | 36.35 | 17.03
direct-vbuffer | 19.47 | 0.15 | 19.49 | 0.17 | 19.48 | 0.16 | 19.50 | 0.18 | 19.49 | 0.17 | 19.50 | 0.18
