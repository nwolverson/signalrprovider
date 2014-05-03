
#r @"C:\Users\Nicholas\Documents\git\signalrprovider\packages\Microsoft.AspNet.SignalR.Core.2.0.2\lib\net45\Microsoft.AspNet.SignalR.Core.dll"

open System.Reflection

let asm = Assembly.LoadFrom @"C:\Users\Nicholas\Documents\git\signalrproviderexample\SignalRServer\bin\Debug\SignalRServer.dll" 
 
let hubs = 
        asm.DefinedTypes
        |> Seq.filter (fun t -> t.GetCustomAttributes<Microsoft.AspNet.SignalR.Hubs.HubNameAttribute>() |> Seq.isEmpty |> not )

let hubName (hubType : TypeInfo) =
        hubType.GetCustomAttribute<Microsoft.AspNet.SignalR.Hubs.HubNameAttribute>().HubName


#load "ProvidedTypes/Code/ProvidedTypes.fs"

open Microsoft.FSharp.Core.CompilerServices

open ProviderImplementation.ProvidedTypes

#r @"C:\Users\Nicholas\Documents\git\signalrproviderexample\packages\FunScript.TypeScript.Binding.jquery.1.1.0.13\lib\net40\FunScript.TypeScript.Binding.jquery.dll"
#r @"C:\Users\Nicholas\Documents\git\signalrproviderexample\packages\FunScript.TypeScript.Binding.signalr.1.1.0.13\lib\net40\FunScript.TypeScript.Binding.signalr.dll"


let meths = Microsoft.AspNet.SignalR.Hubs.ReflectionHelper.GetExportedHubMethods (hubs |> Seq.head)

let mi = meths |> Seq.toList |> (fun l -> List.nth l 1)
let parms = mi.GetParameters() |> Seq.map (fun p -> ProvidedParameter(p.Name, (* p.ParameterType *) typeof<obj>))
let name = mi.Name
let meth = ProvidedMethod(name, parms |> List.ofSeq, typeof<unit>) // all  obj for now

open Microsoft.FSharp.Quotations

let testCode = [ <@@ 1 :> obj @@>; <@@ "2" :> obj @@>; <@@ 3.0 :> obj @@> ]

let testCodeTyped = [ <@@ 1 @@>; <@@ "2" @@>; <@@ 3.0 :> obj @@> ]

//Expr.Call(Expr.Value(null), mi, testCodeTyped)

 
let makeCode (args : Quotations.Expr list ) = 
    let argsArray = 
        args 
        |> Seq.skip 1 
        |> Seq.fold (fun s x -> <@@ (%%x)::(%%s) @@> ) <@@ [] @@> 
    let code = 
        <@@ 
            let conn = ( %%args.[0] : obj) :?> HubConnection
            let proxy = conn.createHubProxy("myHub")
            let arguments = (%%argsArray : obj list) |> Array.ofList
            proxy.invoke(name, (arguments : obj array) ) |> ignore
        @@>
    code


makeCode testCode

meth.InvokeCode <- makeCode

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