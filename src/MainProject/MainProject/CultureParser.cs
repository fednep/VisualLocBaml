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
    class CultureParser
    {
        private static CultureParser __Instance;

        private Dictionary<string, LanguageCountry> __LanguageCountriesDict;

        public static CultureParser Instance
        {
            get
            {
                if (__Instance == null)
                    __Instance = new CultureParser();

                return __Instance;
            }
        }

        public CultureParser()
        {
            __LanguageCountriesDict = new Dictionary<string, LanguageCountry>();

            string cultures = res.Resources.cultures;

            foreach (var line in cultures.Split(new string[] { "\r\n" }, StringSplitOptions.None))
            {
                if (line.Trim() == "")
                    continue;

                var cultureData  = line.Split(new char[] { ',' });

                if (cultureData.Length != 4)
                    throw new ArgumentException("Internal error: invalid culture dictionary in the resource.");

                __LanguageCountriesDict.Add(cultureData[0].ToLower(), new LanguageCountry(cultureData[1], cultureData[2], cultureData[3]));
            }
        }

        public LanguageCountry GetByCultureCode(string cultureCode)
        {
            cultureCode = cultureCode.ToLower();
            if (!__LanguageCountriesDict.ContainsKey(cultureCode))
                return new LanguageCountry(StringUtils.String("UnknownLanguage"), StringUtils.String("UnknownCountry"), "");
            
            return __LanguageCountriesDict[cultureCode];
        }

        public List<Culture> GetCultureList()
        {
            List<Culture> res = new List<Culture>();
            foreach (var culture in __LanguageCountriesDict.Keys)
                res.Add(new Culture(culture));
            
            res.Sort((culture1, culture2) => culture1.Description.CompareTo(culture2.Description));
            return res;
        }
    }
}
