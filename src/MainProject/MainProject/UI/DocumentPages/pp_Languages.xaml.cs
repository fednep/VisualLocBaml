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
using System.ComponentModel;
using System.Linq;
using System.Text;
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
    /// Interaction logic for pp_Langages.xaml
    /// </summary>
    public partial class 
        pp_Languages : Page, INotifyPropertyChanged
    {

        private ObservableCollection<CultureItem> __Cultures;

        public pp_Languages()
        {
            InitializeComponent();

            Loaded += pp_Languages_Loaded;

            DataContext = this;
            __Cultures = new ObservableCollection<CultureItem>();
        }

        private void RemoveAssembly_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        void pp_Languages_Loaded(object sender, RoutedEventArgs e)
        {
            FillCultures();
        }

        private void FillCultures()
        {
            int totalStrings = Document.StringsInDB;

            List<CultureItem> cultures = new List<CultureItem>();

            foreach (var culture in Document.Cultures)
            {                
                int percents = 0;
                if (totalStrings > 0)
                    percents = culture.StringsTranslated * 100 / totalStrings;

                percents = Math.Min(100, percents);

                cultures.Add(
                    new CultureItem(culture, 
                        App.MainWindow.Document.GetTranslationFileNameForCulture(culture.CultureCode), percents));
            }

            cultures.Sort( (culture1, culture2) => culture1.Language.CompareTo(culture2.Language));

            __Cultures.Clear();
            foreach (var culture in cultures)
                __Cultures.Add(culture);
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void DoPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void AddLanguage(object sender, RoutedEventArgs e)
        {
            var pp_NewLanguage = new pp_NewLanguage();
            App.MainWindow.AnimatedNavigate(App.MainWindow.DocumentPageFrame, pp_NewLanguage);
        }

        private void Export(object sender, RoutedEventArgs e)
        {
            pp_LanguagesExport exportPage = new pp_LanguagesExport();
            exportPage.Cultures = Cultures;
            exportPage.AllCultures.IsChecked = true;

            App.MainWindow.AnimatedNavigate(App.MainWindow.MiddleLayerFrame, exportPage);
        }

        private void Import(object sender, RoutedEventArgs e)
        {
            pp_LanguagesImport importPage = new pp_LanguagesImport();
            App.MainWindow.AnimatedNavigate(App.MainWindow.MiddleLayerFrame, importPage);            
        }

        public ObservableCollection<CultureItem> Cultures
        {
            get
            {
                return __Cultures;
            }
        }

        public Document Document
        {
            get
            {
                return App.MainWindow.Document;
            }
        }

        private void LanguageOpen(object sender, RoutedEventArgs e)
        {
            Button pressedButton = sender as Button;

            CultureItem selectedCulture = (CultureItem)pressedButton.Tag;

            p_Translation translationPage = new p_Translation();
            translationPage.SelectedCulture = selectedCulture.DocumentCulture;

            App.MainWindow.AnimatedNavigate(App.MainWindow.MiddleLayerFrame, translationPage);            
        }

        private async void RemoveLanguage(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem == null)
                return;

            CultureItem culture = menuItem.Tag as CultureItem;
            if (culture == null)
                return;

            string message = StringUtils.String("MB_RemoveLanguage", culture.Culture.Description);

            MessageBoxResult mbResult = await App.MainWindow.ShowMessage(message);            
            if (mbResult == MessageBoxResult.Yes)
            {
                App.MainWindow.Document.RemoveLanguage(culture.Culture.CultureCode);
                Cultures.Remove(culture);
            }
        }
    }
}
