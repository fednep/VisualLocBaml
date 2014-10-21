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

namespace MainProject.UI.DocumentPages
{
    public class FileToImport
    {
        private string __FileName;
        private Culture __Culture;

        public FileToImport(string fileName, Culture culture)
        {
            __FileName = fileName;
            __Culture = culture;
        }

        public static FileToImport FromFile(string fileName)
        {
            var document = App.MainWindow.Document;
            var file = System.IO.Path.GetFileName(fileName).ToLower();

            foreach (var culture in document.Cultures)
            {
                if (file == document.GetTranslationFileNameForCulture(culture.CultureCode).ToLower())
                {
                    return new FileToImport(fileName, new Culture(culture.CultureCode));
                }
            }

            return null;
        }

        public string FileName
        {
            get
            {
                return __FileName;
            }
        }

        public string RelativeFileName
        {
            get
            {
                return System.IO.Path.GetFileName(__FileName);
            }
        }

        public Culture Culture
        {
            get
            {
                return __Culture;
            }
        }
    }
}
