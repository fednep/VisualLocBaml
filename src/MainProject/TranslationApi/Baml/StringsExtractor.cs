//---------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// Description: TranslationDictionariesWriter & TranslationDictionariesReader class
//
//---------------------------------------------------------------------------

using System;
using System.IO;
using System.Resources;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Markup.Localizer;
using System.Collections.Generic;
using System.Reflection;

namespace TranslationApi.Baml
{
    /// <summary>
    /// Writer to write out localizable values into CSV or tab-separated txt files.     
    /// </summary>
    internal static class  StringsExtractor
    {

        internal static List<BamlString> ExtractBamlStrings(Assembly assembly)
        {
            InputBamlStreamList bamlStreamList = new InputBamlStreamList(assembly);

            List<BamlString> resultingList = new List<BamlString>();
            for (int i = 0; i < bamlStreamList.Count; i++)
            {
                // Search for comment file in the same directory. The comment file has the extension to be 
                // "loc".
//                string commentFile = Path.ChangeExtension(bamlStreamList[i].Name, "loc");
                TextReader commentStream = null;

                try
                {
                    //if (File.Exists(commentFile))
                    //{
                    //    commentStream = new StreamReader(commentFile);
                    //}

                    // create the baml localizer
                    BamlLocalizer mgr = new BamlLocalizer(
                        bamlStreamList[i].Stream,
                        new BamlLocalizabilityByReflection(new Assembly[] { assembly }),
                        commentStream
                        );

                    // extract localizable resource from the baml stream
                    BamlLocalizationDictionary dict = mgr.ExtractResources();

                    // write out each resource
                    foreach (DictionaryEntry entry in dict)
                    {

                        BamlLocalizableResourceKey key = (BamlLocalizableResourceKey)entry.Key;
                        BamlLocalizableResource resource = (BamlLocalizableResource)entry.Value;

                        resultingList.Add(new BamlString(bamlStreamList[i].Name, LocBamlConst.ResourceKeyToString(key), resource));
                    }

                }
                finally
                {
                    if (commentStream != null)
                        commentStream.Close();
                }
            }

            // close all the baml input streams, output stream is closed by writer.
            bamlStreamList.Close();

            // options.WriteLine(StringLoader.Get("Done"));


            return resultingList;
        }

        ///// <summary>
        ///// Write the localizable key-value pairs
        ///// </summary>
        ///// <param name="options"></param>
        //internal static void Write(LocBamlOptions options)            
        //{   
        //    Stream output = new FileStream(options.Output, FileMode.Create);
            
        //    var list = ExtractBamlStrings(options);

        //    using (ResourceTextWriter writer = new ResourceTextWriter(options.TranslationFileType, output))
        //    {
        //        foreach (var item in list)
        //            item.Write(writer);
        //    }
        //}
    }


