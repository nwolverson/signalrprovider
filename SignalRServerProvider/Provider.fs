namespace SignalRServerProvider


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


[<TypeProvider>]
type ServerProvider (config: TypeProviderConfig) as this=
    inherit TypeProviderForNamespaces ()

    let ns = "SignalRProvider.ClientHubs"
    let asm = Assembly.GetExecutingAssembly()

    let tyInfo = SignalRProvider.getTypes(config.ReferencedAssemblies)
    do failwith (tyInfo |> List.length |> string)

    let ty = ProvidedTypeDefinition(asm, ns, "TestType", Some typeof<obj>)
    let ctor = ProvidedConstructor(parameters = [], InvokeCode = (fun args -> <@@ new obj() @@>))
    do ty.AddMember(ctor)
    do
        this.RegisterRuntimeAssemblyLocationAsProbingFolder(config)
        this.AddNamespace(ns, [ty])


[<assembly:TypeProviderAssembly>]
do ()