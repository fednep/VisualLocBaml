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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using TranslationApi;

namespace MainProject
{
    public class AvailableAssemblyForTranslation: INotifyPropertyChanged
    {
        private LocalizableAssembly __Assembly;
        private bool? __IsSelected;
        private bool __IsMissing;
        private AvailableAssembliesCollection __ParentCollection;

        public AvailableAssemblyForTranslation(AvailableAssembliesCollection parentCollection, LocalizableAssembly assembly)
        {
            __Assembly = assembly;            
            __ParentCollection = parentCollection;
            __IsSelected = false;
            __IsMissing = false;
        }

        public LocalizableAssembly Assembly
        {
            get
            {
                return __Assembly;
            }
        }

        public bool? IsSelected
        {
            get
            {
                return __IsSelected;
            }

            set
            {
                __IsSelected = value;
                __ParentCollection.RecalculateSelected();
                DoPropertyChanged("IsSelected");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void DoPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler == null)
                return;

            handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsMissing 
        {
            get
            {
                return __IsMissing;
            }

            set
            {
                __IsMissing = value;
            }
        }
    }
}
