
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

using TranslationApi.Baml;
using MainProject.DocumentPages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TranslationApi;

namespace MainProject.UI.DocumentPages
{
    /// <summary>
    /// Interaction logic for pp_LanguagesExport.xaml
    /// </summary>
    public partial class pp_GenerateAssemblies : Page
    {
        private string __OutpuFolder;
        private ObservableCollection<CultureItem> __Cultures;
        private ObservableCollection<AssemblyItem> __Assemblies;
        private bool __ExportAllStrings;

        public pp_GenerateAssemblies()
        {
            __OutpuFolder = "";

            InitializeComponent();
            DataContext = this;

            __Cultures = new ObservableCollection<CultureItem>();
            __Assemblies = new ObservableCollection<AssemblyItem>();

            var document = App.MainWindow.Document;

            OutputFolder = document.PathToBinaries;

            foreach (var assembly in document.Assemblies)
                __Assemblies.Add(new AssemblyItem(assembly));

            foreach (var culture in document.Cultures)
                __Cultures.Add(new CultureItem(culture, "", 0));

            __ExportAllStrings = false;

            AllAssemblies.IsChecked = true;
            AllCultures.IsChecked = true;
        }

        public string OutputFolder
        {
            get
            {
                return __OutpuFolder;
            }

            set
            {
                __OutpuFolder = value;
                
            }
        }

        public ObservableCollection<CultureItem> Cultures
        {
            get
            {
                return __Cultures;
            }

            set
            {
                __Cultures = value;
            }
        }

        public ObservableCollection<AssemblyItem> Assemblies
        {
            get
            {
                return __Assemblies;
            }

            set
            {
                __Assemblies = value;
            }
        }

        public bool ExportAllStrings
        {
            get
            {
                return __ExportAllStrings;
            }

            set
            {
                __ExportAllStrings = value;
            }
        }

        private bool __ModifyingCultures;
        private bool __ModifyingAssemblies;
        private void AllCulturesChecked(object sender, RoutedEventArgs e)
        {
            __ModifyingCultures = true;

            try
            {
                foreach (var culture in Cultures)
                {
                    culture.IsSelected = AllCultures.IsChecked == true;
                }
            }
            finally
            {
                __ModifyingCultures = false;
            }
        }

        private void CultureChecked(object sender, RoutedEventArgs e)
        {
            if (__ModifyingCultures)
                return;

            int checkedCount = 0;
            int uncheckedCount = 0;

            foreach (var culture in Cultures)
            {
                if (culture.IsSelected)
                    checkedCount += 1;
                else
                    uncheckedCount += 1;
            }

            if (checkedCount == 0)
                AllCultures.IsChecked = false;
            else if (uncheckedCount == 0)
                AllCultures.IsChecked = true;
            else
                AllCultures.IsChecked = null;
        }

        private void AllAssembliesChecked(object sender, RoutedEventArgs e)
        {
            __ModifyingAssemblies = true;

            try
            {
                foreach (var assembly in Assemblies)
                {
                    assembly.IsSelected = AllAssemblies.IsChecked == true;
                }
            }
            finally
            {
                __ModifyingAssemblies = false;
            }
        }

        private void AssemblyChecked(object sender, RoutedEventArgs e)
        {
            if (__ModifyingAssemblies)
                return;

            int checkedCount = 0;
            int uncheckedCount = 0;

            foreach (var assembly in Assemblies)
            {
                if (assembly.IsSelected)
                    checkedCount += 1;
                else
                    uncheckedCount += 1;
            }

            if (checkedCount == 0)
                AllAssemblies.IsChecked = false;
            else if (uncheckedCount == 0)
                AllAssemblies.IsChecked = true;
            else
                AllAssemblies.IsChecked = null;
        }

