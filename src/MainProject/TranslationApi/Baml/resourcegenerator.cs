//---------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// Description: ResourceGenerator class
//              It generates the localized baml from translations
//
//---------------------------------------------------------------------------
using System;
using System.IO;
using System.Windows;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Resources;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Markup.Localizer;

namespace TranslationApi.Baml
{
    /// <summary>
    /// ResourceGenerator class
    /// </summary>
    internal static class ResourceGenerator
    {
        /// <summary>
        /// Generates localized Baml from translations
        /// </summary>
        /// <param name="options">LocBaml options</param>
        /// <param name="dictionaries">the translation dictionaries</param>
        internal static void Generate(LocBamlOptions options, TranslationDictionariesReader dictionaries)
        {
            // base on the input, we generate differently            
            switch (options.InputType)
            {
                case FileType.BAML:
                    {
                        throw new Exception("Is not supported yet");
                        break;
                    }

                case FileType.RESOURCES:
                    {
                        throw new Exception("Is not supported yet");
                        break;
                    }

                case FileType.EXE:
                case FileType.DLL:
                    {
                        GenerateAssembly(options, dictionaries);
                        break;
                    }
                default:
                    {
                        Debug.Assert(false, "Can't generate to this type");
                        break;
                    }
            }
        }

        private static void GenerateBamlStream(Stream input, Stream output, BamlLocalizationDictionary dictionary, ResourceGenerationOptions options)
        {
            string commentFile = Path.ChangeExtension(options.AssemblyFileName, "loc");
            TextReader commentStream = null;

            try
            {
                if (File.Exists(commentFile))
                {
                    commentStream = new StreamReader(commentFile);
                }

                // create a localizabilty resolver based on reflection
                BamlLocalizabilityByReflection localizabilityReflector =
                    new BamlLocalizabilityByReflection(options.AdditionalAssemblies);

                // create baml localizer
                BamlLocalizer mgr = new BamlLocalizer(
                    input,
                    localizabilityReflector,
                    commentStream
                    );

                // get the resources
                BamlLocalizationDictionary source = mgr.ExtractResources();
                BamlLocalizationDictionary translations = new BamlLocalizationDictionary();

                foreach (DictionaryEntry entry in dictionary)
                {
                    BamlLocalizableResourceKey key = (BamlLocalizableResourceKey)entry.Key;
                    // filter out unchanged items
                    if (!source.Contains(key)
                      || entry.Value == null
                      || source[key].Content != ((BamlLocalizableResource)entry.Value).Content)
                    {
                        //if (!string.IsNullOrEmpty(((BamlLocalizableResource)entry.Value).Content))
                        translations.Add(key, (BamlLocalizableResource)entry.Value);
                    }
                }

                // update baml
                mgr.UpdateBaml(output, translations);
            }
            finally
            {
                if (commentStream != null)
                {
                    commentStream.Close();
                }
            }
        }

