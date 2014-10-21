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
using System.Windows.Data;

namespace MainProject.UI
{
    class CultureToFlagConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string size_dir = "24";

            string cultureCode = (string)value;

            var languageCulture = CultureParser.Instance.GetByCultureCode(cultureCode);

            if (parameter is string)
                if ((string)parameter == "small")
                    size_dir = "16";

            var imageResourceUri = String.Format("pack://application:,,,/res/flags/{0}/{1}.png", size_dir, (languageCulture.FlagCountryCode).ToLower());
            try
            {
                var resource = App.GetResourceStream(new Uri(imageResourceUri));
                return imageResourceUri;
            }
            catch
            {
                return String.Format("pack://application:,,,/res/flags/{0}/unk.png", size_dir);
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
