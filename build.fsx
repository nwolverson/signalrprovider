#r @"packages/FAKE/tools/FakeLib.dll"
#r @"packages/FAKE/tools/Fake.IIS.dll"
#r @"System.Xml.Linq.dll"

open Fake
open Fake.IISExpress

open System
open System.IO
open System.Xml.Linq

/// Create a IISExpress config file from a given template
let createConfigFile (name, siteId : int, templateFileName, path, hostName, port : int) = 
    let xname s = XName.Get(s)
    let uniqueConfigFile = Path.Combine(Path.GetTempPath(), "iisexpress-" + Guid.NewGuid().ToString() + ".config")
    use template = File.OpenRead(templateFileName)
    let xml = XDocument.Load(template)
    let sitesElement = xml.Root.Element(xname "system.applicationHost").Element(xname "sites")
    let appElement = 
        XElement
            (xname "site", XAttribute(xname "name", name), XAttribute(xname "id", siteId.ToString()), 
             XAttribute(xname "serverAutoStart", "true"), 
             
             XElement
                 (xname "application", XAttribute(xname "path", "/"), 
                  
                  XElement
                      (xname "virtualDirectory", XAttribute(xname "path", "/"), XAttribute(xname "physicalPath", DirectoryInfo(path).FullName))), 
             
             XElement
                 (xname "bindings", 
                  
                  XElement
                      (xname "binding", XAttribute(xname "protocol", "http"), 
                       XAttribute(xname "bindingInformation", "*:" + port.ToString() + ":" + hostName))))
    sitesElement.Add(appElement)
    xml.Save(uniqueConfigFile)
    uniqueConfigFile


Target "RestorePackages" (fun _ ->
    "SignalRProvider.sln" 
        |> RestoreMSSolutionPackages (fun p -> { p with OutputPath = "packages" })
    "demo/SignalRProviderTest.sln" 
        |> RestoreMSSolutionPackages (fun p -> { p with OutputPath = "demo/packages" })
    "test/IntegrationTests/IntegrationTests.sln" 
        |> RestoreMSSolutionPackages (fun p -> { p with OutputPath = "test/IntegrationTests/packages" })
)

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
    -- "node_modules/**"
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
        |> MSBuildWithDefaults "Build" 
        |> Log "DemoBuild-Output: "
)

Target "GenerateJSDemo" (fun _ ->
    //FileUtils.cd "demo/BuildScript/bin/Release"
    let result = ExecProcess (fun p ->
        p.WorkingDirectory <- @"demo\BuildScript\bin\Release\"
        p.FileName <- @"demo\BuildScript\bin\Release\BuildScript.exe") (System.TimeSpan.FromMinutes 1.)
        
    if result <> 0 then failwith "Failed to generate JS"
)

Target "BuildTest" (fun _ ->
    !! "test/**/*.sln"
        |> MSBuildReleaseExt null [("LCID","1033")] "Build" 
        |> Log "DemoBuild-Output: "
)

Target "GenerateJSTest" (fun _ ->
    let result = ExecProcess (fun p ->
        p.WorkingDirectory <- @"test\IntegrationTests\bin\Release\"
        p.FileName <- @"test\IntegrationTests\bin\Release\IntegrationTests.exe") (System.TimeSpan.FromMinutes 1.)
        
    if result <> 0 then failwith "Failed to generate JS"
)

Target "RunJavascriptTest" (fun _ ->
    
    let hostName = "localhost"
    let port = 8092
    let buildDir = "build/web"
    let siteName = "testsite2"
    let webDir = "test/Server" //buildDir + "/" + siteName

    let config = createConfigFile(siteName, 4, "iisexpress-template.config", webDir, hostName, port)
    let webSiteProcess = HostWebsite id config 4

    [
        @"node_modules\mocha\mocha.js"
        @"node_modules\mocha\mocha.css"
    ]
        |> Copy webDir

    let mochaPhantom = @"node_modules\mocha-phantomjs\bin\mocha-phantomjs"
    let testFile = sprintf "http://%s:%d/" hostName port 

    let result =
        ExecProcess (fun info ->
            info.FileName <- "node"
            info.Arguments <- [mochaPhantom; testFile] |> String.concat " "
        ) (System.TimeSpan.FromMinutes 5.)

    ProcessHelper.killProcessById webSiteProcess.Id

    if result <> 0 then failwith "Failed result from integration tests"
)

Target "Default" (fun _ -> ())

"Clean"
    ==> "RestorePackages"
    ==> "Build"
    ==> "Copy"

"RestorePackages" ==> "BuildDemo"
"RestorePackages" ==> "BuildTest"
"Copy" ==> "BuildDemo"
"Copy" ==> "BuildTest"

"BuildDemo" ==> "GenerateJSDemo"

"Build" ==> "Default"

"GenerateJSDemo" ==> "Default"

"BuildTest" ==> "GenerateJSTest"
    ==> "RunJavascriptTest"



"RunJavascriptTest" ==> "Default"

RunTargetOrDefault "Default"