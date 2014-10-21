
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

using MainProject.UI;
using MainProject.UI.DocumentPages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MainProject
{
    public partial class p_EmptyProject
    {

        public p_EmptyProject()
        {
            this.InitializeComponent();
            DataContext = this;
        }

        private async void NextClick(object sender, RoutedEventArgs e)
        {
            if (!System.IO.Directory.Exists(Path.Text))
            {
                await App.MainWindow.ShowMessage(StringUtils.String("PathNotFound"), MessageBoxButton.OK);                    
                return;
            }

            App.MainWindow.Document.PathToBinaries = Path.Text.Trim();

            p_SetupPathAndAssemblies setupPage = new p_SetupPathAndAssemblies();
            await SetupPathAndAssemblies(setupPage);
        }

        public static async Task SetupPathAndAssemblies(p_SetupPathAndAssemblies setupPage)
        {

            await App.MainWindow.ExecuteLongTask(setupPage.SearchForLocalizableAssemblies);

            if (setupPage.AvailableAssemblies.Count > 0)
                App.MainWindow.AnimatedNavigate(App.MainWindow.MiddleLayerFrame, setupPage, NavigationAnimation.FadeToLeft, false);
            else
            {
                p_AssembliesNotFound noAssemblies = new p_AssembliesNotFound();
                App.MainWindow.AnimatedNavigate(App.MainWindow.MiddleLayerFrame, noAssemblies, NavigationAnimation.FadeToLeft, false);
            }
        }

        private void Browse(object sender, RoutedEventArgs e)
        {
            p_SetupPathAndAssemblies setupPage = new p_SetupPathAndAssemblies();

            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = Path.Text;
            var result = folderBrowserDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Path.Text = folderBrowserDialog.SelectedPath;
            }
        }
    }
}