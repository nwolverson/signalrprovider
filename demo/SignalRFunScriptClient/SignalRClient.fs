[<ReflectedDefinition>]
module SignalRClient 

open FunScript.TypeScript
open FunScript

open SignalRProvider

let signalR = Globals.Dollar.signalR
let j (s: string) = Globals.Dollar.Invoke(s)
let log = Globals.console.log

let serverHub = new Hubs.myServerHub(signalR.hub)

let onstart () = 
    j("#submit").click (fun _ -> 
        serverHub.SendMessage (j("#source")._val() :?> string) |> ignore
        j("#source")._val("") |> ignore
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

    let client = Hubs.ChatClientHub()
    client.BroadcastMessage <- (fun msg -> printResult msg)
    client.Register(signalR.hub)

    signalR.hub.start onstart

type Wrapper() =
    member this.GenerateScript() = Compiler.compileWithoutReturn <@ main() @>