        private static void GenerateResourceStream(
                ResourceGenerationOptions options,                     // options from the command line
                string resourceName,                        // the name of the .resources file
                IResourceReader reader,                     // the reader for the .resources
                IResourceWriter writer,                     // the writer for the output .resources
                TranslationDictionariesReader dictionaries  // the translations
            )
        {

            //options.WriteLine(StringLoader.Get("GenerateResource", resourceName));
            // enumerate through each resource and generate it
            foreach (DictionaryEntry entry in reader)
            {
                string name = entry.Key as string;
                object resourceValue = null;

                // See if it looks like a Baml resource
                if (BamlStream.IsResourceEntryBamlStream(name, entry.Value))
                {
                    Stream targetStream = null;
                    //                    options.Write("    ");
                    //options.Write(StringLoader.Get("GenerateBaml", name));

                    // grab the localizations available for this Baml
                    string bamlName = BamlStream.CombineBamlStreamName(resourceName, name);
                    BamlLocalizationDictionary localizations = dictionaries[bamlName];
                    if (localizations != null)
                    {
                        targetStream = new MemoryStream();

                        // generate into a new Baml stream
                        GenerateBamlStream(
                            (Stream)entry.Value,
                            targetStream,
                            localizations,
                            options
                        );
                    }

                    options.WriteLine(StringLoader.Get("Done"));

                    // sets the generated object to be the generated baml stream
                    resourceValue = targetStream;
                }

                if (resourceValue == null)
                {
                    //
                    // The stream is not localized as Baml yet, so we will make a copy of this item into 
                    // the localized resources
                    //

                    // We will add the value as is if it is serializable. Otherwise, make a copy
                    resourceValue = entry.Value;

                    object[] serializableAttributes = resourceValue.GetType().GetCustomAttributes(typeof(SerializableAttribute), true);
                    if (serializableAttributes.Length == 0)
                    {
                        // The item returned from resource reader is not serializable
                        // If it is Stream, we can wrap all the values in a MemoryStream and 
                        // add to the resource. Otherwise, we had to skip this resource.
                        Stream resourceStream = resourceValue as Stream;
                        if (resourceStream != null)
                        {
                            Stream targetStream = new MemoryStream();
                            byte[] buffer = new byte[resourceStream.Length];
                            resourceStream.Read(buffer, 0, buffer.Length);
                            targetStream = new MemoryStream(buffer);
                            resourceValue = targetStream;
                        }
                    }
                }

                if (resourceValue != null)
                {
                    writer.AddResource(name, resourceValue);
                }
            }
        }

        private static void GenerateStandaloneResource(string fullPathName, Stream resourceStream)
        {
            // simply do a copy for the stream
            using (FileStream file = new FileStream(fullPathName, FileMode.Create, FileAccess.Write))
            {
                const int BUFFER_SIZE = 4096;
                byte[] buffer = new byte[BUFFER_SIZE];
                int bytesRead = 1;
                while (bytesRead > 0)
                {
                    bytesRead = resourceStream.Read(buffer, 0, BUFFER_SIZE);
                    file.Write(buffer, 0, bytesRead);
                }
            }
        }

