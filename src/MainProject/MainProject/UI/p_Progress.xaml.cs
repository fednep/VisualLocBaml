
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
using System.Threading;
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
    /// Interaction logic for p_Progress.xaml
    /// </summary>
    public partial class p_Progress : Page, IProgress<string>, INotifyPropertyChanged
    {
        private CancellationTokenSource __CancellationToken;
        private bool __Cancellable;

        public p_Progress()
        {
            InitializeComponent();
            __CancellationToken = new CancellationTokenSource();
            __Cancellable = false;

            DataContext = this;
        }

        public CancellationToken CancellationToken
        {
            get
            {
                return __CancellationToken.Token;
            }
        }

        public void Report(string value)
        {
            ProgressTitle.Text = value;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            __CancellationToken.Cancel();
        }

        public bool Cancellable
        {
            get
            {
                return __Cancellable;
            }

            set
            {
                __Cancellable = value;
                DoPropretyChanged("Cancellable");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void DoPropretyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
