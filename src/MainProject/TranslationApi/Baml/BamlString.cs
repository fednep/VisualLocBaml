using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup.Localizer;

namespace TranslationApi.Baml
{
    [Serializable]
    public sealed class BamlString
    {
        private string __BamlFile;
        private string __ResourceKey;
        private LocalizationCategory __Category;
        private string __Readability;
        private string __Modifiable;
        private string __Comments;
        private string __Content;

        public BamlString(string bamlFile, string resourceKey, BamlLocalizableResource resource)
        {
            Init(bamlFile, 
                resourceKey,  
                resource.Category, 
                resource.Readable.ToString(), 
                resource.Modifiable.ToString(), 
                ValueOrEmpty(resource.Comments));

            __Content = resource.Content;
        }

        public BamlString(string bamlFile, string resourceKey, LocalizationCategory category, string readability, string modifiable, string comment, 
            string content)
        {
            Init(bamlFile, resourceKey, category, readability, modifiable, comment);
            __Content = content;
        }

        private void Init(string bamlFile, string resourceKey, LocalizationCategory category, string readability, string modifiable, string comment)
        {
            __BamlFile = bamlFile;
            __ResourceKey = resourceKey;

            __Category = category;
            __Readability = readability;
            __Modifiable = modifiable;

            __Comments = comment;
        }

        private string ValueOrEmpty(string value)
        {
            if (value == null)
                return "";

            return value;
        }

        public string BamlFile
        {
            get
            {
                return __BamlFile;
            }
        }

        public string ResourceKey
        {
            get
            {
                return __ResourceKey;
            }
        }

        public LocalizationCategory Category
        {
            get
            {
                return __Category;
            }
        }

        public string Readability
        {
            get
            {
                return __Readability;
            }
        }

        public string Modifiable
        {
            get
            {
                return __Modifiable;
            }
        }

        public string Comments
        {
            get
            {
                return __Comments;
            }
        }

        public string Content
        {
            get
            {
                return __Content;
            }
        }
    }
}
