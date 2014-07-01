﻿namespace SignalRServer

open Microsoft.AspNet.SignalR
open Microsoft.AspNet.SignalR.Hubs

open System
open System.Runtime
open System.Web
open System.Web.Routing

open Microsoft.Owin

open Dynamic
open TaskHelper
open System.Threading.Tasks

module MyServer =
    [<HubName("myhub")>]
    type MyHub() as this = 
        inherit Hub()

        member x.MyCustomServerFunction(fromClient : string) : unit =
                let (t:Task) = this.Clients.Caller?myCustomClientFunction("Cheers for '" + fromClient + "'")
                t.Wait()

        member this.functionWith3Args(x : int, y: string, z: obj) = 42.0 + 0.0
        member this.functionWith4Args(xx : int, y: string, z: obj, a: int) = 42

        member this.testUpdating3() = false

        override x.OnConnected() =
            base.OnConnected()
            

//------------------------------------------------------------------------------------------------------------
// Options of hosting:
// A) ASP.NET Web Application
// B) OWIN server: Command-line application

// Signal-R 1.1.3: ASP.NET Web Application. Setup routing here, this is called from Global.asax.cs
//    let SetupRouting() =
//        RouteTable.Routes.MapConnection<MyConnection>("signalrConn", "signalrConn") |> ignore
//        
//        let hubc = new HubConfiguration(EnableDetailedErrors = true, EnableCrossDomain = true)
//        RouteTable.Routes.MapHubs("/signalrHub", hubc) |> ignore
//        SignalRCommunicationSendPings()

// Signal-R 2.0: Use OWIN:
//    (If you use Silverlight client, then you would need to supply clientaccesspolicy.xml and crossdomain.xml)
    open Microsoft.Owin.Hosting
    let config = new HubConfiguration(EnableDetailedErrors = true)

    type MyWebStartup() =
        member x.Configuration(app:Owin.IAppBuilder) =
            Owin.OwinExtensions.MapSignalR(app, "/signalrHub", config) |> ignore
            

    [<assembly: Microsoft.Owin.OwinStartup(typeof<MyWebStartup>)>]
    do()

    // If you want to run this as console application, then uncomment EntryPoint-attribute and
    // from SignalRServer project properties change this application "Output Type" to: Console Application
    // (But then this will be .exe-file instead of dll-file and you can't reference it from 
    //  the current ASP.NET Web Application, project WebApp.)
    //[<EntryPoint>]
    let main argv = 
        //Note that server and client has to use the same port
        let url = "http://localhost:8080"
        // Here you would need new empty C#-class just for configuration: ServerStartup.MyWebStartup:
        use app =  WebApp.Start<MyWebStartup>(url) 
        Console.WriteLine "Server running..."
        Console.ReadLine() |> ignore
        app.Dispose()
        Console.WriteLine "Server closed."
        0
