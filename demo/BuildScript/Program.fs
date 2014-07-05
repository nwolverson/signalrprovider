// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

open System.IO

[<EntryPoint>]
let main argv = 
    let script = (new SignalRClient.Wrapper()).GenerateScript()
    File.WriteAllText("/Users/nicholaw/Sites/Scripts/SignalRFunScriptClient.js", script)
    0 // return an integer exit code

