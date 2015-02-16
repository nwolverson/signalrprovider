namespace SignalRServer

open Microsoft.AspNet.SignalR
open Microsoft.AspNet.SignalR.Hubs

module MyServer =
    type ComplexObject() =
        member val Number = 0 with get, set
        member val Text = "" with get, set

    type IMyAwesomeClientHub =
        abstract member BroadcastMessage : string -> unit
        abstract member FiveArgs : string -> int -> float -> string -> int -> unit
        abstract member FiveArgsTupled : string * int * float * string * int -> unit
        abstract member SendList : int list -> unit
        abstract member SendArray : int[] -> unit
        abstract member SendSeq : int seq -> unit

    [<HubName("myServerHub")>]
    type MyHub() = 
        inherit Hub<IMyAwesomeClientHub>()
        
        member this.SendMessage(text : string) : string =
            this.Clients.Others.BroadcastMessage(text)
            this.Clients.Caller.SendList([1; 2; 3])
            this.Clients.Caller.SendArray([|1; 2; 3; 4|])
            this.Clients.Caller.SendSeq( Seq.ofList [ 1; 2; 3; 4; 5 ])
            "Message sent"

        member this.functionWith3Args(x : int, y: string, z: int) = 
            this.Clients.Caller.FiveArgs y x 3.4 y x
            42.0 

        member this.functionWith4Args(xx : int, y: string, z: ComplexObject, a: int) = 
            this.Clients.Caller.FiveArgsTupled("abc"+y, xx, 42.12345, a.ToString(), z.Number)
            xx * a + z.Number

        override this.OnConnected() =
            base.OnConnected()

    let config = new HubConfiguration(EnableDetailedErrors = true)

    type MyWebStartup() =
        member x.Configuration(app:Owin.IAppBuilder) =
            Owin.OwinExtensions.MapSignalR(app, "/signalrHub", config) |> ignore
    
    type CorsWebStartup() =
        member x.Configuration(app:Owin.IAppBuilder) =
            Owin.CorsExtensions.UseCors(app, Microsoft.Owin.Cors.CorsOptions.AllowAll) |> ignore
            Owin.OwinExtensions.MapSignalR(app, "/signalrHub", config) |> ignore

    [<assembly: Microsoft.Owin.OwinStartup(typeof<MyWebStartup>)>]
    do()

