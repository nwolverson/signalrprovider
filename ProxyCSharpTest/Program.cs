using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ProxyCSharpTest
{
    class Program
    {
        //const string dll = @"C:\Users\Nicholas\Documents\git\signalrprovider\SignalRProvider\bin\Debug\ReflectionProxy.dll";
        const string dll = "c:\\users\\nicholas\\documents\\visual studio 2013\\Projects\\CreateInstanceFromHell\\RefAssembly\\bin\\Debug\\RefAssembly.dll";
        //const string cls = "ReflectionProxy.ReflectionProxy";
        const string cls = "RefAssembly.Class1";
        static void Main(string[] args)
        {
            var appDomain = AppDomain.CreateDomain("proxyTestDomain", null, new AppDomainSetup { ShadowCopyFiles="false",DisallowApplicationBaseProbing=true });
            //var appDomain = AppDomain.CreateDomain("proxyTestDomain");

            

            var obj = appDomain.CreateInstanceFromAndUnwrap(dll, cls) ;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            var t = obj.GetType();

            
            var x = t.InvokeMember("Test", System.Reflection.BindingFlags.InvokeMethod, Type.DefaultBinder, obj, new object[] { "Hello" } );
            t.InvokeMember("Test0", System.Reflection.BindingFlags.InvokeMethod, Type.DefaultBinder, obj, new object[] { });
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;


            AppDomain.Unload(appDomain);
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return Assembly.LoadFrom(dll);
        }
    }
}
