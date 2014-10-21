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

namespace MainProject
{
    public class Culture
    {
        private string __CultureCode;

        private string __LanguageCode;
        private string __Language;

        private string __CountryCode;
        private string __Country;
        private string __FlagCountryCode;

        public Culture(string cultureCode)
        {
            __CultureCode = cultureCode;

            var splittedCulture = __CultureCode.Split(new char[] { '-' });
            if (splittedCulture.Length > 3)
                throw new ArgumentException("Culture code is in the incorrect format. Correct format example is en-US");

            if (splittedCulture.Length == 1)
                __LanguageCode = __CultureCode;
            if (splittedCulture.Length == 2)
            {
                __LanguageCode = splittedCulture.First();
                __CountryCode = splittedCulture.Last();
            }
            else if (splittedCulture.Length == 3)
            {
                __LanguageCode = splittedCulture[1];
                __CountryCode = splittedCulture.Last();
            }
            
            var LanguageCountry = CultureParser.Instance.GetByCultureCode(cultureCode);
            __Language = LanguageCountry.Language;
            __Country = LanguageCountry.Country;
            __FlagCountryCode = LanguageCountry.FlagCountryCode;
        }

        public static bool IsValid(string cultureCode)
        {
            var splittedCulture = cultureCode.Split(new char[] { '-' });
            if (splittedCulture.Length > 3)
                return false;

            return true;
        }
        public string CultureCode
        {
            get
            {
                return __CultureCode;
            }
        }

        public string Language
        {
            get
            {
                return __Language;
            }
        }

        public string FlagCountryCode
        {
            get
            {
                return __FlagCountryCode;
            }
        }

        public string LanguageCode
        {
            get
            {
                return __LanguageCode;
            }
        }

        public string Description
        {
            get
            {
                if (String.IsNullOrEmpty(__Country))
                    return String.Format(StringUtils.String("CultureNeutral_FullDescription", __Language, __CultureCode));

                return String.Format(StringUtils.String("CultureFullDescription", __Language, __Country, __CultureCode));
            }
        }

        public string Country
        {
            get
            {
                return __Country;
            }
        }

        public string CountryCode
        {
            get
            {
                return __CountryCode;
            }
        }

        internal bool CompareTo(Culture culture)
        {
            if (CultureCode.ToLower() == culture.CultureCode.ToLower())
                return true;

            return false;
        }
    }
}
