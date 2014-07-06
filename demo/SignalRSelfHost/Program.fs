// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

open System
open System.Runtime
open System.Web

open Microsoft.Owin
open Microsoft.Owin.Hosting

open Dynamic
open TaskHelper
open System.Threading.Tasks
open Microsoft.Owin.Hosting

open Microsoft.AspNet.SignalR



[<EntryPoint>]
let main argv = 
    // use CORS and host html/js on a different port via whatever web server
    let url = "http://localhost:8080"
    // Here you would need new empty C#-class just for configuration: ServerStartup.MyWebStartup:
    use app =  WebApp.Start<SignalRServer.MyServer.CorsWebStartup>(url) 
    Console.WriteLine ("Server running on " + url)
    Console.ReadLine() |> ignore
    app.Dispose()
    Console.WriteLine "Server closed."
    0
