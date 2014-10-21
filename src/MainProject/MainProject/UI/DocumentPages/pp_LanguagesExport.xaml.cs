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
    public partial class pp_LanguagesExport : Page
    {
        private string __OutpuFolder;
        private ObservableCollection<CultureItem> __Cultures;
        private bool __ExportAllStrings;

        public pp_LanguagesExport()
        {
            __OutpuFolder = "";

            InitializeComponent();
            DataContext = this;

            __ExportAllStrings = false;
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

        private void SelectFolder(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = ExportFolder.Text == "" ?  App.MainWindow.Document.LastExportPath : ExportFolder.Text;

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                ExportFolder.Text = folderBrowserDialog.SelectedPath;
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            p_DocumentMenu menu = new p_DocumentMenu();
            menu.DocumentPageFrame.Navigate(new pp_Languages());

            App.MainWindow.AnimatedNavigate(App.MainWindow.MiddleLayerFrame, menu, NavigationAnimation.FadeToRight);
        }

        private async Task<object> ExportForTranslation(IProgress<string> progress, CancellationToken cancelToken)
        {
            List<ExportResult> resultsList = new List<ExportResult>();
            foreach (var culture in __Cultures)
            {
                if (culture.IsSelected)
                {
                    string fileName = System.IO.Path.Combine(ExportFolder.Text, culture.FileName);

                    progress.Report(StringUtils.String("ExportingStrings", culture.CountryCode, culture.FileName));

                    try
                    {                        
                        ExportResult exportResult = await App.MainWindow.Document.ExportApi.ExportStrings(
                                                        fileName, 
                                                        culture.Culture.CultureCode, 
                                                        IncludeTranslated.IsChecked == true);
                        resultsList.Add(exportResult);
                    }
                    catch (Exception e)
                    {
                        ExportResult result = new ExportResult(fileName, culture.Culture.CultureCode, ExportCSVResult.Fail, 0, e.Message);
                        resultsList.Add(result);
                    }
                }
            }

            return resultsList;
        }

        private async void Export(object sender, RoutedEventArgs e)
        {

            List<ExportResult> result = (List<ExportResult>)
                await App.MainWindow.ExecuteLongTask(ExportForTranslation);

            App.MainWindow.Document.LastExportPath = ExportFolder.Text;

            pp_LanguagesExportResult resultPage = new pp_LanguagesExportResult();
            resultPage.ResultList = result;

            p_DocumentMenu documentMenu = new p_DocumentMenu();
            documentMenu.DocumentPageFrame.Content = resultPage;

            App.MainWindow.AnimatedNavigate(App.MainWindow.MiddleLayerFrame, documentMenu, NavigationAnimation.FadeToLeft, false);
        }
    }
}
