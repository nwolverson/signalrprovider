namespace SignalRServer

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
    type ComplexObject() =
        member val Number = 0 with get, set
        member val Text = "" with get, set

    [<HubName("myhub")>]
    type MyHub() as this = 
        inherit Hub()

        member x.MyCustomServerFunction(fromClient : string) : unit =
                let (t:Task) = this.Clients.Caller?myCustomClientFunction("Cheers for '" + fromClient + "'")
                t.Wait()

        member this.functionWith3Args(x : int, y: string, z: int) = 42.0 
        member this.functionWith4Args(xx : int, y: string, z: ComplexObject, a: int) = 42

        member this.testUpdating3() = false

        member this.complexArg(arg: ComplexObject) = arg.Number

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
            Owin.CorsExtensions.UseCors(app, Microsoft.Owin.Cors.CorsOptions.AllowAll) |> ignore
            Owin.OwinExtensions.MapSignalR(app, "/signalrHub", config) |> ignore
            

    [<assembly: Microsoft.Owin.OwinStartup(typeof<MyWebStartup>)>]
    do()

