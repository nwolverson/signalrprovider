open System.IO

[<EntryPoint>]
let main argv = 
    let script = (new SignalRClient.Wrapper()).GenerateScript()
    File.WriteAllText("SignalRFunScriptClient.js", script)
    0 // return an integer exit code

