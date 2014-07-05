[<ReflectedDefinition>]
module SignalRClient 

open SignalRProvider

open FunScript.TypeScript
open FunScript

open FSharp.Data
open System.IO

type t = int * string


type complexType = SignalRProvider.Types.``SignalRServer!MyServer+ComplexObject``

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

let start () = 

    serverHub.testUpdating3() |> ignore
    let arg = new complexType(Number=123) // (Number = 123, Text = "Test")
    
    let d = Map.empty.Add("a", 1)
    let d1 = d.Add("b", 42)

    let ay = d1.["a"]

    serverHub.functionWith4Args(1, "2", arg, 5) |> ignore
    //serverHub.complexArg()

    let intList1 = ([|1;2;3|] :> obj) :?> Underscore.List<int>
    let intList2 = ([|4;5;6|] :> obj) :?> Underscore.List<int>

    
    let u = Globals.Underscore.Invoke<int>([|1|]).unionOverload2(intList1, intList2)

    let argss = [| intList1; intList2 |]
    let v = Globals.Underscore.Invoke<int>([|1|]).unionOverload2 argss 
    let w = Globals.Underscore.Invoke<int>([|1|]).unionOverload2  [| intList1; intList2 |] 

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

let main() = 
    Globals.console.log("##Starting:## ")
    signalR.hub.url <- "/signalrHub"

    proxy.on("myCustomClientFunction", fun args -> 
        let s = (args :> obj) :?> string // TODO varargs goes wrong
        printResult s
        "Response: " + s |> log) 
    |> ignore

    signalR.hub.start start

type Wrapper() =
    member this.GenerateScript() = Compiler.compileWithoutReturn <@ main() @>