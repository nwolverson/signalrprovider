module SignalRProvider

open System.IO
open ProviderImplementation.ProvidedTypes

open Microsoft.FSharp.Core.CompilerServices
open System.Reflection
open System
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Quotations.DerivedPatterns

open FunScript.TypeScript
open Microsoft.AspNet.SignalR.Hubs
open Microsoft.AspNet.SignalR
open ReflectionProxy

[<TypeProvider>]
type ClientProvider (config: TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces ()

    let ns = "SignalRProvider.Hubs"
    let asm = Assembly.GetExecutingAssembly()

    let appDomain = AppDomain.CreateDomain("signalRprovider", null, new AppDomainSetup(ShadowCopyFiles="false",DisallowApplicationBaseProbing=true))
    let dll = typeof<ReflectionProxy>.Assembly.Location
    
    let obj = appDomain.CreateInstanceFromAndUnwrap(dll, typeof<ReflectionProxy>.FullName) 

    let handler = ResolveEventHandler (fun _ _ -> Assembly.LoadFrom dll)
    do AppDomain.CurrentDomain.add_AssemblyResolve handler

    let rp = obj :?> ReflectionProxy
    let ret = rp.GetDefinedTypes(config.ReferencedAssemblies)

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

    let typeInfo = ret
    let definedTypes = typeInfo |> List.map makeHubType

    do
        this.RegisterRuntimeAssemblyLocationAsProbingFolder(config)
        this.AddNamespace(ns, definedTypes)

[<assembly:TypeProviderAssembly>]
do ()