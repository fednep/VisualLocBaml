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
    public partial class pp_Settings : Page
    {
        private AvailableAssembliesCollection __Assemblies;

        public pp_Settings()
        {
            InitializeComponent();
            DataContext = this;

            __Assemblies = new AvailableAssembliesCollection();

            foreach (var assembly in Document.Assemblies)
            {
                var assemblyItem = new AvailableAssemblyForTranslation(__Assemblies, assembly.Assembly);               
                __Assemblies.Add(assemblyItem);
            }

            FillAssembliesToRemove();
        }

        private void FillAssembliesToRemove()
        {
            
        }

        public Document Document
        {
            get
            {
                return App.MainWindow.Document;
            }
        }

        private void BrowsePathToBinaries(object sender, RoutedEventArgs e)
        {
            p_SetupPathAndAssemblies setupPage = new p_SetupPathAndAssemblies();
            if (!setupPage.ChooseFolder())
                return;
        }

        private void AssemblyChacked(object sender, RoutedEventArgs e)
        {

        }

        public AvailableAssembliesCollection Assemblies
        {
            get
            {
                return __Assemblies;
            }
        }

        bool __IsAddAssemblyVisible;

        private async void AddRemoveAssembly(object sender, RoutedEventArgs e)
        {
            p_SetupPathAndAssemblies setupPage = new p_SetupPathAndAssemblies();
            setupPage.AddRemoveAssembliesOnly = true;

            await p_EmptyProject.SetupPathAndAssemblies(setupPage);
        }
    }
}
