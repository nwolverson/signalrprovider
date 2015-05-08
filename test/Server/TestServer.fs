module TestServer

open Microsoft.AspNet.SignalR
open Microsoft.AspNet.SignalR.Hubs
    
type ComplexObject() =
    member val Number = 0 with get, set
    member val Text = "" with get, set

type IMyAwesomeClientHub =
    abstract member FiveArgs : string -> int -> float -> string -> int -> unit
    abstract member FiveArgsTupled : string * int * float * string * int -> unit
    abstract member SendList : int list -> unit
    abstract member SendArray : int[] -> unit
    abstract member SendSeq : int seq -> unit
    abstract member Complex : ComplexObject -> unit

[<HubName("myServerHub")>]
type MyHub() = 
    inherit Hub<IMyAwesomeClientHub>()

    // same types as client hub so can round trip with same args

    member this.FiveArgs a b c d e =
        this.Clients.Caller.FiveArgs a b c d e
        ()

    member this.FiveArgs2(a,b,c,d,e) =
        this.Clients.Caller.FiveArgsTupled(a,b,c,d,e)
        ()

    member this.SendList l =
        this.Clients.Caller.SendList l

    member this.SendArray arr =
        this.Clients.Caller.SendArray arr

    member this.SendSeq seq =
        this.Clients.Caller.SendSeq seq

    member this.Complex obj =
        this.Clients.Caller.Complex obj

    member this.Complex2 (obj : ComplexObject) =
        let obj2 = ComplexObject(Number = obj.Number * 2, Text = obj.Text + "..." )
        this.Clients.Caller.Complex obj2

    member this.CallEverything () =
        this.Clients.All.FiveArgs "string" 123 1. "xxx" 42
        this.Clients.All.SendList [0; 1; 2; 3; 4]
        this.Clients.All.SendArray [|0; 1; 2; 3; 4|]
        this.Clients.All.SendSeq <| Seq.init 5 (fun x -> x)
        this.Clients.All.Complex <| ComplexObject(Number = 123, Text = "123!" )

        ()
