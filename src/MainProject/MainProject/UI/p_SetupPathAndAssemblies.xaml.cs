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
using MainProject.UI.DocumentPages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace MainProject.UI
{
    /// <summary>
    /// Interaction logic for p_SetupPathAndAssembly.xaml
    /// </summary>
    public partial class p_SetupPathAndAssemblies : Page, INotifyPropertyChanged
    {
        private AvailableAssembliesCollection __AvailableAssemblies;
        private ObservableCollection<CultureItem> __TranslationCultures;

        private Culture __DefaultCulture;
        private Document __Document;

        public p_SetupPathAndAssemblies()
        {
            InitializeComponent();
            __AvailableAssemblies = new AvailableAssembliesCollection();
            __TranslationCultures = new ObservableCollection<CultureItem>();
            __Document = App.MainWindow.Document;
            DataContext = this;
        }

        private void ChangePath_Click(object sender, RoutedEventArgs e)
        {
            ChooseFolder();
        }

        public bool AddRemoveAssembliesOnly
        { get; set; }

        public async Task<object> SearchForLocalizableAssemblies(IProgress<string> progress, CancellationToken cancelToken)
        {                    
            List<LocalizableAssembly> localizableAssemblies = new List<LocalizableAssembly>();

            List<string> alreadyDefinedCultures = null;

            progress.Report(StringUtils.String("SearchingForAssemblies"));

            await Task.Run(() =>
            {
                using (var appDomain = new LocalizationAppDomain(Document.PathToBinaries))
                {
                    foreach (var fullFilePath in Directory.EnumerateFiles(Document.PathToBinaries, "*.exe"))
                    {
                        CheckAndAddAssembly(
                                localizableAssemblies,
                                System.IO.Path.GetFileName(fullFilePath),
                                Document.PathToBinaries,
                                appDomain);
                    }

                    foreach (var fullFilePath in Directory.EnumerateFiles(Document.PathToBinaries, "*.dll"))
                    {
                        CheckAndAddAssembly(
                                localizableAssemblies,
                                System.IO.Path.GetFileName(fullFilePath),
                                Document.PathToBinaries,
                                appDomain);
                    }

                    alreadyDefinedCultures = FindTranslationCultures(localizableAssemblies);
                }
            });

            __AvailableAssemblies.Clear();

            if (localizableAssemblies.Count > 0)
                DefaultCulture = new Culture(localizableAssemblies[0].DefaultCulture);

            foreach (var assembly in localizableAssemblies)
            {
                if (assembly.DefaultCulture == DefaultCulture.CultureCode)
                {
                    var selectedAssembly = new AvailableAssemblyForTranslation(__AvailableAssemblies, assembly);
                    selectedAssembly.IsSelected = true;
                    __AvailableAssemblies.Add(selectedAssembly);
                }
            }

            __TranslationCultures.Clear();

            if (AddRemoveAssembliesOnly)
                return null;

            if (alreadyDefinedCultures != null)
                FillTranslationCultures(alreadyDefinedCultures);


            return null;
        }

        private void FillTranslationCultures(List<string> alreadyDefinedCultures)
        {
            List<CultureItem> cultureItemsList = new List<CultureItem>();
            foreach (var subdir in alreadyDefinedCultures)
            {
                if (subdir == DefaultCulture.CultureCode.ToLower())
                    continue;

                if (!Culture.IsValid(subdir))
                    continue;

                CultureItem culture = new CultureItem(new TranslationCulture(subdir, 0), "", 0);
                cultureItemsList.Add(culture);
            }

            cultureItemsList.Sort((culture1, culture2) => culture1.Culture.Description.CompareTo(culture2.Culture.Description));

            __TranslationCultures.Clear();
            foreach (var culture in cultureItemsList)
                __TranslationCultures.Add(culture);

            AllCultures.IsChecked = true;
        }

        private List<string> FindTranslationCultures(List<LocalizableAssembly> localizableAssemblies)
        {
            List<string> alreadyDefinedCultures = new List<string>();

            string[] subdirs = System.IO.Directory.GetDirectories(Document.PathToBinaries);
            foreach (var directory in subdirs)
            {
                var directoryName = System.IO.Path.GetFileName(directory);

                foreach (var translationAssembly in localizableAssemblies)
                {
                    string fileName = System.IO.Path.Combine(
                                                        directory,
                                                        System.IO.Path.GetFileNameWithoutExtension(translationAssembly.AssemblyFile) + ".resources.dll");
                    if (File.Exists(fileName))
                    {
                        alreadyDefinedCultures.Add(directoryName.ToLower());
                        break;
                    }
                }
            }
            return alreadyDefinedCultures;
        }

        private void CheckAndAddAssembly(List<LocalizableAssembly> localizableAssemblies, string file, string basePath, LocalizationAppDomain appDomain)
        {
            string fullPathToAssembly = System.IO.Path.Combine(Document.PathToBinaries, file);
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
                                Document.PathToBinaries,
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

        public Culture DefaultCulture
        {
            get
            {
                return __DefaultCulture;
            }

            set
            {
                __DefaultCulture = value;
                DoPropertyChange("DefaultCulture");
            }
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            var document = App.MainWindow.Document;
            document.UpdateAsseblies(AvailableAssemblies);

            if (AddRemoveAssembliesOnly)
            {
                pp_Settings settings = new pp_Settings();

                p_DocumentMenu documentPage = new p_DocumentMenu();
                documentPage.DocumentPageFrame.Content = settings;

                App.MainWindow.AnimatedNavigate(App.MainWindow.MiddleLayerFrame, documentPage, NavigationAnimation.FadeToRight, false);

            } else
            {
                // Retrieve strings
                StringsRetriever retriever = new StringsRetriever(document);

                var importResultList = (List<ImportResult>)await App.MainWindow.ExecuteLongTask(retriever.RetrieveAllAssemblies);

                bool hasStrings = false;
                foreach (var result in importResultList)
                {
                    if (result.New > 0)
                    {
                        hasStrings = true;
                        break;
                    }
                }

                if (hasStrings)
                {
                    await App.MainWindow.ExecuteLongTask(RetrieveTranslation);
                    document.UpdateTranslatedCultures();
                }


                pp_ImportResult resultPage = new pp_ImportResult();
                resultPage.ImportResults = importResultList;

                p_DocumentMenu documentPage = new p_DocumentMenu();
                documentPage.DocumentPageFrame.Navigate(resultPage);

                App.MainWindow.AnimatedNavigate(App.MainWindow.MiddleLayerFrame, documentPage, NavigationAnimation.FadeToRight, false);

                App.MainWindow.LoadDocumentPage();
            }
        }

        private async Task<object> RetrieveTranslation(IProgress<string> progress, CancellationToken cancelToken)
        {
            LocalizationAppDomain appDomain = await StringsRetriever.CreateAppDomain(App.MainWindow.Document.PathToBinaries);

            var document = App.MainWindow.Document;
            foreach (var culture in __TranslationCultures)
            {
                if (!culture.IsSelected)
                    continue;

                document.AddLanguage(culture.Culture);

                StringsRetriever retriever = new StringsRetriever(App.MainWindow.Document);

                foreach (var assembly in document.Assemblies)
                {
                    string resourceFile = System.IO.Path.Combine(
                        culture.Culture.CultureCode,
                        System.IO.Path.GetFileNameWithoutExtension(assembly.Assembly.AssemblyFile) + ".resources.dll"
                        );

                    string resourceFileFullPath = System.IO.Path.Combine(document.PathToBinaries, resourceFile);

                    if (!System.IO.File.Exists(resourceFileFullPath))
                        continue;

                    string progressString = StringUtils.String("Progress_RetrievingAssemblyTranslation", resourceFile);
                    progress.Report(progressString);

                    LoadedAssembly loadedAssembly = await Task.Run<LoadedAssembly>(() =>
                    {
                        return appDomain.LoadAssembly(
                        resourceFileFullPath);
                    });

                    var progressReporter = new ProgressPercentageReporter(progress, progressString);

                    var lines = await retriever.ExtractTranslationLines(loadedAssembly);
                    lines = lines.Where(retriever.LinesFilter).ToList();

                    await document.ImportApi.ImportTranslationStrings(
                        progressReporter,
                        cancelToken,
                        assembly.Assembly.AssemblyFile,
                        document.AssembliesLanguage.CultureCode,
                        culture.Culture.CultureCode,
                        lines);
                }
            }

            await StringsRetriever.DisposeAppDomain(appDomain);

            return null;
        }

        public void FillFromDocument(Document document)
        {
            if (string.IsNullOrEmpty(Document.PathToBinaries))
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

        private void Back(object sender, RoutedEventArgs e)
        {
            if (AddRemoveAssembliesOnly)
                Commands.GoBack.Execute(null, this);
            else
            {
                App.MainWindow.Document.PathToBinaries = "";
                App.MainWindow.AnimatedNavigate(App.MainWindow.MiddleLayerFrame, new p_EmptyProject());
            }
        }

        public bool ChooseFolder()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = App.MainWindow.Document.PathToBinaries;
            var result = folderBrowserDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                App.MainWindow.Document.PathToBinaries = folderBrowserDialog.SelectedPath;
                return true;
            }

            return false;
        }

        private bool __ModifyingCultures;
        private void AllCulturesChecked(object sender, RoutedEventArgs e)
        {
            __ModifyingCultures = true;

            try
            {
                foreach (var culture in TranslationCultures)
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

            foreach (var culture in TranslationCultures)
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

        public Document Document 
        {
            get
            {
                return __Document;
            }
        }

        public AvailableAssembliesCollection AvailableAssemblies
        {
            get
            {
                return __AvailableAssemblies;
            }
        }

        public ObservableCollection<CultureItem> TranslationCultures
        {
            get
            {
                return __TranslationCultures;
            }
        }
    }
}
