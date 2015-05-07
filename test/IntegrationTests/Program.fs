[<ReflectedDefinition>]
module Tests

open FunScript
open FunScript.TypeScript
open System.IO
open System

open SignalRProvider

[<JSEmit("throw new Error({0})")>]
let fail msg = failwith msg
let assert' b = if not b then fail "Assertion failed"
let assertm msg b = if not b then fail <| "Assertion failed: " + msg

let tocb (f : unit -> unit) = f :> obj:?> JQueryPromiseCallback<_>

let tests() =
    let signalR = Globals.Dollar.signalR
    let serverHub = new Hubs.myServerHub(signalR.hub)
    
    let describe s f = Globals.describe.Invoke(s, Func<_> f)
    let it s f = Globals.it.Invoke(s, Func<_> f)
    let ait s f = Globals.it.InvokeOverload2(s, Func<_,_> f)

    let calldone (d: MochaDoneDelegate) = d.Invoke(null :> obj :?> Error)

    let called = ref (fun () -> ())
    let clientHub = Hubs.MyAwesomeClientHub()
    clientHub.FiveArgs <- fun _ _ _ _ _ -> Globals.console.log "Called FiveArgs" ; (!called)()
    clientHub.Register(signalR.hub)

    Globals.before (fun (ondone : MochaDoneDelegate) -> 
        signalR.hub.url <- "http://localhost:8092/signalrHub"
        signalR.hub.start (fun () -> calldone ondone) |> ignore
    )
    
    Globals.beforeEach (fun () -> 
        called := fun () -> ()
    )

    describe "Tests" <| fun () -> 
        it "getter/setter works" <| fun () ->
            let x = Types.``Server!TestServer+ComplexObject``()
            x.Text <- "Hello"
            x.Text = "Hello" |> assert'

        ait "functionWith3Args" <| fun d ->
            serverHub.functionWith3Args(42, "abc", 1)._done(tocb (fun () -> calldone d)) |> ignore
        ait "functionWith4Args" <| fun d ->
            let x = Types.``Server!TestServer+ComplexObject``()
            x.Text <- "Hello"
            serverHub.functionWith4Args(123, "abc", x, 4)._done(tocb (fun () -> calldone d)) |> ignore

    describe "Callbacks" <| fun () ->
        ait "functionWith3Args" <| fun d ->
            called := fun () -> calldone d
            serverHub.functionWith3Args(42, "abc", 1)._done(tocb (fun () -> ())) |> ignore

[<EntryPoint>]
let main argv = 
    let script = Compiler.compileWithoutReturn <@ tests() @>
    File.WriteAllText("..\..\..\Server\AllTests.js", script)
    0 // return an integer exit code
