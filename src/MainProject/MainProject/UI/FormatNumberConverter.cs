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
using System.Globalization;

namespace MainProject.UI
{
    public class FormatNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is long)
                return ((long)value).ToString("N0");
            if (value is ulong)
                return ((ulong)value).ToString("N0");
            if (value is int)
                return (((int)value).ToString("N0"));
            if (value is short)
                return (((short)value).ToString("N0"));
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType == typeof(int))
            {
                int result;
                int.TryParse(value.ToString(), NumberStyles.Number, null, out result);
                return result;
            }
            else if (targetType == typeof(long))
            {
                long result;
                long.TryParse(value.ToString(), NumberStyles.Number, null, out result);
                return result;
            }
            else if (targetType == typeof(ulong))
            {
                ulong result;
                ulong.TryParse(value.ToString(), NumberStyles.Number, null, out result);
                return result;
            }
            else if (targetType == typeof(short))
            {
                short result;
                short.TryParse(value.ToString(), NumberStyles.Number, null, out result);
                return result;
            }
            else
                throw new NotImplementedException();
        }
    }

}
