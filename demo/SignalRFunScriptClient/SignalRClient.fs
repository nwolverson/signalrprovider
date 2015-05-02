[<ReflectedDefinition>]
module SignalRClient 

open FunScript.TypeScript
open FunScript

open SignalRProvider
open System

let signalR = Globals.Dollar.signalR
let j (s: string) = Globals.Dollar.Invoke(s)
let log = Globals.console.log

let serverHub = new Hubs.myServerHub(signalR.hub)

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

let onstart () = 
    let compty = new complexType()
    compty.Number <- 43
    compty.Text <- "abc"
    compty.Text <- compty.Text + "def"
    serverHub.functionWith4Args(1, "2", compty, 4) |> ignore
    serverHub.functionWith3Args(1, "2", 3)._doneOverload2( fun (x: obj) -> log <| x.ToString() ) |> ignore
   
    j("#submit").click (fun _ -> 
        serverHub.SendMessage (j("#source")._val() :?> string) |> ignore
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
    signalR.hub.url <- "http://localhost:48213/signalrHub"

    let client = Hubs.MyAwesomeClientHub()
    client.BroadcastMessage <- (fun msg -> printResult msg)
    client.FiveArgs <- (fun a b c d e -> log <| a + b.ToString() + c.ToString() + d + e.ToString())
    client.FiveArgsTupled <- (fun a b c d e -> log <| a + b.ToString() + c.ToString() + d + e.ToString())
    client.SendList <- (fun xs -> log <| xs.Length.ToString())
    client.SendArray <- (fun xsa -> log <| xsa.Length.ToString())
    client.SendSeq <- (fun xss -> log <| xss.Length.ToString())
    client.Complex <-(fun c -> log <| c.Number.ToString() + ", " + c.Text)
    client.Register(signalR.hub)

    signalR.hub.start onstart

type Wrapper() =
    member this.GenerateScript() = Compiler.compileWithoutReturn <@ main() @>