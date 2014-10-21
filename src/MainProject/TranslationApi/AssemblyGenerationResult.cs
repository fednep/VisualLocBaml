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

namespace TranslationApi
{
    public class AssemblyGenerationResult
    {
        private string __FileName;
        private string __CultureCode;
        private string __ResultMessage;
        private bool __IsError;
        private TextFileLog __Log;

        public AssemblyGenerationResult(string fileName, string cultureCode)
        {
            __FileName = System.IO.Path.Combine(cultureCode, fileName);
            __CultureCode = cultureCode;

            __IsError = false;
            __ResultMessage = "";
            __Log = new TextFileLog();
        }

        public string FileName
        {
            get
            {
                return __FileName;
            }
        }

        public string CultureCode
        {
            get
            {
                return __CultureCode;
            }
        }

        public string RelativeFileName
        {
            get
            {
                return System.IO.Path.GetFileName(__FileName);
            }
        }

        public bool IsError
        {
            get
            {
                return __IsError;
            }

            set
            {
                __IsError = value;
            }
        }

        public string ResultMessage
        {
            get
            {
                return __ResultMessage;
            }

            set
            {
                __ResultMessage = value;
            }
        }

        public TextFileLog Log 
        { 
            get
            {
                return __Log;
            }
        }        
    }
}
