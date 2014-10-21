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
using System.Windows.Input;

namespace MainProject
{
    public class Commands
    {
        public static RoutedCommand New =    new RoutedCommand();
        public static RoutedCommand Open =   new RoutedCommand();

        public static RoutedCommand RemoveRecent = new RoutedCommand();
        public static RoutedCommand OpenMore = new RoutedCommand();

        public static RoutedCommand Save =   new RoutedCommand();
        public static RoutedCommand SaveAs = new RoutedCommand();
        public static RoutedCommand Close =  new RoutedCommand();

        public static RoutedCommand Exit = new RoutedCommand();

        public static RoutedCommand Settings = new RoutedCommand();

        public static RoutedCommand AddAssembly = new RoutedCommand();

        public static RoutedCommand ViewAssemblyStrings = new RoutedCommand();
        public static RoutedCommand RemoveAssembly = new RoutedCommand();
        public static RoutedCommand UpdateStrings = new RoutedCommand();

        public static RoutedCommand OpenBinariesPath = new RoutedCommand();
        public static RoutedCommand GoBack = new RoutedCommand();

        public static RoutedCommand RetrieveStrings = new RoutedCommand();
        public static RoutedCommand GenerateAssemblies = new RoutedCommand();

        public static RoutedCommand RemoveLanguage = new RoutedCommand();
    }
}
