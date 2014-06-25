[<ReflectedDefinition>]
module SignalRClient 

open SignalRProvider

open FunScript
open FSharp.Data
open System.IO

let signalR = Globals.Dollar.signalR
let j (s: string) = Globals.Dollar.Invoke(s)
let hub = signalR.hub
let proxy = hub.createHubProxy("myHub")
let log = Globals.console.log

let serverHub = new Hubs.myhub(signalR.hub)

let logDeferred s (df: JQueryDeferred<_>) =
    df._done (fun _ -> log <| s + "done") |> ignore
    df.fail (fun _ -> log <| s + "fail") |> ignore
    ()

let jqIgnore x = 
    x
    null : obj

let start () = 

    serverHub.testUpdating3() |> ignore
    serverHub.functionWith4Args(1, "2", new obj(), 5) |> ignore

    let intList1 = ([|1;2;3|] :> obj) :?> Underscore.List<int>
    let intList2 = ([|4;5;6|] :> obj) :?> Underscore.List<int>

    let u = Globals.Underscore<int>([|1|]).union(intList1, intList2)

    let argss = [| intList1; intList2 |]
    let v = Globals.Underscore<int>([|1|]).union( argss )
    let w = Globals.Underscore<int>([|1|]).union( [| intList1; intList2 |] )

    j("#submit").click (fun _ -> 
        serverHub.functionWith3Args(1, "2", 3)._done( fun (x: obj) -> log <| x.ToString() ) |> ignore
        serverHub.MyCustomServerFunction(j("#source")._val() :?> string)
        )
    |> ignore
    log "##Started!##"

let printResult (value : string) =
    //sprintf "<p>%s</p>" value
    "<p>"+ value + "" + "</p>"
    |> j("#results").append
    |> ignore

let main() = 
    Globals.console.log("##Starting:## ")
    signalR.hub.url <- "/signalrHub"

    proxy.on("myCustomClientFunction", fun args -> 
        let s = (args :> obj) :?> string // TODO varargs goes wrong
        printResult s
        "Response: " + s |> log) 
    |> ignore

    hub.start start

type Wrapper() =
    member this.GenerateScript() = Compiler.compileWithoutReturn <@ main() @>