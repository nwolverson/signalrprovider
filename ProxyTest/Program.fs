// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

open System
open System.Reflection

[<EntryPoint>]
let main argv = 
    printfn "%A" argv

    let appDomain = AppDomain.CreateDomain("proxyTestDomain", null, new AppDomainSetup(ShadowCopyFiles="false",DisallowApplicationBaseProbing=true))
    
    let dll = @"C:\Users\Nicholas\Documents\git\signalrprovider\SignalRProvider\bin\Debug\ReflectionProxy.dll"
    let ty = "ReflectionProxy.ReflectionProxy"
   
    let resolve (o: obj) (args: ResolveEventArgs) = 
            Assembly.LoadFrom dll

    AppDomain.CurrentDomain.add_AssemblyResolve (ResolveEventHandler resolve)

    let reflectionProxyRaw = appDomain.CreateInstanceFromAndUnwrap(dll, ty) 
    let t = reflectionProxyRaw.GetType()
    let hi = t.InvokeMember("Hello", System.Reflection.BindingFlags.InvokeMethod, Type.DefaultBinder, reflectionProxyRaw, [| |]) 

    let arg = [ "a"; "b"; "c" ] |> Seq.ofList
    let getTys = t.InvokeMember("GetDefinedTypes", System.Reflection.BindingFlags.InvokeMethod, Type.DefaultBinder, reflectionProxyRaw, [| arg |]) 

    AppDomain.CurrentDomain.remove_AssemblyResolve (ResolveEventHandler resolve)
    
    do AppDomain.Unload(appDomain)

    0 // return an integer exit code
