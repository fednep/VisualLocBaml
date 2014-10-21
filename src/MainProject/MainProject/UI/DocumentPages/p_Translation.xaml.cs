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


using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
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

namespace MainProject.UI.DocumentPages
{
    /// <summary>
    /// Interaction logic for p_Translation.xaml
    /// </summary>
    public partial class p_Translation : Page, INotifyPropertyChanged
    {
        private TranslationBamlConverter __BamlConverter;
        private TranslationCulture __SelectedCulture;
        private bool __IsRetrivingInProgress;
        private DataView __DataView;
        private DataTable __DataTable;

        public p_Translation()
        {
            __BamlConverter = new TranslationBamlConverter();

            InitializeComponent();
            DataContext = this;

            IsRetrivingInProgress = false;
        }

        private async void CultureChanged(object sender, SelectionChangedEventArgs e)
        {

            ProgressMessage.Text = StringUtils.String("Progress_RetrievingTranslation");
            IsFilterPopupOpen = false;
            IsRetrivingInProgress = true;            

            try
            {
                await RetrieveStrings();

            }
            finally
            {
                IsRetrivingInProgress = false;
            }
            //..
        }

        private async Task RetrieveStrings()
        {
            var document = App.MainWindow.Document;
            __DataTable = await Task<DataTable>.Run(() =>
            {
                return document.ExportApi.GetTranslationStringsDataTable(SelectedCulture.CultureCode);
            });

            __DataTable.ColumnChanging += translationStrigs_ColumnChanging;

            __DataView = __DataTable.AsDataView();
            Assemblies = (from rows in __DataTable.AsEnumerable() select rows.Field<string>("filename")).Distinct();
            SelectBamlFiles();
            StringsDataGrid.ItemsSource = null;
            StringsDataGrid.ItemsSource = __DataView;            
        }

        void translationStrigs_ColumnChanging(object sender, DataColumnChangeEventArgs e)
        {
            App.MainWindow.Document.ExportApi.SetTranslation(e.Row, e.ProposedValue);
        }

        public List<TranslationCulture> Cultures
        {
            get
            {
                return App.MainWindow.Document.Cultures;
            }
        }

        public TranslationCulture SelectedCulture
        {
            get
            {
                return __SelectedCulture;
            }

            set
            {
                __SelectedCulture = value;
            }
        }

        public bool IsRetrivingInProgress
        {
            get
            {
                return __IsRetrivingInProgress;
            }

            set
            {
                __IsRetrivingInProgress = value;
                DoPropretyChanged("IsRetrivingInProgress");
            }
        }

        private void Search(object sender, RoutedEventArgs e)
        {
            IsSearchPopupOpen = !IsSearchPopupOpen;
        }

        private void Filter(object sender, RoutedEventArgs e)
        {
            IsFilterPopupOpen = !IsFilterPopupOpen;
        }

        private IEnumerable<string> __Assemblies;

        public IEnumerable<string> Assemblies
        {
            get
            {
                return __Assemblies;
            }

            set
            {
                __Assemblies = value;
                DoPropretyChanged("Assemblies");
            }
        }

