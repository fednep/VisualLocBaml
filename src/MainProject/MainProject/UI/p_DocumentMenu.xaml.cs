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
using MainProject.UI.DocumentPages;
using System;
using System.Collections.Generic;
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

namespace MainProject.UI
{
    /// <summary>
    /// Interaction logic for p_DocumentMenu.xaml
    /// </summary>
    public partial class p_DocumentMenu : Page, INotifyPropertyChanged
    {
        public p_DocumentMenu()
        {
            InitializeComponent();

            DataContext = this;
            DocumentPageFrame.Navigated += DocumentPageFrame_Navigated;
        }

        void DocumentPageFrame_Navigated(object sender, NavigationEventArgs e)
        {
            DoPropertyChanged("CurrentPageTag");
            DoPropertyChanged("IsSelectedMenuEnabled");
        }

        public string CurrentPageTag
        {
            get
            {
                Page currentPage = DocumentPageFrame.Content as Page;
                if (currentPage == null)
                    return "";

                return PageProperties.GetPageTag(currentPage);
            }
        }

        public bool IsSelectedMenuEnabled
        {
            get
            {
                Page currentPage = DocumentPageFrame.Content as Page;
                if (currentPage == null)
                    return true;

                return PageProperties.GetEnableMenuItem(currentPage);
            }
        }

        public Document Document
        {
            get
            {
                return App.MainWindow.Document;
            }
        }

        private void NavigateHome(object sender, RoutedEventArgs e)
        {
            home homePage = new home();

            //NavigationAnimation fadeDirection = NavigationAnimation.FadeToLeft;
            //if (CurrentPageTag == "home")
            //    fadeDirection = NavigationAnimation.FadeToRight;

            App.MainWindow.AnimatedNavigate(App.MainWindow.DocumentPageFrame, homePage, NavigationAnimation.FadeToRight);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void DoPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void NavigateLanguages(object sender, RoutedEventArgs e)
        {
            pp_Languages languagesPage = new pp_Languages();
            App.MainWindow.AnimatedNavigate(App.MainWindow.DocumentPageFrame, languagesPage, NavigationAnimation.FadeToRight);
        }

        private void NavigateSettings(object sender, RoutedEventArgs e)
        {
            pp_Settings settingsPage = new pp_Settings();
            App.MainWindow.AnimatedNavigate(App.MainWindow.DocumentPageFrame, settingsPage, NavigationAnimation.FadeToRight);
        }
    }
}
