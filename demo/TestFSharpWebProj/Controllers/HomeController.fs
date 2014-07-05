namespace TestFSharpWebProj.Controllers

// *** NOTE On Mac and Linux you may need to manually edit the project file to use 
// *** NOTE v9.0/WebApplications/Microsoft.WebApplication.targets instead of v10.0

open System
open System.Collections.Generic
open System.Linq
open System.Web
open System.Web.Mvc
open System.Web.Mvc.Ajax

type HomeController() =
    inherit Controller()
    member this.Index () =
        base.ViewData.["Message"] <- "Welcome to F# ASP.NET MVC on Mono!"
        this.View() :> ActionResult

