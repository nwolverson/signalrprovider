module SignalRServerProvider.Provider


open System.IO

open ProviderImplementation.ProvidedTypes

open Microsoft.FSharp.Core.CompilerServices
open System.Reflection
open System
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Quotations.DerivedPatterns

open ReflectionProxy
open SignalRProviderRuntime
open Microsoft.AspNet.SignalR

open Dynamic
open System.Threading.Tasks

let getTypes assemblies = 
    let appDomain = AppDomain.CreateDomain("signalRprovider", null, new AppDomainSetup(ShadowCopyFiles="false",DisallowApplicationBaseProbing=true))
    let dll = typeof<ReflectionProxy>.Assembly.Location
    
    let obj = appDomain.CreateInstanceFromAndUnwrap(dll, typeof<ReflectionProxy>.FullName) 

    let handler = ResolveEventHandler (fun _ _ -> Assembly.LoadFrom dll)
    do AppDomain.CurrentDomain.add_AssemblyResolve handler

    let rp = obj :?> ReflectionProxy
    let ret = rp.GetDefinedTypes(assemblies)

    do AppDomain.CurrentDomain.remove_AssemblyResolve handler
    do AppDomain.Unload(appDomain)

    ret

[<TypeProvider>]
type ServerProvider (config: TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces ()

    let ns = "SignalRProvider.ClientHubs"
    let asm = Assembly.GetExecutingAssembly()

    let tyInfo = getTypes(config.ReferencedAssemblies |> Seq.filter (fun a -> a.Contains("Client")) |> List.ofSeq) // TODO
    let typeNs = "SignalRProvider.Types"
    let types = new System.Collections.Generic.List<ProvidedTypeDefinition>()

    let makeMethodType hubName (methodName, (args: (string * StructuredType) list, ret: StructuredType)) =
        let rec getTy (ty: StructuredType) = 
            match ty with
            | Simple(t) -> Type.GetType(t)
            | Complex(typeName, l) ->
                failwith "Complex types not in yet"
                let newTypeName = typeName.Replace('.', '!') // collapse namespaces but keep the full name
                let typeDef = ProvidedTypeDefinition(asm, typeNs, newTypeName, Some typeof<obj>) 

                typeDef.AddMembers(l |> List.map (fun (pName, pTy) -> 
                    let pType = getTy pTy
                    let p = ProvidedProperty(pName, pType,
                                            SetterCode = (fun [ob; newVal] -> <@@ () @@>),
                                            GetterCode = (fun [ob] -> <@@ () @@>))
                    p))
                typeDef.AddMember <| ProvidedConstructor([], InvokeCode =  (fun args -> <@@ new obj() @@>))

                types.Add(typeDef)
                upcast typeDef

        let args = args |> List.map (fun (name, ty) -> ProvidedParameter(name, getTy ty))
        // TODO Don't think this can be anything useful. Maybe throw an error if not unit? or ignore.
        let returnType = 
            match ret with
            | Simple(t) when t = typeof<unit>.FullName || t = typeof<System.Void>.FullName -> typeof<unit>
            | _ -> getTy(ret)

        let meth = ProvidedMethod(methodName, args |> List.ofSeq, typeof<unit>, IsStaticMethod = true)

        let name = typeof<Microsoft.AspNet.SignalR.GlobalHost>.AssemblyQualifiedName
        let all = 
            <@@ 
            
            let globalHost = Type.GetType(name)
            if (globalHost = null) then
                failwith "host null"
            else if globalHost.GetProperty("ConnectionManager") = null then
                failwith "property null"
            else
                let manager = globalHost.GetProperty("ConnectionManager").GetValue(null)
                let ctxMethTy = manager.GetType().GetMethods() |> Seq.where (fun m -> m.Name = "GetHubContext" && m.IsGenericMethod = false) |> Seq.head
                let ctx = ctxMethTy.Invoke(manager, [| "myhub" |]) // TODO get server hub name
                let clients = ctx.GetType().GetProperty("Clients").GetValue(ctx)
                clients.GetType().GetProperty("All").GetValue(clients)
            @@>

        // TODO: Hardcoded args count. Both this fun, dynamicCall, and the client code...
        meth.InvokeCode <- (fun margs -> 
            let arg0 = margs.[0]
            let ty0 = args.[0].ParameterType

            let genericDynamicMi = typeof<DynamicTypeWrapper>.GetMethod("dynamicCall")
            let dynamicMi = genericDynamicMi.MakeGenericMethod(ty0, typeof<Task<obj>>)

            let call = Expr.Call(dynamicMi,
                                    [all; <@ methodName @>; arg0])
            <@@ (%%call : Task<obj>) |> ignore @@>)
            //<@@ dynamicCall (%%all) methodName (%%margs.[0] : int) |> ignore @@>) // todo hardwired to int!
        meth

    let makeHubType (name, methodTypeInfo) =
        let methodDefinedTypes = methodTypeInfo |> Seq.map (makeMethodType name)
        
        let ty = ProvidedTypeDefinition(asm, ns, name, Some typeof<obj>)

        Seq.iter ty.AddMember methodDefinedTypes 
        ty

    let definedTypes = tyInfo |> List.map makeHubType

    
    // TODO: 
    // 1. Make ClientHub.method for calls to 
    //   GlobalHost.ConnectionManager.GetHubContext<ServerHubType>().Clients.All.method
    // 2. Move down a level so we have 
    //      ClientHub.All.method => Clients.All.method
    //      ClientHub.Client(id).method => Clients.Client(id).method


    do
        this.RegisterRuntimeAssemblyLocationAsProbingFolder(config)
        this.AddNamespace(ns, definedTypes)


[<assembly:TypeProviderAssembly>]
do ()