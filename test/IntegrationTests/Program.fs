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

    let ondone (f: unit->unit) (d: JQueryDeferred<obj>) = 
        d._done(tocb f) |> ignore

    let called = ref (fun (_: string) -> ())
    let clientHub = Hubs.MyAwesomeClientHub()
    
    // use callback to test this was called, function can't be overwritten after server start.
    // ideally would restart with new client hub each test
    clientHub.FiveArgs <- fun _ _ _ _ _ -> !called "FiveArgs"
    clientHub.FiveArgsTupled <- fun _ _ _ _ _ -> !called "FiveArgsTupled"
    clientHub.SendArray <- fun _ -> !called "SendArray"
    clientHub.SendList <- fun _ -> !called "SendList"
    clientHub.SendSeq <- fun _ ->  !called "SendSeq"
    clientHub.Complex <- fun _ -> !called "Complex"
    clientHub.Register(signalR.hub)

    Globals.before (fun (ondone : MochaDoneDelegate) -> 
        signalR.hub.url <- "http://localhost:8092/signalrHub"
        signalR.hub.start (fun () -> calldone ondone) |> ignore
    )
    
    Globals.beforeEach (fun () -> 
        called := fun _ -> ()
    )

    describe "Tests" <| fun () -> 
        it "getter/setter works" <| fun () ->
            let x = Types.``TestServer+ComplexObject``()
            x.Text <- "Hello"
            x.Text = "Hello" |> assert'

        ait "fiveArgs" <| fun d ->
            serverHub.FiveArgs("abc", 123, 1., "fff", 42) |> ondone (fun () -> calldone d)
        ait "fiveArgs2" <| fun d ->
            serverHub.FiveArgs2("abc", 123, 1., "fff", 42) |> ondone (fun () -> calldone d)
        ait "sendList" <| fun d ->
            serverHub.SendList([|1; 2; 3|]) |> ondone (fun () -> calldone d)
        ait "sendArray" <| fun d ->
            serverHub.SendArray([|1;2;3|]) |> ondone (fun () -> calldone d)
//        ait "sendSeq" <| fun d ->
//            serverHub.SendSeq([|1;2;3|]) |> ondone (fun () -> calldone d)
        
        ait "complex" <| fun d ->
            let x = Types.``TestServer+ComplexObject``()
            x.Text <- "Hello"
            x.Number <- 42
            serverHub.Complex(x) |> ondone (fun () -> calldone d)

    describe "Callbacks" <| fun () ->
        ait "SendList" <| fun d ->
            called := (fun s -> Globals.console.log s
                                if s = "SendList" 
                                then calldone d 
                                else ())
            serverHub.SendList([|1;2;3|]) |> ondone id
            
        ait "SendArray" <| fun d ->
            called := (fun s -> Globals.console.log s
                                if s = "SendArray" 
                                then calldone d 
                                else ())
            serverHub.SendArray([|1;2;3|]) |> ondone id

    // TODO: test callbacks come with correct arguments by not using called: string -> unit !

[<EntryPoint>]
let main argv = 
    let script = Compiler.compileWithoutReturn <@ tests() @>
    File.WriteAllText("..\..\..\Server\AllTests.js", script)
    0 // return an integer exit code
