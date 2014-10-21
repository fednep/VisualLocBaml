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
    /// Interaction logic for pp_GenerateAssembliesResult.xaml
    /// </summary>
    public partial class pp_GenerateAssembliesResult : Page, INotifyPropertyChanged
    {
        private List<AssemblyGenerationResult> __Results;

        public pp_GenerateAssembliesResult()
        {            
            InitializeComponent();
            DataContext = this;
        }

        public List<AssemblyGenerationResult> Results
        {
            get
            {
                return __Results;
            }

            set
            {
                __Results = value;
                DoPropertyChanged("Results");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void DoPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;

            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OpenIssues(object sender, RoutedEventArgs e)
        {
            MetroButton senderButton = (MetroButton)sender;
            AssemblyGenerationResult assemblyGenerationResult = (AssemblyGenerationResult)senderButton.Tag;
            
            LogViewer viewer = new LogViewer();            
            viewer.Log = assemblyGenerationResult.Log.LogData;
            viewer.Owner = App.MainWindow;
            viewer.Show();            
        }
    }
}
