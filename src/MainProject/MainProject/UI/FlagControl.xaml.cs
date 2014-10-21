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

namespace MainProject.UI
{
    /// <summary>
    /// Interaction logic for FlagControl.xaml
    /// </summary>
    public partial class FlagControl : UserControl, INotifyPropertyChanged
    {
        public static DependencyProperty CultureCodeProperty = DependencyProperty.Register("CultureCode", typeof(string), typeof(FlagControl), new PropertyMetadata("unk", new PropertyChangedCallback(ValueChanged)));
        public static DependencyProperty IconSizeProperty = DependencyProperty.Register("IconSize", typeof(string), typeof(FlagControl), new PropertyMetadata("", new PropertyChangedCallback(ValueChanged)));

        public FlagControl()
        {
            InitializeComponent();
        }

        private static void ValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs value)
        {
            if (sender is FlagControl)
            {
                FlagControl flagControl = (FlagControl)sender;
                flagControl.DoPropertyChanged("ImageFileName");
            }
        }


        public string CultureCode
        {
            get
            {
                return (string)GetValue(CultureCodeProperty);
            }
            set
            {
                SetValue(CultureCodeProperty, value);
            }
        }

        public string IconSize
        {
            get
            {
                return (string)GetValue(IconSizeProperty);
            }
            set
            {
                SetValue(IconSizeProperty, value);
            }
        }

        public string ImageFileName
        {
            get
            {
                CultureToFlagConverter converter = new CultureToFlagConverter();
                var stringValue = (string)converter.Convert(CultureCode, typeof(string), IconSize, System.Threading.Thread.CurrentThread.CurrentUICulture);
                return stringValue;
            }
        }
    
        public event PropertyChangedEventHandler PropertyChanged;

        public void DoPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