        internal static void GenerateAssembly(ResourceGenerationOptions options)
        {
            TranslationDictionariesReader dictionaries = new TranslationDictionariesReader(options.BamlStrings);

            string sourceAssemblyFullName = options.AssemblyFileName;                // source assembly full path 
            string outputAssemblyLocalName = System.IO.Path.GetFileName(options.OutputFileName);   // output assembly name
            string moduleLocalName = GetAssemblyModuleLocalName(
                                                    options.CultureInfo,
                                                    System.IO.Path.GetFileName(outputAssemblyLocalName)); // the module name within the assmbly

            // get the source assembly
            Assembly sourceAssembly = options.SourceAssembly;
            CultureInfo cultureInfo = options.CultureInfo;

            // obtain the assembly name
            AssemblyName targetAssemblyNameObj = sourceAssembly.GetName();

            // store the culture info of the source assembly
            CultureInfo srcCultureInfo = targetAssemblyNameObj.CultureInfo;

            // update it to use it for target assembly
            targetAssemblyNameObj.Name = Path.GetFileNameWithoutExtension(outputAssemblyLocalName);
            targetAssemblyNameObj.CultureInfo = cultureInfo;

            // we get a assembly builder
            AssemblyBuilder targetAssemblyBuilder = options.AppDomain.DefineDynamicAssembly(
                targetAssemblyNameObj,                  // name of the assembly
                AssemblyBuilderAccess.RunAndSave,       // access rights
                System.IO.Path.GetDirectoryName(options.OutputFileName)// storage dir
                );

            // we create a module builder for embeded resource modules
            ModuleBuilder moduleBuilder = targetAssemblyBuilder.DefineDynamicModule(
                moduleLocalName,
                outputAssemblyLocalName
                );

            //options.WriteLine(StringLoader.Get("GenerateAssembly"));

            // now for each resource in the assembly
            foreach (string resourceName in sourceAssembly.GetManifestResourceNames())
            {
                // get the resource location for the resource
                ResourceLocation resourceLocation = sourceAssembly.GetManifestResourceInfo(resourceName).ResourceLocation;

                // if this resource is in another assemlby, we will skip it
                if ((resourceLocation & ResourceLocation.ContainedInAnotherAssembly) != 0)
                {
                    continue;   // in resource assembly, we don't have resource that is contained in another assembly
                }

                // gets the neutral resource name, giving it the source culture info
                string neutralResourceName = GetNeutralResModuleName(resourceName, srcCultureInfo);

                // gets the target resource name, by giving it the target culture info
                string targetResourceName = GetCultureSpecificResourceName(neutralResourceName, cultureInfo);

                // resource stream              
                Stream resourceStream = sourceAssembly.GetManifestResourceStream(resourceName);

                // see if it is a .resources
                if (neutralResourceName.ToLower(CultureInfo.InvariantCulture).EndsWith(".resources"))
                {
                    // now we think we have resource stream 
                    // get the resource writer
                    IResourceWriter writer;
                    // check if it is a embeded assembly
                    if ((resourceLocation & ResourceLocation.Embedded) != 0)
                    {
                        // gets the resource writer from the module builder
                        writer = moduleBuilder.DefineResource(
                            targetResourceName,         // resource name
                            targetResourceName,         // resource description
                            ResourceAttributes.Public   // visibilty of this resource to other assembly
                            );
                    }
                    else
                    {
                        // it is a standalone resource, we get the resource writer from the assembly builder
                        writer = targetAssemblyBuilder.DefineResource(
                            targetResourceName,         // resource name 
                            targetResourceName,         // description
                            targetResourceName,         // file name to save to   
                            ResourceAttributes.Public   // visibility of this resource to other assembly
                        );
                    }

                    // get the resource reader
                    IResourceReader reader = new ResourceReader(resourceStream);

                    // generate the resources
                    GenerateResourceStream(options, resourceName, reader, writer, dictionaries);

                    // we don't call writer.Generate() or writer.Close() here 
                    // because the AssemblyBuilder will call them when we call Save() on it.
                }
                else
                {
                    // else it is a stand alone untyped manifest resources.
                    string extension = Path.GetExtension(targetResourceName);

                    string fullFileName = Path.Combine(
                                                    System.IO.Path.GetDirectoryName(options.AssemblyFileName), 
                                                    targetResourceName);

                    // check if it is a .baml, case-insensitive
                    if (string.Compare(extension, ".baml", true, CultureInfo.InvariantCulture) == 0)
                    {
                        // try to localized the the baml
                        // find the resource dictionary
                        BamlLocalizationDictionary dictionary = dictionaries[resourceName];

                        // if it is null, just create an empty dictionary.
                        if (dictionary != null)
                        {
                            // it is a baml stream
                            using (Stream output = File.OpenWrite(fullFileName))
                            {
                                options.Write("    ");
                                options.WriteLine(StringLoader.Get("GenerateStandaloneBaml", fullFileName));
                                GenerateBamlStream(resourceStream, output, dictionary, options);
                                options.WriteLine(StringLoader.Get("Done"));
                            }
                        }
                        else
                        {
                            // can't find localization of it, just copy it
                            GenerateStandaloneResource(fullFileName, resourceStream);
                        }
                    }
                    else
                    {
                        // it is an untyped resource stream, just copy it
                        GenerateStandaloneResource(fullFileName, resourceStream);
                    }

                    // now add this resource file into the assembly
                    targetAssemblyBuilder.AddResourceFile(
                        targetResourceName,           // resource name
                        targetResourceName,           // file name
                        ResourceAttributes.Public     // visibility of the resource to other assembly
                    );

                }
            }

            // at the end, generate the assembly
            targetAssemblyBuilder.Save(outputAssemblyLocalName);
            options.WriteLine(StringLoader.Get("DoneGeneratingAssembly"));
        }

