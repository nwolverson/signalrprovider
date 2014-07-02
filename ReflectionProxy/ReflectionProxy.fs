namespace ReflectionProxy

open System.Reflection
open System
open System.IO

type ReflectionProxy() =
    inherit MarshalByRefObject()

    let loadAssembliesBytes assemblies =
        let ass = assemblies |> Seq.last

        let resolve (o: obj) (args: ResolveEventArgs) = 
            let name = AssemblyName(args.Name).Name
            try
                Assembly.Load args.Name
            with
                | _ -> Assembly.LoadFrom <| Path.Combine(Path.GetDirectoryName ass, name + ".dll")

        AppDomain.CurrentDomain.add_AssemblyResolve (ResolveEventHandler resolve)
        
        ass
        |> Assembly.LoadFrom

    let getMethodType (hubName : string) (mi: MethodInfo) =
        let name = mi.Name
        let parms = mi.GetParameters() |> Seq.map (fun p -> (p.Name,  p.ParameterType (* typeof<obj> *)))

        let returnType = if mi.ReturnType.Equals(typeof<System.Void>) then typeof<unit> else mi.ReturnType

        let methTy = (name, parms, returnType)
        methTy
    
    // whatever version
    let nameHubNameAttr = "Microsoft.AspNet.SignalR.Hubs.HubNameAttribute"
    let nameIHub = "Microsoft.AspNet.SignalR.Hubs.IHub"
    let nameHub = "Microsoft.AspNet.SignalR.Hubs.HHub"

    let hubAttrs (t: Type) = 
        //CustomAttributeData.GetCustomAttributes(t)
        t.GetCustomAttributes()

        |> Seq.filter (fun attr -> attr.GetType().FullName = nameHubNameAttr)

    let hubName (hubType : Type) = 
        let attr = hubAttrs hubType |> Seq.head
        attr.GetType().GetProperty("HubName").GetValue(attr) :?> string
        //CustomAttributeData. .GetType() ConstructorArguments.[0].Value :?> string


    let makeHubType hubType =
        let name = hubName hubType

        // exclusion taken from signalr defn
        let excludeTypes = [ nameHub; typeof<obj>.FullName ]
        let excludeInterfaces = [ nameIHub; typeof<IDisposable>.FullName]

        let findty tname = hubType.GetTypeInfo().ImplementedInterfaces |> Seq.find (fun i -> i.FullName = tname)
        //todo unused
        let ihubty = findty nameIHub
        let idispty = findty typeof<IDisposable>.FullName

        let exclude (m: MethodInfo) =
            m.IsSpecialName 
            || excludeTypes |> List.exists (fun x -> m.GetBaseDefinition().DeclaringType.FullName = x) 
            || excludeInterfaces 
                |> Seq.collect (fun ity -> hubType.GetInterfaceMap(findty ity).TargetMethods) 
                |> Seq.exists (fun x -> x = m) 

        let methTypes = 
            hubType.GetMethods(BindingFlags.Public ||| BindingFlags.Instance)
            |> Seq.where (exclude >> not)
            |> Seq.map (getMethodType name) 
            |> List.ofSeq

        let methTypeNames = 
            methTypes 
            |> List.map (fun (name,args,retty) -> (name, args |> List.ofSeq |> List.map (fun (n,ty) -> (n, ty.FullName)), retty.FullName))

        (name, methTypeNames)

    let findHubs hubs = 
        let hasHubAttribute = hubAttrs >> Seq.isEmpty >> not
        List.filter hasHubAttribute hubs

    member this.GetDefinedTypes(assemblies : string seq) 
        : List<string * List<string * List<string * string> * string>> = 
        let clientAsm = loadAssembliesBytes assemblies
        let deftypes1 = clientAsm.ExportedTypes
        let defTypes = clientAsm.DefinedTypes
        //let infos = defTypes |> Seq.map (fun t -> t.GetTypeInfo()) |> List.ofSeq
        let hubs = deftypes1 |> List.ofSeq|> findHubs
        let hubTypes = hubs |> List.map makeHubType
        hubTypes
        

