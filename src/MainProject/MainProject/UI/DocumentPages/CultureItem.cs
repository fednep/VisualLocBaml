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
using TranslationApi;

namespace MainProject.UI.DocumentPages
{
    public class CultureItem: INotifyPropertyChanged
    {
        private TranslationCulture __DocumentCulture;
        private Culture __Culture;
        private int __TranslatedRate;        
        private string __FileName;

        private bool __IsSelected;

        public CultureItem(TranslationCulture documentCulture, string fileName, int percentsTranslated)
        {
            __DocumentCulture = documentCulture;

            __Culture = new Culture(__DocumentCulture.CultureCode);
            __TranslatedRate = percentsTranslated;
            __FileName = fileName;
        }

        public string Country
        {
            get
            {
                return __Culture.Country;
            }
        }

        public string Language
        {
            get
            {
                return __Culture.Language;
            }
        }

        public string CultureDescription
        {
            get
            {
                return __Culture.Description;
            }
        }

        public string CountryCode
        {
            get
            {
                return __Culture.CountryCode;
            }
        }

        public string FlagCountryCode
        {
            get
            {
                return __Culture.FlagCountryCode;
            }
        }

        public int TranslatedRate
        {
            get
            {
                return __TranslatedRate;
            }
        }

        public string PercentsTranslated
        {
            get
            {
                return String.Format("{0:N0}%", __TranslatedRate);
            }
        }        

        public bool IsSelected
        {
            get
            {
                return __IsSelected;
            }

            set
            {
                __IsSelected = value;
                DoPropertyChanged("IsSelected");
            }
        }

        public Culture Culture
        {
            get
            {
                return __Culture;
            }
        }

        public string RelativeFileName
        {
            get
            {
                return System.IO.Path.GetFileName(__FileName);
            }
        }

        public string FileName
        {
            get
            {
                return __FileName;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void DoPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public TranslationCulture DocumentCulture
        {
            get
            {
                return __DocumentCulture;
            }
        }

    }
}
