#r @"packages/FAKE/tools/FakeLib.dll"

open Fake

let output = [
    "FunScript.TypeScript.Binding.jquery.dll";
    "FunScript.TypeScript.Binding.lib.dll";
    "FunScript.TypeScript.Binding.signalr.dll";
    "ReflectionProxy.dll";
    "SignalRProvider.dll";
    "SignalRProviderRuntime.dll"
    ] 

Target "Clean" (fun _ ->
    !! "**/bin" ++ "**/obj" ++ "build"
        |> CleanDirs
)

let buildDir = @"SignalRProvider\bin\Release\"
Target "Build" (fun _ ->
    ["SignalRProvider.sln"]
        |> MSBuildWithDefaults "Build"
        |> Log "AppBuild-Output: "
)

Target "Copy" (fun _ ->
    output 
        |> List.map ((+) buildDir)
        |> Copy "build"
)

Target "BuildDemo" (fun _ ->
    !! "demo/**/*.sln"
        |> MSBuildReleaseExt null [("LCID","1033")] "Build" 
        |> Log "DemoBuild-Output: "
)

Target "Default" (fun _ -> ())

"Clean"
    ==> "Build"
    ==> "Copy"
    ==> "BuildDemo"
    ==> "Default"


RunTargetOrDefault "Default"