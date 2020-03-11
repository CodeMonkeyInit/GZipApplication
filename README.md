# GZipApplication

A simple multi-threaded cross-platform command line archiver that uses GZip Deflate algorithm.

### Windows usage example
```
GZipApplication.exe compress/decompress {inputFilePath} {outputFilePath}
```

## Implementation details

- Implanted using Threads. Not uses TPL, BackgroundWorker, ThreadPool or async/await.
- Uses Array Pool to reduce LOH allocations
- All IO bound operations (like reading and writing) executed sequentially using IOBoundQueue on Main Thread, CPU bound Work gets executed on CpuBoundWorkQueue which uses amount of threads equal to amount of Processors in system.
