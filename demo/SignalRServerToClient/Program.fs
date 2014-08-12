namespace SignalRServerToClient

open SignalRProvider.ClientHubs

open Dynamic
open TaskHelper
open SignalRProvider.ClientHubs

module ClientHubConsumer =
    open System.Threading.Tasks

    let myTypedFunction s =
        ClientHub.myTypedFunction 41
        ClientHub.myTypedFunction42 43
        ClientHub.myCustomClientFunction <| "Broadcast message: " + s
        //ClientHub.functionWith2Args(1, "two")
