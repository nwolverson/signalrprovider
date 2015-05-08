namespace SignalRServer

open Microsoft.AspNet.SignalR
open Microsoft.AspNet.SignalR.Hubs

module MyServer =
    type IChatClientHub =
        abstract member BroadcastMessage : string -> unit

    [<HubName("myServerHub")>]
    type MyHub() = 
        inherit Hub<IChatClientHub>()
        
        member this.SendMessage(text : string) : string =
            this.Clients.All.BroadcastMessage(text)
            "Message sent"

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

