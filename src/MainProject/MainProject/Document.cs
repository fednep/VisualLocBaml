/*
    Copyright (c) 2013-2014 Fedir Nepyivoda <fednep@gmail.com>

    This file is part of VisualLocBaml project
    http://visuallocbaml.com

    VisualLocBaml is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    VisualLocBaml is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with VisualLocBaml. If not, see <http://www.gnu.org/licenses/>

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml;
using System.IO;
using TranslationApi;
using MainProject.UI;

namespace MainProject
{
    public class Document: INotifyPropertyChanged
    {
        const string UntitledDefaultFileName = "Untitled-{0}.tsf";

        const string ProjectNameTag = "{project}";
        const string CultureCodeTag = "{culture}";
        const string DefaultTranslationTemplateFile = ProjectNameTag + "_" + CultureCodeTag + ".csv";

        private bool __IsUntitled;
        private string __FileName = "";

        private Culture __AssembliesLanguage;

        private AssembliesCollection __Assemblies;
        private List<TranslationCulture> __TranslationCultures;
        private Database __DB;
        private ImportApi __ImportApi;
        private ExportApi __ExportApi;

        public event PropertyChangedEventHandler PropertyChanged;       

        public Document()
        {
            Construct(GetTemporaryFile(), true);
        }

        public Document(string fileName)
        {
            Construct(fileName, false);
        }

        private void Construct(string fileName, bool isUntitled)
        {
            __Assemblies = new AssembliesCollection();
            __FileName = fileName;
            __IsUntitled = isUntitled;

            InitDBAndApi(__FileName);

            RecentProjects.Instance.UpdateUsed(fileName);

            LoadAssemblies();

            // tbd: remove. this string only needed until we cannot get this info from assebly
            __AssembliesLanguage = new Culture("en-US");
        }

        public Culture AssembliesLanguage
        {
            get
            {
                return __AssembliesLanguage;
            }
        }

        private void InitDBAndApi(string fileName)
        {
            __DB = new Database(fileName);
            __ImportApi = new ImportApi(__DB);
            __ExportApi = new ExportApi(__DB);            
            UpdateTranslatedCultures();
        }

        public void UpdateTranslatedCultures()
        {
            __TranslationCultures = __DB.GetCultureCodesWithStatistics();
            __TranslationCultures.Sort((c1, c2) =>
            {
                try
                {
                    Culture cult1 = new Culture(c1.CultureCode);
                    Culture cult2 = new Culture(c2.CultureCode);

                    return cult1.Description.CompareTo(cult2.Description);
                } catch
                {
                    return c1.CultureCode.CompareTo(c2.CultureCode);
                }
            });

            DoPropertyChange("Cultures");
        }

        private void LoadAssemblies()
        {            
            foreach (var assembly in __DB.LocalizableAssemblies())
                __Assemblies.Add(new TranslatedAssembly(assembly));
        }

        private void SaveAssemblies()
        {
            List<LocalizableAssembly> assemblies = new List<LocalizableAssembly>();
            var res = from assembly in __Assemblies select assembly.Assembly;
            __DB.SaveAssemblies(res.ToList());
        }

        private string GetTemporaryFile()
        {
            for (int i = 0; i < 10000; i++)
            {
                var fileName = Path.Combine(Path.GetTempPath(), String.Format(UntitledDefaultFileName, i));

                if (!File.Exists(fileName))
                    return fileName;
            }

            return Path.GetTempFileName();
        }

        public bool Save(string fileName)
        {
            var oldFileName = __FileName;

            __DB.CloseAndMoveTo(fileName);
            InitDBAndApi(fileName);
            
            __IsUntitled = false;
            __FileName = fileName;

            RecentProjects.Instance.Remove(oldFileName);
            RecentProjects.Instance.UpdateUsed(fileName);

            DoPropertyChange("FileName");
            return true;
        }

        private static bool SaveXMLDefinition(string fileName)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            XmlWriter wr = XmlWriter.Create(fileName, settings);

            wr.WriteStartElement("TranslationProject");
            wr.WriteEndElement();
            wr.Close();

            return true;
        }
        
        internal bool LoadFromFile(string fileName)
        {
            __FileName = fileName;
            DoPropertyChange("FileName");

            return true;
        }

        internal void Close()
        {
            __DB.Close();

            if (__IsUntitled)
            {
                System.IO.File.Delete(__FileName);
                RecentProjects.Instance.Remove(__FileName);
            }
        }

        internal void UpdateAsseblies(
            AvailableAssembliesCollection availableAssembliesCollection)
        {
            __Assemblies.Clear();
            foreach (var selectedAssembly in
                availableAssembliesCollection.Where(a => a.IsSelected == true))
            {
                __Assemblies.Add(new TranslatedAssembly(selectedAssembly.Assembly));
            }

            SaveAssemblies();
        }

        internal void AddLanguage(Culture culture)
        {
            __DB.AddCulture(culture.CultureCode);
            UpdateTranslatedCultures();
        }

        internal string GetTranslationFileNameForCulture(string cultureCode)
        {
            string templateFile = TranslationFileNameTemplate;
            templateFile = templateFile.Replace(ProjectNameTag, System.IO.Path.GetFileNameWithoutExtension(FileName));
            templateFile = templateFile.Replace(CultureCodeTag, cultureCode.ToLower());

            return templateFile;
        }

        internal void RemoveLanguage(string cultureCode)
        {
            __DB.RemoveCulture(cultureCode);
            UpdateTranslatedCultures();
        }

    #region Public Properties

        public AssembliesCollection Assemblies
        {
            get { return __Assemblies; }
        }

        public string PathToBinaries
        {
            get
            {
                return __DB.GetStringValue("PathToBinaries", "");
            }

            set
            {
                __DB.SetStringValue("PathToBinaries", value);
                DoPropertyChange("PathToBinaries");
            }
        }

        public string TranslationFileNameTemplate
        {
            get
            {
                return __DB.GetStringValue("TranslationFileNameTemplate", DefaultTranslationTemplateFile);
            }

            set
            {
                __DB.SetStringValue("TranslationFileNameTemplate", value);
                DoPropertyChange("TranslationFileNameTemplate");
            }
        }

        public ImportApi ImportApi
        {
            get
            {
                return __ImportApi;
            }
        }

        public ExportApi ExportApi
        {
            get
            {
                return __ExportApi;
            }
        }

        public string FileName
        {
            get
            {
                return __FileName;
            }
        }

        public bool IsUntitled
        {
            get
            {
                return __IsUntitled;
            }
        }

        void DoPropertyChange(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }       

        public int StringsInDB
        {
            get
            {
                return __ImportApi.ActiveStringsCount();
            }
        }

        public List<TranslationCulture> Cultures
        {
            get
            {
                return __TranslationCultures;
            }
        }

        public string LastExportPath
        {
            get
            {
                return __DB.GetStringValue("LastExportPath", "");
            }

            set
            {
                __DB.SetStringValue("LastExportPath", value);
            }
        }

        public string LastImportPath 
        {
            get
            {
                return __DB.GetStringValue("LastImportPath", "");
            }

            set
            {
                __DB.SetStringValue("LastImportPath", value);
            }
        }
        

    #endregion
    }
}
