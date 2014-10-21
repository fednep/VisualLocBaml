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

using MainProject.res;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MainProject.UI
{
    class CultureToTextConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {            
            string cultureCode = (string)value;
            if (cultureCode == "")
                return "";

            Culture cul = null;
            if (value.GetType() == typeof(string))           
                cul = new Culture(cultureCode);

            if (value.GetType() == typeof(Culture))
                cul = (Culture)value;

            if (cul == null)
                throw new InvalidOperationException(String.Format("Value is of an unexpected type {0}", value.GetType()));

             if (parameter is string)
             {
                 switch (((string)parameter).ToLower())
                 {
                     case "countrycode":
                         return cul.CountryCode;

                     case "country": 
                         return cul.Country;

                     case "language": 
                         return cul.Language;
                 }
             }

             if (String.IsNullOrEmpty(cul.Country))
                 return String.Format(StringUtils.String("CultureNeutral_FullDescription", cul.Language, cul.CultureCode));        

             return String.Format(StringUtils.String("CultureFullDescription", cul.Language, cul.Country, cul.CultureCode));        
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