        private IEnumerable __BamlFiles;
        public IEnumerable BamlFiles
        {
            get
            {
                return __BamlFiles;
            }
            set
            {
                __BamlFiles = value;
                DoPropretyChanged("BamlFiles");
            }
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            if (IsRetrivingInProgress)
                return;

            p_DocumentMenu menu = new p_DocumentMenu();
            pp_Languages languages = new pp_Languages();
            menu.DocumentPageFrame.Content = languages;

            App.MainWindow.AnimatedNavigate(App.MainWindow.MiddleLayerFrame, menu);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void DoPropretyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ClearFilter(object sender, RoutedEventArgs e)
        {
            FilterBaml.Text = "";
            FilterAssembly.Text = "";
            FilterString.Text = "";

            __DataView.RowFilter = "";

            IsFilterPopupOpen = false;
            DoPropretyChanged("IsFilterEnabled");
        }

        private void ApplyFilter(object sender, RoutedEventArgs e)
        {
            string assemblyFilter = FilterAssembly.SelectedItem as string;
            string bamlFilter = FilterBaml.SelectedItem as string;
            string stringFilter = FilterString.Text;

            List<string> filterTerms = new List<string>();

            if (!string.IsNullOrEmpty(assemblyFilter))
            {
                filterTerms.Add(
                    String.Format("filename = '{0}'", assemblyFilter.Replace("'", " ")));
            }

            if (!string.IsNullOrEmpty(bamlFilter))
            {
                filterTerms.Add(
                    String.Format("baml like '*{0}'", bamlFilter.Replace("'", " ")));
            }

            if (!string.IsNullOrEmpty(stringFilter))
            {
                filterTerms.Add(
                    String.Format(@"(string LIKE '*{0}*' OR 
                                    translation LIKE '*{0}*')", stringFilter.Replace("'", " ")));
            }

            __DataView.RowFilter = String.Join(" AND ", filterTerms.ToArray());
            DoPropretyChanged("IsFilterEnabled");

            IsFilterPopupOpen = false;
        }

        public bool IsFilterEnabled
        {
            get
            {
                if (__DataView == null)
                    return false;

                return __DataView.RowFilter != "";
            }
        }

        bool __FilterPopupOpen;

        public bool IsFilterPopupOpen
        {
            get
            {
                return __FilterPopupOpen;
            }
            set
            {
                __FilterPopupOpen = value;
                DoPropretyChanged("IsFilterPopupOpen");
            }
        }

        bool __IsSearchPopupOpen;

        public bool IsSearchPopupOpen
        {
            get
            {
                return __IsSearchPopupOpen;
            }
            set
            {
                __IsSearchPopupOpen = value;
                DoPropretyChanged("IsSearchPopupOpen");
            }
        }

        private void DoSearch(object sender, RoutedEventArgs e)
        {

        }

        private void FilterAssemblyChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectBamlFiles();
        }

        private void SelectBamlFiles()
        {
            string assemblyFileName = FilterAssembly.SelectedItem as string;

            if (string.IsNullOrEmpty(assemblyFileName))
                BamlFiles = (from rows in __DataTable.AsEnumerable()
                             select __BamlConverter.Convert(rows.Field<string>("baml"), typeof(string), null, null)).Distinct();
            else
                BamlFiles = (from rows in __DataTable.AsEnumerable()
                             where rows.Field<string>("filename") == assemblyFileName
                             select __BamlConverter.Convert(rows.Field<string>("baml"), typeof(string), null, null)).Distinct();
        }

        private async void Import(object sender, RoutedEventArgs e)
        {
            string fileName = App.MainWindow.Document.GetTranslationFileNameForCulture(__SelectedCulture.CultureCode);

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = StringUtils.String("TranslationFileFilter");
            dialog.FileName = fileName;
            if (dialog.ShowDialog() == false)
                return;

            List<ImportTranslationResult> resultsList = new List<ImportTranslationResult>();

            ProgressMessage.Text = StringUtils.String("ImportStringsTranslation", 
                                        __SelectedCulture.CultureCode, System.IO.Path.GetFileName(dialog.FileName));

            ImportTranslationResult result = null;
            IsRetrivingInProgress = true;
            try
            {
                result = await App.MainWindow.Document.ImportApi.ImportTranslation(
                                                                    dialog.FileName,
                                                                    __SelectedCulture.CultureCode);

                if (String.IsNullOrEmpty(result.ErrorMessage))
                    await RetrieveStrings();
            }
            finally
            {
                IsRetrivingInProgress = false;
            }

            if (!String.IsNullOrEmpty(result.ErrorMessage))
                await App.MainWindow.ShowMessage(result.ErrorMessage, MessageBoxButton.OK);

            App.MainWindow.Document.UpdateTranslatedCultures();
        }

        private async void Export(object sender, RoutedEventArgs e)
        {
            string fileName = App.MainWindow.Document.GetTranslationFileNameForCulture(__SelectedCulture.CultureCode);

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = StringUtils.String("TranslationFileFilter");
            dialog.FileName = fileName;
            if (dialog.ShowDialog() == false)
                return;

            ProgressMessage.Text = StringUtils.String("ExportingStrings", 
                                      __SelectedCulture.CultureCode, System.IO.Path.GetFileName(dialog.FileName));

            IsRetrivingInProgress = true;

            Exception errorException = null;
            try
            {
                var result = await App.MainWindow.Document.ExportApi.ExportStrings(
                                                dialog.FileName,
                                                __SelectedCulture.CultureCode,
                                                true);
                IsRetrivingInProgress = false;                
            }
            catch (Exception exc)
            {
                errorException = exc;
                IsRetrivingInProgress = false;
            }

            if (errorException != null)
                await App.MainWindow.ShowMessage(errorException.Message, MessageBoxButton.OK);
        }
    }
}
