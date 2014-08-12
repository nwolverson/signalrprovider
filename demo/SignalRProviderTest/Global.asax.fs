namespace SignalRProviderTest

open System
open System.Net.Http
open System.Web
open System.Web.Mvc
open System.Web.Routing
open Microsoft.AspNet.SignalR

/// Route for ASP.NET MVC applications
type Route = { 
    controller : string
    action : string
    id : UrlParameter }

type Global() =
    inherit System.Web.HttpApplication() 

    static member RegisterFilters(filters: GlobalFilterCollection) =
        filters.Add(new HandleErrorAttribute())

    static member RegisterRoutes(routes:RouteCollection) =
        routes.IgnoreRoute("{resource}.axd/{*pathInfo}")
        routes.MapRoute(
            "Default", // Route naem
            "{controller}/{action}/{id}", // URL with parameters
            { controller = "Home"; action = "Index"; id = UrlParameter.Optional } // Parameter defaults
        ) |> ignore

    member x.Application_Start() =
        AreaRegistration.RegisterAllAreas()
        Global.RegisterFilters(GlobalFilters.Filters)
        Global.RegisterRoutes(RouteTable.Routes)

        let makeHub() = 
            let hub = new SignalRServer.MyServer.MyHub(12345, SignalRServerToClient.ClientHubConsumer.myTypedFunction) 
            System.Diagnostics.Debug.WriteLine "created hub in makeHub"
            hub :> obj
        
        GlobalHost.DependencyResolver.Register(typeof<SignalRServer.MyServer.MyHub>, makeHub)
