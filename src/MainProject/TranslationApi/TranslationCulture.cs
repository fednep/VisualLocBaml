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
using TranslationApi;

namespace TranslationApi
{
    public class TranslationCulture
    {
        private string __CultureCode;
        private int __StringsTranslated;

        public TranslationCulture(string cultureCode, int stringsTranslated)
        {
            __CultureCode = cultureCode;
            __StringsTranslated = stringsTranslated;
        }

        public string CultureCode
        {
            get
            {
                return __CultureCode;
            }
        }

        public int StringsTranslated
        {
            get
            {
                return __StringsTranslated;
            }
        }
    }
}
