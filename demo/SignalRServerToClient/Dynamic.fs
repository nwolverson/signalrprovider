module Dynamic

// Source code from: http://www.fssnip.net/2U

open System
open System.Runtime.CompilerServices
open Microsoft.CSharp.RuntimeBinder

// Simple implementation of ? operator that works for instance
// method calls that take a single argument and return some result
let (?) (inst:obj) name (arg:'T) : 'R =
  // TODO: For efficient implementation, consider caching of call sites 
  // Create dynamic call site for converting result to type 'R
  let convertSite = 
    CallSite<Func<CallSite, Object, 'R>>.Create
      (Binder.Convert(CSharpBinderFlags.None, typeof<'R>, null))

  // Create call site for performing call to method with the given
  // name and a single parameter of type 'T
  let callSite = 
    CallSite<Func<CallSite, Object, 'T, Object>>.Create
      (Binder.InvokeMember
        ( CSharpBinderFlags.None, name, null, null, 
          [| CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null);
             CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) |]))

  // Run the method call using second call site and then 
  // convert the result to the specified type using first call site
  convertSite.Target.Invoke
    (convertSite, callSite.Target.Invoke(callSite, inst, arg))
