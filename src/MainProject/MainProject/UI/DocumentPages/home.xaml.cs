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
using MainProject.UI;
using System;
using System.Collections.Generic;
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
using MainProject.UI.DocumentPages;
using TranslationApi;
using System.Globalization;
using System.IO;

namespace MainProject.DocumentPages
{

    /// <summary>
    /// Interaction logic for home.xaml
    /// </summary>
    public partial class home : Page
    {
        private Document __Document;

        public home()
        {
            InitializeComponent();

            __Document = App.MainWindow.Document;
            DataContext = this;

            CommandBindings.Add(new CommandBinding(Commands.RetrieveStrings, new ExecutedRoutedEventHandler(Retrieve_Cmd)));
        }

        private async void Retrieve_Cmd(object sender, ExecutedRoutedEventArgs e)
        {
            StringsRetriever retriever = new StringsRetriever(__Document);

            var currentFrame = App.MainWindow.MainFrame.Content;
            var importResultList = (List<ImportResult>)await App.MainWindow.ExecuteLongTask(retriever.RetrieveAllAssemblies);

            pp_ImportResult resultPage = new pp_ImportResult();
            resultPage.ImportResults = importResultList;

            p_DocumentMenu documentPage = new p_DocumentMenu();
            documentPage.DocumentPageFrame.Navigate(resultPage);

            App.MainWindow.AnimatedNavigate(App.MainWindow.MiddleLayerFrame, documentPage, NavigationAnimation.FadeToRight, false);
        }

        public Document Document
        {
            get
            {
                return __Document;
            }
        }


        private void GenerateAssemblies(object sender, RoutedEventArgs e)
        {
            pp_GenerateAssemblies generateAssemblies = new pp_GenerateAssemblies();
            App.MainWindow.AnimatedNavigate(App.MainWindow.MiddleLayerFrame, generateAssemblies);
        }

    }
}
