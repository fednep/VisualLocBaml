using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TranslationApi.Baml
{
    public class ResourceGenerationOptions
    {

        private Assembly __SourceAssembly;
        private string __AssemblyFileName;
        private Assembly[] __AdditionalAssemblies;
        private CultureInfo __CultureInfo;
        private List<BamlString> __BamlStrings;
        private string __OutputFileName;
        private AppDomain __AppDomain;

        public ResourceGenerationOptions(AppDomain appDomain, Assembly sourceAssembly, string assemblyFileName, CultureInfo cultureInfo, List<BamlString> bamlStrings, string outputFileName)
        {
            __AppDomain = appDomain;
            __SourceAssembly = sourceAssembly;
            __AssemblyFileName = assemblyFileName;
            __AdditionalAssemblies = new Assembly[0];            
            __CultureInfo = cultureInfo;

            __BamlStrings = bamlStrings;
            __OutputFileName = outputFileName;
        }

        public void WriteLine(string message)
        {
            // do nothing
        }

        public void Write(string message)
        {
            // do nothing
        }

        public Assembly SourceAssembly
        {
            get 
            {
                return __SourceAssembly;
            }
        }

        public string AssemblyFileName
        {
            get 
            {
                return __AssemblyFileName;
            }
        }
        public Assembly[] AdditionalAssemblies
        {
            get 
            {
                return __AdditionalAssemblies;
            }
            set
            {
                __AdditionalAssemblies = value;
            }
        }

        public CultureInfo CultureInfo
        {
            get 
            {
                return __CultureInfo;
            }
        }

        public List<BamlString> BamlStrings
        {
            get 
            {
                return __BamlStrings;
            }
        }

        public string OutputFileName
        {
            get
            {
                return __OutputFileName;
            }
        }

        public AppDomain AppDomain
        {
            get
            {
                return __AppDomain;
            }
        }
    }
}
