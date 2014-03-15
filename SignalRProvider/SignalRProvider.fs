module SignalRProvider

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection

[<TypeProvider>]
type ClientProvider (config: TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces ()

    let ns = "SignalRProvider.Hubs"
    let asm = Assembly.GetExecutingAssembly()

    let myType = ProvidedTypeDefinition(asm, ns, "MyType", Some typeof<obj>)

    // TODO - use all?
    let clientAsm =
        let lastref = config.ReferencedAssemblies |> Seq.last
        let refAsm = Assembly.LoadFrom lastref
        refAsm

    let hubs = 
        clientAsm.DefinedTypes
        |> Seq.filter (fun t -> t.GetCustomAttributes<Microsoft.AspNet.SignalR.Hubs.HubNameAttribute>() |> Seq.isEmpty |> not )

    let hubName (hubType : TypeInfo) =
        hubType.GetCustomAttribute<Microsoft.AspNet.SignalR.Hubs.HubNameAttribute>().HubName

    let makeMethod (hubName : string) (mi: MethodInfo) =
        let name = mi.Name
        let parms = mi.GetParameters() |> Seq.map (fun p -> ProvidedParameter(p.Name, (* p.ParameterType *) typeof<obj>))
        let meth = ProvidedMethod(name, parms |> List.ofSeq, typeof<unit>) // all  obj for now
         //p.ReturnType)
        
        meth.InvokeCode <- (fun args -> 
            let argsArray = 
                args 
                |> Seq.skip 1 
                |> Seq.fold (fun s x -> <@@ (%%x)::(%%s) @@> ) <@@ [] @@> 
            let code = 
                <@@ 
                    let conn = ( %%args.[0] : obj) :?> HubConnection
                    let proxy = conn.createHubProxy(hubName)
                    let arguments = (%%argsArray : obj list) |> Array.ofList
                    proxy.invoke(name, (arguments : obj array) ) |> ignore
                @@>
            code)

        meth

    let makeHubType hubType =
        let name = hubName hubType
        let props = 
            Microsoft.AspNet.SignalR.Hubs.ReflectionHelper.GetExportedHubMethods hubType
            |> Seq.map (makeMethod name)  //GetterCode = (fun args -> <@@ "Hello world " @@>)))
        let ty = ProvidedTypeDefinition(asm, ns, name, Some typeof<obj>)
        let ctor = ProvidedConstructor(parameters = [ ProvidedParameter("conn", typeof<HubConnection>) ], 
                    InvokeCode = (fun args -> <@@ (%%(args.[0]) : HubConnection) :> obj @@>))
        ty.AddMember ctor
        props |> Seq.iter (fun prop -> ty.AddMember prop)
        ty

    let hubTypes = 
        hubs
        |> Seq.map makeHubType
        |> Seq.toList

    do
        this.RegisterRuntimeAssemblyLocationAsProbingFolder(config)
        this.AddNamespace(ns, hubTypes)

[<assembly:TypeProviderAssembly>]
do ()