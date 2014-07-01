module SignalRProvider

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection
open System

open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Quotations.DerivedPatterns

open Microsoft.AspNet.SignalR.Hubs
open Microsoft.AspNet.SignalR

open System.IO

open ReflectionProxy

open FunScript.TypeScript


[<TypeProvider>]
type ClientProvider (config: TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces ()

    let ns = "SignalRProvider.Hubs"
    let asm = Assembly.GetExecutingAssembly()

    let myType = ProvidedTypeDefinition(asm, ns, "MyType", Some typeof<obj>)

    let loadAssembliesBytes() =
        let ass = config.ReferencedAssemblies |> Seq.last

        let resolve (o: obj) (args: ResolveEventArgs) = 
            let name = AssemblyName(args.Name).Name
            try
                Assembly.Load args.Name
            with
                | _ -> Assembly.LoadFrom <| Path.Combine(Path.GetDirectoryName ass, name + ".dll")

        AppDomain.CurrentDomain.add_AssemblyResolve (ResolveEventHandler resolve)
        
        ass
        |> File.ReadAllBytes 
        |> Assembly.Load

    
    let hubAttrs (t: TypeInfo) = 
        CustomAttributeData.GetCustomAttributes(t)
        |> Seq.filter (fun attr -> attr.AttributeType.FullName = "Microsoft.AspNet.SignalR.Hubs.HubNameAttribute")

    let arg = config.ReferencedAssemblies |> Seq.ofArray

    let dir1 = config.ReferencedAssemblies |> Seq.last |> Path.GetDirectoryName
    let dirCode = Assembly.GetExecutingAssembly().Location |> Path.GetDirectoryName
    let appDomain = AppDomain.CreateDomain("signalRprovider", null, new AppDomainSetup(ShadowCopyFiles="false",DisallowApplicationBaseProbing=true))
    
    let dll = Path.Combine(dirCode, "ReflectionProxy.dll")
    let cls = "ReflectionProxy.ReflectionProxy"
    let obj = appDomain.CreateInstanceFromAndUnwrap(dll, cls) 

    let resolve (o: obj) (args: ResolveEventArgs) = Assembly.LoadFrom dll

    let handler = ResolveEventHandler resolve
    do AppDomain.CurrentDomain.add_AssemblyResolve handler

    let ret = obj.GetType().InvokeMember("GetDefinedTypes", System.Reflection.BindingFlags.InvokeMethod, Type.DefaultBinder, obj, [| arg |]) 

    do AppDomain.CurrentDomain.remove_AssemblyResolve handler
    do AppDomain.Unload(appDomain)

    let makeMethodType hubName (name, args, ret) =
        let args = args |> Seq.map (fun (name, ty) -> ProvidedParameter(name, Type.GetType(ty)))
        let returnType = 
            if (typeof<unit>.FullName = ret) then typeof<unit>
            else if (typeof<System.Void>.FullName = ret) then typeof<unit>
            else Type.GetType(ret)

        let deferType = typedefof<JQueryDeferred<_>>.MakeGenericType(returnType)

        let objDeferType = typeof<JQueryDeferred<obj>>

        let meth = ProvidedMethod(name, args |> List.ofSeq, objDeferType)

        let castParam (e: Expr) = Expr.Coerce(e, typeof<obj> )

        //let unbox = match <@ 1 :> obj :?> int @> with Call(e, mi, es) -> mi

        meth.InvokeCode <- (fun args -> 
            let argsArray = Expr.NewArray(typeof<obj>, args |> Seq.skip 1 |> Seq.map castParam |> List.ofSeq)

            let objExpr = <@@ let conn = ( %%args.[0] : obj) :?> HubConnection
                              conn.createHubProxy(hubName) @@>

            let invokeExpr = <@@ (%%objExpr : HubProxy).invokeOverload2(name, (%%argsArray: obj array)) @@> 

            invokeExpr)
            
        meth

    let makeHubType (name, methodTypeInfo) =
        let methodDefinedTypes = methodTypeInfo |> Seq.map (makeMethodType name)
        
        let ty = ProvidedTypeDefinition(asm, ns, name, Some typeof<obj>)
        let ctor = ProvidedConstructor(parameters = [ ProvidedParameter("conn", typeof<HubConnection>) ], 
                    InvokeCode = (fun args -> <@@ (%%(args.[0]) : HubConnection) :> obj @@>))
        ty.AddMember ctor
        Seq.iter ty.AddMember methodDefinedTypes 
        ty

    // Ugh.. not taking a dependency on the DLL
    let typeInfo = ret :?> list<string * list<string * list<string * string> * string>>

    let definedTypes = typeInfo |> List.map makeHubType

    do
        this.RegisterRuntimeAssemblyLocationAsProbingFolder(config)
        this.AddNamespace(ns, definedTypes)

[<assembly:TypeProviderAssembly>]
do ()