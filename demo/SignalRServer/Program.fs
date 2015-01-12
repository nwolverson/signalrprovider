namespace SignalRServer

open Microsoft.AspNet.SignalR
open Microsoft.AspNet.SignalR.Hubs

module MyServer =
    type ComplexObject() =
        member val Number = 0 with get, set
        member val Text = "" with get, set

    type IMyHubClient =
        abstract member BroadcastMessage : string -> unit

    [<HubName("myhub")>]
    type MyHub() = 
        inherit Hub<IMyHubClient>()
        
        member this.SendMessage(text : string) : string =
            this.Clients.Others.BroadcastMessage(text)
            "Message sent"

        member this.functionWith3Args(x : int, y: string, z: int) = 42.0 
        member this.functionWith4Args(xx : int, y: string, z: ComplexObject, a: int) = xx * a + z.Number

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

