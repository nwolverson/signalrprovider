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
open SignalRProviderRuntime

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
type ClientProvider (config: TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces ()

    let ns = "SignalRProvider.Hubs"
    let asm = Assembly.GetExecutingAssembly()

    let typeInfo = getTypes (config.ReferencedAssemblies |> Seq.filter (fun a -> a.EndsWith("Server.dll")) |> List.ofSeq) // TODO

    let typeNs = "SignalRProvider.Types"
    let types = new System.Collections.Generic.List<ProvidedTypeDefinition>()

    let makeMethodType hubName (name, (args: (string * StructuredType) list, ret: StructuredType)) =
        
        let rec getTy (ty: StructuredType) = 
            match ty with
            | Simple(t) -> Type.GetType(t)
            | Complex(typeName, l) ->
                let newTypeName = typeName.Replace('.', '!') // collapse namespaces but keep the full name
                let typeDef = ProvidedTypeDefinition(asm, typeNs, newTypeName, Some typeof<obj>) 

                let setMi = typeof<JsonObject>.GetMethod("Set")

                typeDef.AddMembers(l |> List.map (fun (pName, pTy) -> 
                    let pType = getTy pTy
                    let set = setMi.MakeGenericMethod(pType)
                    let p = ProvidedProperty(pName, 
                                            pType,
                                            SetterCode = (fun [ob; newVal] ->
                                                Expr.Call(set, [ob; Expr.Value(pName); newVal])),
                                            GetterCode = (fun [ob] -> 
                                               <@@ () @@>))
                                               // <@@ JsonObject.Get (%%ob: obj) pName @@>))
                    p))
                typeDef.AddMember <| ProvidedConstructor([], InvokeCode =  (fun args -> <@@ JsonObject.Create() @@>))

                types.Add(typeDef)
                upcast typeDef


        let args = args |> List.map (fun (name, ty) -> ProvidedParameter(name, getTy ty))
        let returnType = 
            match ret with
            | Simple(t) when t = typeof<unit>.FullName || t = typeof<System.Void>.FullName -> typeof<unit>
            | _ -> getTy(ret)

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

    let definedTypes = typeInfo |> List.map makeHubType

    do
        this.RegisterRuntimeAssemblyLocationAsProbingFolder(config)
        this.AddNamespace(ns, definedTypes)
        this.AddNamespace(typeNs, types |> List.ofSeq)


[<assembly:TypeProviderAssembly>]
do ()