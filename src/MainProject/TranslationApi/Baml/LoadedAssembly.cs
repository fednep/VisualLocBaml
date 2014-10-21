using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Globalization;

namespace TranslationApi.Baml
{
    public class LoadedAssembly: MarshalByRefObject
    {
        private string __FileName;
        private bool __IsLoaded = false;

        private bool __WasExceptionOnLoad;
        private Exception __ExceptionOnLoad;

        private Assembly __Assembly;
        private NeutralResourcesLanguageAttribute __NeutralResourceAttribute;

        public LoadedAssembly(string fileName)
        {
            __FileName = fileName;
        }

        public void Load()
        {
            __IsLoaded = false;
            try
            {                
                __Assembly = Assembly.LoadFrom(__FileName);
                __NeutralResourceAttribute = Attribute.GetCustomAttribute(__Assembly, typeof(System.Resources.NeutralResourcesLanguageAttribute)) 
                                                as System.Resources.NeutralResourcesLanguageAttribute;
            }
            catch (Exception e)
            {
                __WasExceptionOnLoad = true;
                __ExceptionOnLoad = e;
            }
        }

        public static bool IsNetAssembly(string path)
        {
            var sb = new StringBuilder(256);
            int written;
            var hr = GetFileVersion(path, sb, sb.Capacity, out written);
            return hr == 0;
        }

        [DllImport("mscoree.dll", CharSet = CharSet.Unicode)]
        private static extern int GetFileVersion(string path, StringBuilder buffer, int buflen, out int written);

        public List<BamlString> ExtractTranslationLines()
        {
            return LocBaml.ExtractStringsFromAssembly(__Assembly);
        }

        public bool IsLoaded
        {
            get
            {
                return __IsLoaded;
            }
        }

        public bool WasExceptionOnLoad
        {
            get
            {
                return __WasExceptionOnLoad;
            }
        }

        public Exception ExceptionOnLoad
        {
            get
            {
                return __ExceptionOnLoad;
            }
        }

        public Assembly Assembly
        {
            get
            {
                return __Assembly;
            }
        }

        public string FileName
        {
            get
            {
                return __FileName;
            }

            set
            {
                __FileName = value;
            }
        }

        public string NeutralResourceCultureName
        {
            get
            {
                if (__NeutralResourceAttribute == null)
                    return "";

                return __NeutralResourceAttribute.CultureName;
            }
        }

        public void GenerateSatteliteAssembly(string cultureCode, List<BamlString> translatedStrings, string satteliteTargetFileName)
        {
            CultureInfo cultureInfo = new CultureInfo(cultureCode);

            ResourceGenerationOptions generationOptions = 
                new ResourceGenerationOptions(
                        AppDomain.CurrentDomain, __Assembly, __FileName, 
                        cultureInfo, translatedStrings, satteliteTargetFileName);

            LocBaml.GenerateBamlResources(generationOptions);
        }
    }
}
