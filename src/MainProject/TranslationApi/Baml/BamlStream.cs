using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Diagnostics;

namespace TranslationApi.Baml
{
    /// <summary>
    /// BamlStream class which represents a baml stream
    /// </summary>
    internal class BamlStream
    {
        private string __Name;
        private Stream __Stream;

        /// <summary>
        /// constructor
        /// </summary>
        internal BamlStream(string name, Stream stream)
        {
            __Name = name;
            __Stream = stream;
        }

        /// <summary>
        /// name of the baml 
        /// </summary>
        internal string Name
        {
            get { return __Name; }
        }

        /// <summary>
        /// The actual Baml stream
        /// </summary>
        internal Stream Stream
        {
            get { return __Stream; }
        }

        /// <summary>
        /// close the stream
        /// </summary>
        internal void Close()
        {
            if (__Stream != null)
            {
                __Stream.Close();
            }
        }

        /// <summary>
        /// Helper method which determines whether a stream name and value pair indicates a baml stream
        /// </summary>
        internal static bool IsResourceEntryBamlStream(string name, object value)
        {

            string extension = "";
            try
            {
                extension = Path.GetExtension(name);
            } catch
            {
                return false;
            }

            if (string.Compare(
                    extension,
                    "." + FileType.BAML.ToString(),
                    true,
                    CultureInfo.InvariantCulture
                    ) == 0
                )
            {
                //it has .Baml at the end
                Type type = value.GetType();

                if (typeof(Stream).IsAssignableFrom(type))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Combine baml stream name and resource name to uniquely identify a baml within a 
        /// localization project
        /// </summary>
        internal static string CombineBamlStreamName(string resource, string bamlName)
        {
            Debug.Assert(resource != null && bamlName != null, "Resource name and baml name can't be null");

            string suffix = Path.GetFileName(bamlName);
            string prefix = Path.GetFileName(resource);

            return prefix + LocBamlConst.BamlAndResourceSeperator + suffix;
        }
    }
}
