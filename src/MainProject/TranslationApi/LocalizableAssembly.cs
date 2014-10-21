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
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace TranslationApi
{
    public class LocalizableAssembly
    {
        private string __AssemblyFile;
        private string __DefaultResourceFile;
        private string __DefaultCulture;
        
        public void LocalizableAssebly()
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assemblyFile">Относительный путь к файлу сборки, например SomeApp.exe</param>        
        /// <param name="cultureString">Дефолтноя культура сборки, например en-US</param>
        /// <param name="defaultResourceFile">Относительный путь к дефолным ресурсам для этой сборки</param>
        public LocalizableAssembly(string assemblyFile, string defaultCulture, string defaultResourceFile)
        {
            __AssemblyFile = assemblyFile.ToLower();
            __DefaultCulture = defaultCulture.ToLower();
            __DefaultResourceFile = defaultResourceFile.ToLower();
        }



        public string AssemblyFile
        {
            get
            {
                return __AssemblyFile;
            }
        }        

        public string DefaultResourceFile
        {
            get
            {
                return __DefaultResourceFile;
            }
        }

        public string DefaultCulture
        {
            get
            {
                return __DefaultCulture;
            }
        }
    }
}