        //--------------------------------------------------
        // The function follows Managed code parser
        // implementation. in the future, maybe they should 
        // share the same code
        //--------------------------------------------------
        private static void GenerateAssembly(LocBamlOptions options, TranslationDictionariesReader dictionaries)
        {
            // there are many names to be used when generating an assembly
            throw new Exception("This vertion doesn't support this function");
        }


        //-----------------------------------------
        // private function dealing with naming 
        //-----------------------------------------

        // return the local output file name, i.e. without directory
        private static string GetOutputFileName(LocBamlOptions options)
        {
            string outputFileName;
            string inputFileName = Path.GetFileName(options.InputFile);

            switch (options.InputType)
            {
                case FileType.BAML:
                    {
                        return inputFileName;
                    }
                case FileType.EXE:
                    {
                        inputFileName = inputFileName.Remove(inputFileName.LastIndexOf('.')) + ".resources.dll";
                        return inputFileName;
                    }
                case FileType.DLL:
                    {
                        return inputFileName;
                    }
                case FileType.RESOURCES:
                    {
                        // get the output file name
                        outputFileName = inputFileName;

                        // get to the last dot seperating filename and extension
                        int lastDot = outputFileName.LastIndexOf('.');
                        int secondLastDot = outputFileName.LastIndexOf('.', lastDot - 1);
                        if (secondLastDot > 0)
                        {
                            string cultureName = outputFileName.Substring(secondLastDot + 1, lastDot - secondLastDot - 1);
                            if (LocBamlConst.IsValidCultureName(cultureName))
                            {
                                string extension = outputFileName.Substring(lastDot);
                                string frontPart = outputFileName.Substring(0, secondLastDot + 1);
                                outputFileName = frontPart + options.CultureInfo.Name + extension;
                            }
                        }
                        return outputFileName;
                    }
                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        private static string GetAssemblyModuleLocalName(CultureInfo cultureInfo, string targetAssemblyName)
        {
            string moduleName;
            if (targetAssemblyName.ToLower(CultureInfo.InvariantCulture).EndsWith(".resources.dll"))
            {
                // we create the satellite assembly name
                moduleName = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}.{1}.{2}",
                    targetAssemblyName.Substring(0, targetAssemblyName.Length - ".resources.dll".Length),
                    cultureInfo.Name,
                    "resources.dll"
                    );
            }
            else
            {
                moduleName = targetAssemblyName;
            }
            return moduleName;
        }



        // return the neutral resource name
        private static string GetNeutralResModuleName(string resourceName, CultureInfo cultureInfo)
        {
            if (cultureInfo.Equals(CultureInfo.InvariantCulture))
            {
                return resourceName;
            }
            else
            {
                // if it is an satellite assembly, we need to strip out the culture name
                string normalizedName = resourceName.ToLower(CultureInfo.InvariantCulture);
                int end = normalizedName.LastIndexOf(".resources");

                if (end < 0)
                {
                    return resourceName;
                }

                int start = normalizedName.LastIndexOf('.', end - 1);

                if (start > 0 && end - start > 0)
                {
                    string cultureStr = resourceName.Substring(start + 1, end - start - 1);

                    if (string.Compare(cultureStr, cultureInfo.Name, true) == 0)
                    {
                        // it has the correct culture name, so we can take it out
                        return resourceName.Remove(start, end - start);
                    }
                }
                return resourceName;
            }
        }

        private static string GetCultureSpecificResourceName(string neutralResourceName, CultureInfo culture)
        {
            // gets the extension
            string extension = Path.GetExtension(neutralResourceName);

            // swap in culture name
            string cultureName = Path.ChangeExtension(neutralResourceName, culture.Name);

            // return the new name with the same extension
            return cultureName + extension;
        }

    }
}
