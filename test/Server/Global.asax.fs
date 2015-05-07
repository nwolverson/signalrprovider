namespace Server

open System
open System.Net.Http
open System.Web
open System.Web.Http
open System.Web.Routing

open Microsoft.AspNet.SignalR
open Microsoft.AspNet.SignalR.Hubs

type Global() =
    inherit System.Web.HttpApplication() 

module TestServer =
    open System.Threading.Tasks

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

        member this.functionWith3Args(x : int, y: string, z: int) = 
           this.Clients.Caller.FiveArgs y x 3.4 y x 
           42.0

        member this.functionWith4Args(xx : int, y: string, z: ComplexObject, a: int) = 
            this.Clients.Caller.FiveArgsTupled(y+"|"+z.Text, xx, 42.12345, a.ToString(), z.Number)
            xx * a + z.Number

        override this.OnConnected() =
            base.OnConnected()

    type Startup() =
        member x.Configuration(app:Owin.IAppBuilder) =
            let config = new HubConfiguration(EnableDetailedErrors = true)
            Owin.OwinExtensions.MapSignalR(app, "/signalrHub", config) |> ignore

    [<assembly: Microsoft.Owin.OwinStartup(typeof<Startup>)>]
    do()