        private async Task<object> GenerateAllAssemblies(IProgress<string> progress, CancellationToken cancelToken)
        {

            var document = App.MainWindow.Document;

            LocalizationAppDomain appDomain = await StringsRetriever.CreateAppDomain(document.PathToBinaries);
            List<AssemblyGenerationResult> resultsList = new List<AssemblyGenerationResult>();            

            Dictionary<string, LoadedAssembly> loadedAssemblies = new Dictionary<string, LoadedAssembly>();

            foreach (var cultureItem in Cultures)
            {
                if (!cultureItem.IsSelected)
                    continue;

                foreach (var assemblyItem in Assemblies)
                {

                    if (!assemblyItem.IsSelected)
                        continue;

                    string satteliteTargetFileName = System.IO.Path.GetFileName(assemblyItem.Assembly.Assembly.DefaultResourceFile);
                    AssemblyGenerationResult result = new AssemblyGenerationResult(satteliteTargetFileName, cultureItem.Culture.CultureCode);

                    if (cancelToken.IsCancellationRequested)
                    {
                        result.IsError = true;
                        result.Log.LogLine(StringUtils.String("AssemblyResult_Cancelled"), LogSeverity.Info);
                        result.ResultMessage = StringUtils.String("AssemblyResult_Cancelled");
                        continue;
                    }

                    try
                    {
                        string assemblyToLoad = System.IO.Path.Combine(document.PathToBinaries, 
                                                                       assemblyItem.Assembly.Assembly.DefaultResourceFile);
                        LoadedAssembly loadedAssembly;

                        if (loadedAssemblies.ContainsKey(assemblyToLoad))
                            loadedAssembly = loadedAssemblies[assemblyToLoad];
                        else
                        {
                            loadedAssembly = await Task.Run<LoadedAssembly>(() =>
                            {
                                return appDomain.LoadAssembly(assemblyToLoad);
                            });
                            loadedAssemblies.Add(assemblyToLoad, loadedAssembly);
                        }

                        string outputDirectory = System.IO.Path.Combine(document.PathToBinaries, cultureItem.Culture.CultureCode);

                        if (!Directory.Exists(outputDirectory))
                            Directory.CreateDirectory(outputDirectory);

                        string satteliteTargetFullPath = System.IO.Path.Combine(outputDirectory, satteliteTargetFileName);

                        progress.Report(StringUtils.String("GenerationAssemblyForCulture", satteliteTargetFileName, cultureItem.Culture.CultureCode));

                        await Task.Run(() =>
                        {
                            List<BamlString> translatedStrings = document.ExportApi.GetTranslatedStrings(
                                assemblyItem.Assembly.Assembly.AssemblyFile, cultureItem.Culture.CultureCode);
                            loadedAssembly.GenerateSatteliteAssembly(cultureItem.Culture.CultureCode, translatedStrings, satteliteTargetFullPath);
                        });

                        result.ResultMessage = StringUtils.String("AssemblyResult_Created");
                        resultsList.Add(result);
                    }
                    catch (Exception e)
                    {
                        result.IsError = true;
                        result.Log.LogLine(e.Message, LogSeverity.Error);
                        result.ResultMessage = StringUtils.String("AssemblyResult_Exception");

                        resultsList.Add(result);
                        continue;
                    }
                }
            }

            return resultsList;
        }

        private string SatteliteFileName(TranslatedAssembly assembly, TranslationCulture culture)
        {
            return System.IO.Path.Combine(
                    culture.CultureCode,
                    System.IO.Path.GetFileNameWithoutExtension(assembly.Assembly.AssemblyFile) + ".resources.dll");
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            p_DocumentMenu menu = new p_DocumentMenu();
            menu.DocumentPageFrame.Navigate(new home());

            App.MainWindow.AnimatedNavigate(App.MainWindow.MiddleLayerFrame, menu, NavigationAnimation.FadeToRight);
        }

        private async void Generate(object sender, RoutedEventArgs e)
        {
            List<AssemblyGenerationResult> results = (List<AssemblyGenerationResult>)
                                                            await App.MainWindow.ExecuteLongTask(GenerateAllAssemblies);

            pp_GenerateAssembliesResult resultPage = new pp_GenerateAssembliesResult();
            resultPage.Results = results;

            p_DocumentMenu menu = new p_DocumentMenu();
            menu.DocumentPageFrame.Content = resultPage;

            App.MainWindow.AnimatedNavigate(App.MainWindow.MiddleLayerFrame, menu, NavigationAnimation.FadeToLeft, false);
        }
    }
}
