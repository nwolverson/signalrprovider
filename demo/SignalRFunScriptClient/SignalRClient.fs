﻿[<ReflectedDefinition>]
module SignalRClient 

open FunScript.TypeScript
open FunScript

open SignalRProvider

open System.IO

let signalR = Globals.Dollar.signalR
let j (s: string) = Globals.Dollar.Invoke(s)
let proxy = signalR.hub.createHubProxy("myHub")
let log = Globals.console.log

let serverHub = new Hubs.myhub(signalR.hub)

let logDeferred s (df: JQueryDeferred<_>) =

    df._doneOverload2 (fun _ -> log <| s + "done") |> ignore
    df.failOverload2 (fun _ -> log <| s + "fail") |> ignore
    ()

let jqIgnore x = 
    x
    null : obj

type ComplexLocalType() =
    member val XX = 42 with get,set
    member val YY = "abc" with get,set

type complexType = SignalRProvider.Types.``SignalRServer!MyServer+ComplexObject``

open SignalRProviderRuntime
let onstart () = 
    let compty = new complexType()
    compty.Number <- 43
    compty.Text <- "abc"
    serverHub.functionWith4Args(1, "2", compty, 4) |> ignore
   
    j("#submit").click (fun _ -> 
        serverHub.functionWith3Args(1, "2", 3)._doneOverload2( fun (x: obj) -> log <| x.ToString() ) |> ignore
        serverHub.MyCustomServerFunction(j("#source")._val() :?> string) |> ignore
        new obj()
        )
    |> ignore
    log "##Started!##"

let printResult (value : string) =
    //sprintf "<p>%s</p>" value
    "<p>"+ value + "" + "</p>"
    |> j("#results").append
    |> ignore

[<SignalRProviderRuntime.ClientHub>]
type ClientHub() =
    member this.myTypedFunction (x: int) = 
        log <| "myTypedFunction called with argument: "+x.ToString()
    member this.myTypedFunction42 (x: int) = 
        log <| "myTypedFunction42 called with argument: "+x.ToString()
    member this.functionWith2Args (x: int) (y: string) = 
        log <| "functionWith2Args called with arguments: "+x.ToString()+y

    member this.myCustomClientFunction (s: string) =
        printResult s
        "Response: " + s |> log

let fixVarArgs (f: 't -> 't2) = System.Func<_,_>(fun (arg : obj array) -> f (arg :> obj :?> 't))

let main() = 
    Globals.console.log("##Starting:## ")
    signalR.hub.url <- "http://localhost:48213/signalrHub"

    let h = new ClientHub()
    proxy.on("myCustomClientFunction", h.myCustomClientFunction |> fixVarArgs)
        .on("myTypedFunction", h.myTypedFunction |> fixVarArgs) 
        .on("myTypedFunction42", h.myTypedFunction42 |> fixVarArgs)
        //.on("functionWith2Args", h.functionWith2Args)
        |> ignore

    signalR.hub.start onstart

type Wrapper() =
    member this.GenerateScript() = Compiler.compileWithoutReturn <@ main() @>