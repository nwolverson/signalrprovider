namespace Server

open Microsoft.AspNet.SignalR

type Global() =
    inherit System.Web.HttpApplication() 

type Startup() =
    member x.Configuration(app:Owin.IAppBuilder) =
        let config = new HubConfiguration(EnableDetailedErrors = true)
        Owin.OwinExtensions.MapSignalR(app, "/signalrHub", config) |> ignore

[<assembly: Microsoft.Owin.OwinStartup(typeof<Startup>)>]
do()

