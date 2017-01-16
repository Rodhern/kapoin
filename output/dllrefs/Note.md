## F-sharp core library (open edition for .Net 3.5)

Files build from [Github fork](https://github.com/Rodhern/fsharp/tree/net35/).


### Redistributable executable

FSharp.Core.dll  - redistributed in final software (needed at compile time and run time)


### Mandatory compiler data

FSharp.Core.sigdata  - needed by Visual Studio when compiling F-sharp projects (compile time)
FSharp.Core.optdata  - do


### Optional compiler data

FSharp.Core.xml  - documentation used by Visual Studio to provide Intellisense (optional)


### Optional debug symbol data

FSharp.Core.pdb  - used by the Visual Studio debugger (for debug only)


## Remark

When doing a Git checkout the file times (the 'modified date' stamps) are not preserved. It may make more sense to store binary files in archives, e.g. Zip files.
