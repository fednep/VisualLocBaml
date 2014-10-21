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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MainProject
{
    /// <summary>
    /// Interaction logic for HeaderControl.xaml
    /// </summary>
    public partial class HeaderControl : UserControl
    {
        public static DependencyProperty IsMenuEnabledProperty = DependencyProperty.Register("IsMenuEnabled",
                                                                                        typeof(bool), typeof(HeaderControl), new PropertyMetadata(true));
        public HeaderControl()
        {
            InitializeComponent();

            MouseDoubleClick += HeaderControl_MouseDoubleClick;

            if (App.MainWindow != null)
                DataContext = App.MainWindow.Document;
        }

        public bool IsMenuEnabled
        {
            get
            {
                return (bool)GetValue(IsMenuEnabledProperty);
            }

            set
            {
                SetValue(IsMenuEnabledProperty, value);
            }
        }

        void HeaderControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ((MainWindow)App.Current.MainWindow).ToggleMaximize();
            e.Handled = true;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                App.Current.MainWindow.DragMove();
        }

        private void MetroButton_Click(object sender, RoutedEventArgs e)
        {
            ProjectOptions.PlacementTarget = ProjectNameButton;
            ProjectOptions.IsOpen = true;
        }
    }
}
