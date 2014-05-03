namespace SignalRProviderTest.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Web
open System.Web.Mvc
open System.Web.Mvc.Ajax

type HomeController() =
    inherit Controller()
    member this.Index () = this.View()
    member this.Script () = 
        let script = (new SignalRClient.Wrapper()).GenerateScript()
        new JavaScriptResult(Script = script)

