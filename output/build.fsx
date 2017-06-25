// include Fake lib
#r @"../packages/FAKE/tools/FakeLib.dll"
open Fake
open FileUtils
open AssemblyInfoFile

// Properties
let version = "0.0.2.97"
let buildProps = ("Configuration", "AnyCPUConfiguration")::[]
let buildDir = "./output/FakeBuildDir/"


// Check if there is Kapoin dlls in the VSBuild output folder
Target "CheckForDirty" (fun _ ->
  let vs = fileExists "./output/VSBuild/KapoinWrappers.dll"
           || fileExists "./output/VSBuild/KapoinHelpers.dll"
           || fileExists "./output/VSBuild/Kapoin.dll"
  if vs
   then let msg = "The VSBuild folder is not clean!"
        traceError msg
        failwith msg
   else trace (" Kapoin version parameter: " + version + ".") 
        for (k,v) in buildProps // write custom build properties in console
         do trace (" build with /p:" + k + "=\"" + v + "\"") )

// Clean build directory (note: ignores ./output/VSBuild/)
Target "Clean" (fun _ -> CleanDir buildDir )

// Update Kapoin Assembly info (from Fake tutorial)
Target "Prepare" (fun _ ->
  CopyFile "./output/FakeBuildDir/FSharp.Core.dll" "./output/dllrefs/FSharp.Core.dll"
  let assemblyInfos = !! (@"./Kapoin_01_Wrappers/AssemblyInfo.cs")
                      ++ (@"./Kapoin_02_Helpers/AssemblyInfo.fs")
                      ++ (@"./Kapoin_03_Main/AssemblyInfo.fs")
  ReplaceAssemblyInfoVersionsBulk assemblyInfos (fun f -> 
    { f with AssemblyVersion = version
             AssemblyFileVersion = version } ) )

// Build the main module with MSBuild
Target "BuildMain" (fun _ ->
  !! "./Kapoin_03_Main/Kapoin_03_Main.fsproj"
  |> MSBuild buildDir "Build" buildProps
  |> Log "Main build output: " )

// Copy the compiled DLLs to a ZIP file (note: extract to /GameData/Rodhern/Plugins/ when ready)
Target "ZipResult" (fun _ ->
  let vername = version.Replace (".", "-")
  !! (buildDir + "/FSharp.Core.dll")
  ++ (buildDir + "/KapoinWrappers.dll")
  ++ (buildDir + "/KapoinHelpers.dll")
  ++ (buildDir + "/Kapoin.dll")
  |> Zip buildDir ("./output/Kapoin-DLLs-ver-" + vername + ".zip") )

// Dependencies
"CheckForDirty"
  ==> "Clean"
  ==> "Prepare"
  ==> "BuildMain"
  ==> "ZipResult"

// start build
RunTargetOrDefault "ZipResult"
