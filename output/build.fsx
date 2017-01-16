// include Fake lib
#r @"../packages/FAKE/tools/FakeLib.dll"
open Fake
open FileUtils
open AssemblyInfoFile

// Properties
let version = "0.0.1.97"
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
   else trace (" Kapoin version parameter: " + version + ".") )

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
  |> MSBuildRelease buildDir "Build"
  |> Log "Main build output: " )

// Copy the compiled DLLs to a ZIP file (note: extract to /GameData/Rodhern/Plugins/ when ready)
Target "ZipResult" (fun _ ->
  let vername = version.Replace (".", "-")
  !! (buildDir + "/FSharp.Core.dll")
  ++ (buildDir + "/KapoinWrappers.dll")
  ++ (buildDir + "/KapoinHelpers.dll")
  ++ (buildDir + "/Kapoin.dll")
  |> Zip buildDir ("./output/Kapoin-DLLs-ver-" + vername + ".zip") )

// Build the helper project with MSBuild (note: not in use)
Target "BuildHelper" (fun _ ->
  !! "./Kapoin_02_Helpers/Kapoin_02_Helpers.fsproj"
  |> MSBuildRelease buildDir "Build"
  |> Log "Helper build output: " )

// Build the wrapper project with MSBuild (note: not in use)
Target "BuildWrapper" (fun _ ->
  !! "./Kapoin_01_Wrappers/Kapoin_01_Wrappers.csproj"
  |> MSBuildRelease buildDir "Build"
  |> Log "Wrapper build output: " )

// Dependencies
"CheckForDirty"
  ==> "Clean"
  ==> "Prepare"
  ==> "BuildMain"
  ==> "ZipResult"

// start build
RunTargetOrDefault "ZipResult"
