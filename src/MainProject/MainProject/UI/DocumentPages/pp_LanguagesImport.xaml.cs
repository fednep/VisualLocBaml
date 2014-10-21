
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

using MainProject.DocumentPages;
using Microsoft.Win32;
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TranslationApi;
using TranslationApi.KBCsv;

namespace MainProject.UI.DocumentPages
{
    /// <summary>
    /// Interaction logic for pp_LanguagesExport.xaml
    /// </summary>
    public partial class pp_LanguagesImport : Page
    {

        private ObservableCollection<FileToImport> __FilesToImport;

        public pp_LanguagesImport()
        {

            InitializeComponent();
            __FilesToImport = new ObservableCollection<FileToImport>();
            DataContext = this;
        }

        private void AddFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = StringUtils.String("TranslationFileFilter");
            openFileDialog.Multiselect = true;

            var lastImportPath = App.MainWindow.Document.LastImportPath;
            if (lastImportPath != "")
                openFileDialog.InitialDirectory = lastImportPath;

            if (openFileDialog.ShowDialog() == true)
            {
                var files = openFileDialog.FileNames;
                if (files.Count() > 0)
                {
                    App.MainWindow.Document.LastImportPath = System.IO.Path.GetDirectoryName(files[0]);

                    foreach (var file in files)
                    {
                        TryAddFile(file);
                    }
                }
            }
        }

        private void TryAddFile(string fileName)
        {
            var file = FileToImport.FromFile(fileName);
            if (file == null)
                return;

            foreach (var fileToImport in __FilesToImport)
                if (fileToImport.Culture.CultureCode == file.Culture.CultureCode)
                    return;

            __FilesToImport.Add(file);
        }

        public ObservableCollection<FileToImport> FilesToImport
        {
            get
            {
                return __FilesToImport;
            }
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            p_DocumentMenu menu = new p_DocumentMenu();
            menu.DocumentPageFrame.Navigate(new pp_Languages());

            App.MainWindow.AnimatedNavigate(App.MainWindow.MiddleLayerFrame, menu, NavigationAnimation.FadeToRight);
        }

        private async Task<object> ImportTranslation(IProgress<string> progress, CancellationToken cancelToken)
        {
            List<ImportTranslationResult> resultsList = new List<ImportTranslationResult>();

            foreach (FileToImport fileToImport in FilesToImport)
            {
                progress.Report(StringUtils.String("ImportStringsTranslation", fileToImport.Culture.CultureCode, fileToImport.RelativeFileName));

                ImportTranslationResult result = 
                    await App.MainWindow.Document.ImportApi.ImportTranslation(fileToImport.FileName, fileToImport.Culture.CultureCode);

                resultsList.Add(result);
            }

            App.MainWindow.Document.UpdateTranslatedCultures();

            return resultsList;
        }

        private async void Import(object sender, RoutedEventArgs e)
        {
             List<ImportTranslationResult> resultsList = (List<ImportTranslationResult>)
                 await App.MainWindow.ExecuteLongTask(ImportTranslation);

             pp_ImportTranslationResult importResultPage = new pp_ImportTranslationResult();
             importResultPage.ImportResults = resultsList;

             p_DocumentMenu menuPage = new p_DocumentMenu();
             menuPage.DocumentPageFrame.Content = importResultPage;

             App.MainWindow.AnimatedNavigate(App.MainWindow.MiddleLayerFrame, menuPage, NavigationAnimation.FadeToLeft, false);
        }
    }
}
