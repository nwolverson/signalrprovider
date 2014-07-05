
// *** NOTE On Mac and Linux you may need to manually edit the project file to use 
// *** NOTE v9.0/WebApplications/Microsoft.WebApplication.targets instead of v10.0

namespace TestFMVC

open System
open System.Web
open System.Web.Mvc
open System.Web.Routing

type Route = { 
    controller : string
    action : string
    id : UrlParameter }

type Global() =
    inherit System.Web.HttpApplication() 

    static member RegisterRoutes(routes:RouteCollection) =
        routes.IgnoreRoute("{resource}.axd/{*pathInfo}")
        routes.MapRoute(
            "Default", // Route name
            "{controller}/{action}/{id}", // URL with parameters
            { controller = "Home"; action = "Index"; id = UrlParameter.Optional } // Parameter defaults
            )

    member x.Start() =
        AreaRegistration.RegisterAllAreas()
        Global.RegisterRoutes(RouteTable.Routes)

