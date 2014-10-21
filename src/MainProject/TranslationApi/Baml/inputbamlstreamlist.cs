//---------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// Description: InputBamlStreamList class
//              It enumerates all the baml streams in the input file for parsing
//
//---------------------------------------------------------------------------
using System;
using System.IO;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Resources;

namespace TranslationApi.Baml
{
    /// <summary>
    /// Class that enumerates all the baml streams in the input file
    /// </summary>
    internal class InputBamlStreamList
    {

        private ArrayList __BamlStreams;

        /// <summary>
        /// constructor
        /// </summary>        
        internal InputBamlStreamList(Assembly assembly)
        {
            __BamlStreams = new ArrayList();
            CreateListByAssembly(assembly);
        }

        /// <summary>
        /// constructor
        /// </summary>        
        internal InputBamlStreamList(LocBamlOptions options)
        {
            __BamlStreams  = new ArrayList();
            switch(options.InputType)
            {
                case FileType.BAML:
                {
                    __BamlStreams.Add(
                        new BamlStream(
                            Path.GetFileName(options.InputFile),
                            File.OpenRead(options.InputFile)
                            )
                    );
                    break;
                }

                case FileType.RESOURCES:
                {
                    using (ResourceReader resourceReader = new ResourceReader(options.InputFile))
                    {
                        // enumerate all bamls in a resources
                        EnumerateBamlInResources(resourceReader, options.InputFile);
                    }
                    break;
                }

		        case FileType.EXE:
                case FileType.DLL:
                {
                    // for a dll, it is the same idea
                    CreateListByAssembly(
                        Assembly.LoadFrom(options.InputFile));
                    break;
                }
                default:
                {
                    
                    Debug.Assert(false, "Not supported type");
                    break;
                }                    
            }                  
        }

        private void CreateListByAssembly(Assembly assembly)
        {

            foreach (string resourceName in assembly.GetManifestResourceNames())
            {
                ResourceLocation resourceLocation = assembly.GetManifestResourceInfo(resourceName).ResourceLocation;

                // if this resource is in another assemlby, we will skip it
                if ((resourceLocation & ResourceLocation.ContainedInAnotherAssembly) != 0)
                {
                    continue;
                }

                Stream resourceStream = assembly.GetManifestResourceStream(resourceName);
                using (ResourceReader reader = new ResourceReader(resourceStream))
                {
                    EnumerateBamlInResources(reader, resourceName);
                }
            }
        }

        /// <summary
        /// >
        /// return the number of baml streams found
        /// </summary>
        internal int Count
        {
            get{return __BamlStreams.Count;}
        }

        /// <summary>
        /// Gets the baml stream in the input file through indexer
        /// </summary>        
        internal BamlStream this[int i]
        {
            get { return (BamlStream) __BamlStreams[i];}
        }

        /// <summary>
        /// Close the baml streams enumerated
        /// </summary>
        internal void Close()
        {
            for (int i = 0; i < __BamlStreams.Count; i++)
            {
               ((BamlStream) __BamlStreams[i]).Close();
            }            
        }

        //--------------------------------
        // private function
        //--------------------------------
        /// <summary>
        /// Enumerate baml streams in a resources file
        /// </summary>        
        private void EnumerateBamlInResources(ResourceReader reader, string resourceName)
        {                       
            foreach (DictionaryEntry entry in reader)
            {
                string name = entry.Key as string;
                if (BamlStream.IsResourceEntryBamlStream(name, entry.Value))
                {    
                    __BamlStreams.Add( 
                        new BamlStream(
                            BamlStream.CombineBamlStreamName(resourceName, name),
                            (Stream) entry.Value
                        )
                    );
                }    
            }
        }
   }

}
