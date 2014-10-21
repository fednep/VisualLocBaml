using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TranslationApi.Baml
{
    public class LocalizationAppDomain: IDisposable
    {
        private AppDomain __AppDomain;
        private LocalizationInternalAppDomain __DomainInterface;

        public LocalizationAppDomain(string pathToBinaries)
        {
            __AppDomain = AppDomain.CreateDomain("TranslationAssemblies");
            __DomainInterface = (LocalizationInternalAppDomain)__AppDomain.CreateInstanceAndUnwrap(
                    Assembly.GetExecutingAssembly().FullName, "TranslationApi.Baml.LocalizationInternalAppDomain");

            AddPath(pathToBinaries);
        }

        ~LocalizationAppDomain()
        {
            Dispose();               
        }
        
        public void Dispose()
        {
            try
            {
                AppDomain.Unload(__AppDomain);
                GC.SuppressFinalize(this);
            }
            catch { }
        }

        public LoadedAssembly LoadAssembly(string fullAssemblyPath)
        {
            return __DomainInterface.LoadAssembly(fullAssemblyPath);
        }

        public void AddPath(string path)
        {
            __DomainInterface.AddPath(path);
        }

        public AppDomain AppDomain
        {
            get
            {
                return __AppDomain;
            }
        }
    }

    public class LocalizationInternalAppDomain: MarshalByRefObject
    {
        private List<string> __AssemblySearchPath;

        public LocalizationInternalAppDomain()
        {
            __AssemblySearchPath = new List<string>();
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            AssemblyName assemblyName = new AssemblyName(args.Name);

            var exeFile = assemblyName.Name + ".exe";
            var dllFile = assemblyName.Name + ".dll";

            foreach (var path in __AssemblySearchPath)
            {
                var fullPathExe = Path.Combine(path, exeFile);
                var fullPathDll = Path.Combine(path, dllFile);

                if (File.Exists(fullPathDll))
                    return Assembly.LoadFrom(fullPathDll);

                if (File.Exists(fullPathExe))
                    return Assembly.LoadFrom(fullPathExe);
            }

            return null;
        }

        internal LoadedAssembly LoadAssembly(string fullAssemblyPath)
        {
            LoadedAssembly loadedAssembly = new LoadedAssembly(fullAssemblyPath);
            loadedAssembly.Load();
            return loadedAssembly;
        }

        internal void AddPath(string path)
        {
            __AssemblySearchPath.Add(path);
        }        

    }
}
