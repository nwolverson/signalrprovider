module SignalRProvider

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection

[<TypeProvider>]
type ClientProvider (config: TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces ()

    let ns = "SignalRProvider.Provided"
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

    let makeMethod (p: MethodInfo) =
        let parms = p.GetParameters() |> Seq.map (fun p -> ProvidedParameter(p.Name, p.ParameterType))
        let meth = ProvidedMethod(p.Name, parms |> List.ofSeq, typeof<string>)//p.ReturnType)
        meth.AddMethodAttrs(MethodAttributes.Static)
        meth.InvokeCode <- (fun args -> <@@ String.concat "|" [ ( %%args.[0] ).ToString(); ( %%args.[1] ).ToString() ]  @@> )
        meth

    let makeHubType hubType =
        let name = hubName hubType
        let props = 
            hubType.DeclaredMethods
            |> Seq.map makeMethod  //GetterCode = (fun args -> <@@ "Hello world " @@>)))
        let ty = ProvidedTypeDefinition(asm, ns, name, Some typeof<obj>)
        props |> Seq.iter (fun prop -> ty.AddMember prop)
        ty

    let hubTypes = 
        hubs
        |> Seq.map makeHubType
        |> Seq.toList

    do
        this.AddNamespace(ns, hubTypes)

[<assembly:TypeProviderAssembly>]
do ()