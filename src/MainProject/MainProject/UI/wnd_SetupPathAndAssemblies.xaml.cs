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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using BamlLocalization;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using TranslationApi;
using MainProject.UI;

namespace MainProject
{



    /// <summary>
    /// Interaction logic for wnd_SetupPathAndAssemblies.xaml
    /// </summary>
    public partial class wnd_SetupPathAndAssemblies : Window, INotifyPropertyChanged
    {
        private AvailableAssembliesCollection __AvailableAssemblies;
        private string __PathToBinaries = "";
        private string __DefaultCulture;

        public wnd_SetupPathAndAssemblies()
        {
            InitializeComponent();

            __AvailableAssemblies = new AvailableAssembliesCollection();
            SetDefaultCulture();
            DataContext = this;
        }

        private void SetDefaultCulture()
        {
            __DefaultCulture = StringUtils.String("CultureUnknown");
        }        

        private void ChangePath_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderBrowser();
        }

        public bool OpenFolderBrowser()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = PathToBinaries;
            var result = folderBrowserDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                PathToBinaries = folderBrowserDialog.SelectedPath;                
                return true;
            }

            return false;
        }

        public AvailableAssembliesCollection AvailableAssemblies
        {
            get
            {
                return __AvailableAssemblies;
            }
        }

        public string PathToBinaries
        {
            get
            {
                return __PathToBinaries;
            }

            set
            {
                __PathToBinaries = value;
                SearchForLocalizableAssemblies();
                DoPropertyChange("PathToBinaries");
            }
        }

        private void SearchForLocalizableAssemblies()
        {
            List <LocalizableAssembly> localizableAssemblies = new List<LocalizableAssembly>();

            using (var appDomain = new LocalizationAppDomain(__PathToBinaries))
            {


                foreach (var fullFilePath in Directory.EnumerateFiles(__PathToBinaries, "*.exe"))
                {                    
                    CheckAndAddAssembly(
                            localizableAssemblies, 
                            System.IO.Path.GetFileName(fullFilePath), 
                            __PathToBinaries, 
                            appDomain);
                }

                foreach (var fullFilePath in Directory.EnumerateFiles(__PathToBinaries, "*.dll"))
                {
                    CheckAndAddAssembly(
                            localizableAssemblies,
                            System.IO.Path.GetFileName(fullFilePath),
                            __PathToBinaries,
                            appDomain);
                }
            }

            SetDefaultCulture();
            __AvailableAssemblies.Clear();
                        
            if (localizableAssemblies.Count > 0)
                DefaultCulture = localizableAssemblies[0].DefaultCulture;

            foreach (var assembly in localizableAssemblies)
            {
                if (assembly.DefaultCulture == DefaultCulture)
                {
                    var selectedAssembly = new AvailableAssemblyForTranslation(__AvailableAssemblies, assembly);
                    selectedAssembly.IsSelected = true;
                    __AvailableAssemblies.Add(selectedAssembly);
                }
            }

            DoPropertyChange("DefaultCulture");
        }

        private void CheckAndAddAssembly(List<LocalizableAssembly> localizableAssemblies, string file, string basePath, LocalizationAppDomain appDomain)
        {
            string fullPathToAssembly = System.IO.Path.Combine(__PathToBinaries, file);
            if (!LoadedAssembly.IsNetAssembly(fullPathToAssembly))
                return;

            LoadedAssembly assembly = appDomain.LoadAssembly(fullPathToAssembly);
            if (assembly.WasExceptionOnLoad)
                return;

            if (assembly.NeutralResourceCultureName == "")
                return;

            var assemblyName = System.IO.Path.GetFileNameWithoutExtension(file);
            var resourceFile = System.IO.Path.Combine(
                                    assembly.NeutralResourceCultureName,
                                    String.Format("{0}.resources.dll", assemblyName));

            var resourceFullPath = System.IO.Path.Combine(
                                __PathToBinaries,
                                resourceFile);

            if (!File.Exists(resourceFullPath))
                return;

            localizableAssemblies.Add(new LocalizableAssembly(file, 
                                                              assembly.NeutralResourceCultureName, 
                                                              resourceFile));
            return;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void DoPropertyChange(string propertyName)
        {
            var handler = PropertyChanged;

            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public string DefaultCulture
        {
            get
            {
                return __DefaultCulture;
            }

            set
            {
                __DefaultCulture = value;
            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            UrlHelper.OpenUrl("http://wpflocalizer.com/guide/localizableassemblies/");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public void FillFromDocument(Document document)
        {
            if  (string.IsNullOrEmpty(__PathToBinaries))
                throw new ArgumentException("Internal error: Path to binaries should be set before call to FillFromDocument");

            foreach (var assembly in document.Assemblies)
            {
                FindAndCheck(assembly);
            }
        }

        private void FindAndCheck(TranslatedAssembly documentAssembly)
        {
            foreach (var assembly in __AvailableAssemblies)
            {
                if (assembly.Assembly.AssemblyFile == documentAssembly.Assembly.AssemblyFile)
                {
                    assembly.IsSelected = true;
                    return;
                }
            }

            var newAssembly = new AvailableAssemblyForTranslation(__AvailableAssemblies, documentAssembly.Assembly);
            newAssembly.IsMissing = true;
            newAssembly.IsSelected = true;
            __AvailableAssemblies.Add(newAssembly);
        }
    }
}
