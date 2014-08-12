namespace SignalRProviderRuntime

open FunScript

[<ReflectedDefinition>]
type JsonObject = 
    [<JSEmitInlineAttribute("({})")>]
    static member Create() = failwith "Funscript emit" : obj
    
    [<JSEmitInlineAttribute("(({0})[{1}] = {2})")>]
    static member Set<'x> (ob: obj) (propertyName: string) (value: 'x) = failwith "FunScript emit" : unit

    [<JSEmitInlineAttribute("(({0})[{1}]")>]
    static member Get (ob: obj) (propertyName: string) = failwith "FunScript emit" : 'x

type ClientHubAttribute() = 
    inherit System.Attribute()

    member this.HubName = "ClientHub"