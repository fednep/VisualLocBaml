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

namespace MainProject
{  
    public class RoundButton : Button
    {
        public static DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(FrameworkElement), typeof(RoundButton));
        public static DependencyProperty EllipseBackgroundProperty = DependencyProperty.Register("EllipseBackground", typeof(Brush), typeof(RoundButton));
            

        static RoundButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RoundButton), new FrameworkPropertyMetadata(typeof(RoundButton)));
        }

        public Brush EllipseBackground
        {
            get
            {
                return (Brush)GetValue(EllipseBackgroundProperty);
            }
            set
            {
                SetValue(EllipseBackgroundProperty, value);
            }
        }

        public FrameworkElement Icon
        {
            get
            {
                return (FrameworkElement)GetValue(IconProperty);
            }

            set
            {
                SetValue(IconProperty, value);
            }
        }        


    }
}