    /// <summary>
    /// Reader to read the translations from CSV or tab-separated txt file    
    /// </summary> 
    internal class TranslationDictionariesReader
    {
        internal TranslationDictionariesReader(List<BamlString> bamlStrings)
        {

            // hash key is case insensitive strings
            _table = new Hashtable();

            // we read each Row
            int rowNumber = 0;
            foreach (BamlString bamlString in bamlStrings)
            {
                rowNumber++;

                // field #1 is the baml name.
                string bamlName = bamlString.BamlFile;

                // it can't be null
                if (bamlName == null)
                    throw new ApplicationException(StringLoader.Get("EmptyRowEncountered"));

                // field #2: key to the localizable resource
                string key = bamlString.ResourceKey;
                if (key == null)
                    throw new ApplicationException(StringLoader.Get("NullBamlKeyNameInRow"));

                BamlLocalizableResourceKey resourceKey = LocBamlConst.StringToResourceKey(key);

                // get the dictionary 
                BamlLocalizationDictionary dictionary = this[bamlName];
                if (dictionary == null)
                {
                    // we create one if it is not there yet.
                    dictionary = new BamlLocalizationDictionary();
                    this[bamlName] = dictionary;
                }

                BamlLocalizableResource resource;

                // the rest of the fields are either all null,
                // or all non-null. If all null, it means the resource entry is deleted.

                if (bamlString.Category == null)
                {
                    // it means all the following fields are null starting from column #3.
                    resource = null;
                }
                else
                {
                    resource = new BamlLocalizableResource();
                    resource.Category = bamlString.Category;
                    resource.Readable = (bool)BoolTypeConverter.ConvertFrom(bamlString.Readability);
                    resource.Modifiable = (bool)BoolTypeConverter.ConvertFrom(bamlString.Modifiable);
                    resource.Comments = bamlString.Comments;
                    resource.Content = bamlString.Content;

                    // in case content being the last column, consider null as empty.
                    if (resource.Content == null)
                        resource.Content = string.Empty;
                }

                // at this point, we are good.
                // add to the dictionary.
                dictionary.Add(resourceKey, resource);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="reader">resoure text reader that reads CSV or a tab-separated txt file</param>
        internal TranslationDictionariesReader(ResourceTextReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            // hash key is case insensitive strings
            _table = new Hashtable();

            // we read each Row
            int rowNumber = 0;
            while (reader.ReadRow())
            {
                rowNumber ++;
                
                // field #1 is the baml name.
                string bamlName = reader.GetColumn(0);

                // it can't be null
                if (bamlName == null)
                    throw new ApplicationException(StringLoader.Get("EmptyRowEncountered"));

                if (string.IsNullOrEmpty(bamlName))
                {
                    // allow for comment lines in csv file.
                    // each comment line starts with ",". It will make the first entry as String.Empty.
                    // and we will skip the whole line.
                    continue;   // if the first column is empty, take it as a comment line
                }

                // field #2: key to the localizable resource
                string key = reader.GetColumn(1);
                if (key == null)
                    throw new ApplicationException(StringLoader.Get("NullBamlKeyNameInRow"));

                BamlLocalizableResourceKey resourceKey = LocBamlConst.StringToResourceKey(key);

                // get the dictionary 
                BamlLocalizationDictionary dictionary = this[bamlName];                
                if (dictionary == null)
                {   
                    // we create one if it is not there yet.
                    dictionary = new BamlLocalizationDictionary();
                    this[bamlName] = dictionary;                
                }
                
                BamlLocalizableResource resource;
                
                // the rest of the fields are either all null,
                // or all non-null. If all null, it means the resource entry is deleted.
                
                // get the string category
                string categoryString = reader.GetColumn(2);
                if (categoryString == null)
                {
                    // it means all the following fields are null starting from column #3.
                    resource = null;
                }
                else
                {
                    // the rest must all be non-null.
                    // the last cell can be null if there is no content
                    for (int i = 3; i < 6; i++)
                    {
                        if (reader.GetColumn(i) == null)
                            throw new Exception(StringLoader.Get("InvalidRow"));
                    }

                    // now we know all are non-null. let's try to create a resource
                    resource  = new BamlLocalizableResource();
                    
                    // field #3: Category
                    resource.Category = (LocalizationCategory) StringCatConverter.ConvertFrom(categoryString);
                    
                    // field #4: Readable
                    resource.Readable     = (bool) BoolTypeConverter.ConvertFrom(reader.GetColumn(3));

                    // field #5: Modifiable
                    resource.Modifiable   = (bool) BoolTypeConverter.ConvertFrom(reader.GetColumn(4));

                    // field #6: Comments
                    resource.Comments     = reader.GetColumn(5);

                    // field #7: Content
                    resource.Content      = reader.GetColumn(6);

                    // in case content being the last column, consider null as empty.
                    if (resource.Content == null)
                        resource.Content = string.Empty;

                    // field > #7: Ignored.
                }                             

                // at this point, we are good.
                // add to the dictionary.
                dictionary.Add(resourceKey, resource);
            }        
        }

        internal BamlLocalizationDictionary this[string key]
        {
            get{
                return (BamlLocalizationDictionary) _table[key.ToLowerInvariant()];
            }
            set
            {
                _table[key.ToLowerInvariant()] = value;
            }
        }

        // hashtable that maps from baml name to its ResourceDictionary
        private Hashtable _table;                
        private static TypeConverter BoolTypeConverter  = TypeDescriptor.GetConverter(true);
        private static TypeConverter StringCatConverter = TypeDescriptor.GetConverter(LocalizationCategory.Text);
    }
}
