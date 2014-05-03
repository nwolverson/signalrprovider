namespace SignalRServer

open Microsoft.AspNet.SignalR
open Microsoft.AspNet.SignalR.Hubs

open System
open System.Runtime
open System.Reactive.Concurrency
open System.Reactive.Linq
open System.Web
open System.Web.Routing

open Dynamic
open TaskHelper
open System.Threading.Tasks

module Common =
    type IMyHub =
        abstract member MyCustomServerFunction: string -> unit

module MyServer =
    open Common

    //SignalR supports two kinds of connections: Hub and PersistentConnection

    type MyConnection() as this = 
        inherit PersistentConnection()
        override x.OnConnected(req,id) =
            this.Connection.Send(id, "Welcome!") |> ignore
            base.OnConnected(req,id)
        override x.OnReceived(req,id,data) =
            this.Connection.Send(id, "Cheers for " + data) |> ignore
            base.OnReceived(req,id,data)

    

    [<HubName("myhub")>]
    type MyHub() as this = 
        inherit Hub()

        interface IMyHub with
            member x.MyCustomServerFunction s = this.MyCustomServerFunction s

        member x.MyCustomServerFunction(fromClient : string) : unit =
                let (t:Task) = this.Clients.Caller?myCustomClientFunction("Cheers for '" + fromClient + "'")
                t.Wait()

        member this.functionWith3Args(x : int, y: string, z: obj) = 42.0

        override x.OnConnected() =
            base.OnConnected()
        // Define the MyCustomServerFunction on the server.
        

    let sendAll msg = 
            GlobalHost.ConnectionManager.GetConnectionContext<MyConnection>().Connection.Broadcast(msg) 
                |> Async.awaitPlainTask |> ignore
            GlobalHost.ConnectionManager.GetHubContext<MyHub>().Clients.All?myCustomClientFunction(msg)
                |> Async.awaitPlainTask |> ignore
            

    //Send some responses from server to client...
    let SignalRCommunicationSendPings() =

        //Send first message
        sendAll("Hello world!")

        //Send ping on every 5 seconds
        let pings = Observable.Interval(TimeSpan.FromSeconds(5.0), Scheduler.Default)
        pings.Subscribe(fun s -> sendAll("ping!")) |> ignore

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
            Owin.OwinExtensions.MapSignalR<MyConnection>(app, "/signalrConn") |> ignore

            Owin.OwinExtensions.MapSignalR(app, "/signalrHub", config) |> ignore
            //SignalRCommunicationSendPings()
            ()

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
