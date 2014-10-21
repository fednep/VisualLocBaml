
using System;
using System.IO;
using System.Collections;
using System.Globalization;
using System.Diagnostics;
using System.Threading;
using System.Reflection;
using System.Security;
using System.Windows;
using System.Windows.Threading;
using System.Collections.Generic;

namespace TranslationApi.Baml
{
    /// <summary>
    /// LocBaml tool: A command line tool to localize baml
    /// </summary>
    public static class LocBaml
    {
        private const int ErrorCode = 100;        
        private const int SuccessCode = 0;

        public static List<BamlString> ExtractStringsFromAssembly(Assembly assembly)
        {
            return StringsExtractor.ExtractBamlStrings(assembly);
        }

        /// <summary>
        /// Genereate localized baml 
        /// </summary>        
        private static void GenerateBamlResources(LocBamlOptions options)
        {   
            Stream input = File.OpenRead(options.TranslationsFile);
            using (ResourceTextReader reader = new ResourceTextReader(options.TranslationFileType, input))
            {   
                TranslationDictionariesReader dictionaries = new TranslationDictionariesReader(reader);
                ResourceGenerator.Generate(options, dictionaries);
            }
        }

        public static void GenerateBamlResources(ResourceGenerationOptions options)
        {            
            
            ResourceGenerator.GenerateAssembly(options);
        }
    }

}


