//SignalR:
#r "../packages/Newtonsoft.Json.5.0.8/lib/net45/Newtonsoft.Json.dll"
#r "../packages/Microsoft.AspNet.SignalR.Core.2.0.0/lib/net45/Microsoft.AspNet.SignalR.Core.dll"

//Reactive Extensions:
#r "System.ComponentModel.Composition.dll"
#r "../packages/Rx-Interfaces.2.1.30214.0/lib/Net45/System.Reactive.Interfaces.dll"
#r "../packages/Rx-Core.2.1.30214.0/lib/Net45/System.Reactive.Core.dll"
#r "../packages/Rx-Linq.2.1.30214.0/lib/Net45/System.Reactive.Linq.dll"

//Dynamic:
#r "Microsoft.CSharp.dll"
#load "Dynamic.fs"

//ASP.NET Web Application routing (method SetupRouting):
#r "System.Web"
#r "../packages/Microsoft.AspNet.SignalR.SystemWeb.2.0.0/lib/net45/Microsoft.AspNet.SignalR.SystemWeb.dll"

//OWIN host:
#r "../packages/Owin.1.0/lib/net40/Owin.dll"
#r "../packages/Microsoft.Owin.Hosting.2.0.1/lib/net45/Microsoft.Owin.Hosting.dll"
//#r "../packages/Microsoft.Owin.Host.SystemWeb.2.0.1/lib/net45/Microsoft.Owin.Host.SystemWeb.dll"
//#r "../packages/Microsoft.AspNet.SignalR.Owin.1.1.1/lib/net45/Microsoft.AspNet.SignalR.Owin.dll"
#r "../packages/Microsoft.Owin.Host.HttpListener.2.0.1/lib/net45/Microsoft.Owin.Host.HttpListener.dll"
#r "../packages/Microsoft.Owin.Security.2.0.1/lib/net45/Microsoft.Owin.Security.dll"
#r "../packages/Microsoft.Owin.2.0.1/lib/net45/Microsoft.Owin.dll"


#load "TaskHelper.fs"


#load "Program.fs"
open SignalRServer.MyServer

//Let's start Owin-server on F#-interactive!
main ()

//----------------------------------------------------------------------------------------------------------------------
//This sends a message to all clients
sendAll "hello"

//This would send "ping!" to all clients every 5 seconds
SignalRCommunication()
