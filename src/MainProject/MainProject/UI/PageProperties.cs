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

namespace MainProject.UI
{
    class PageProperties : DependencyObject
    {
        public static DependencyProperty PageTagProperty = DependencyProperty.RegisterAttached("PageTag", typeof(string), typeof(PageProperties), new PropertyMetadata(""));
        public static DependencyProperty EnableMenuItemProperty = DependencyProperty.RegisterAttached("EnableMenuItem", typeof(bool), typeof(PageProperties), new PropertyMetadata(true));

        public static string GetPageTag(DependencyObject obj)
        {
            return (string)obj.GetValue(PageTagProperty);
        }

        public static void SetPageTag(DependencyObject obj, string value)
        {
            obj.SetValue(PageTagProperty, value);
        }

        public static bool GetEnableMenuItem(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableMenuItemProperty);
        }

        public static void SetEnableMenuItem(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableMenuItemProperty, value);
        }
    }
}
