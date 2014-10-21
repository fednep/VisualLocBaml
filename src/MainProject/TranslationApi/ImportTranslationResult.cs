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
using TranslationApi.KBCsv;

namespace TranslationApi
{

    public class ImportTranslationResult
    {
        private List<ImportTranslationResultLogLine> __Log;

        private string __FileName;
        private string __CultureCode;
        private string __ErrorMassage;

        private List<int> __AllSids;
        private List<int> __SidsWithErrors;
        private List<int> __SidsUpdated;

        public ImportTranslationResult(string fileName, string cultureCode)
        {
            __Log = new List<ImportTranslationResultLogLine>();

            __FileName = fileName;
            __CultureCode = cultureCode;

            __AllSids = new List<int>();
            __SidsWithErrors = new List<int>();
            __SidsUpdated = new List<int>();
        }

        public string ErrorMessage
        {
            get
            {
                return __ErrorMassage;
            }

            set
            {
                __ErrorMassage = value;
            }
        }

        public List<int> AllSids
        {
            get
            {
                return __AllSids;
            }

            set
            {
                __AllSids = value;
            }
        }        

        public List<int> SidsUpdated
        {
            get
            {
                return __SidsUpdated;
            }

            set
            {
                __SidsUpdated = value;
            }
        }


        public List<int> SidsWithErrors
        {
            get
            {
                return __SidsWithErrors;
            }

            set
            {
                __SidsWithErrors = value;
            }
        }

        public ICollection<DataRecord> Records
        {
            get;
            set;
        }

        public List<ImportTranslationResultLogLine> Log
        {
            get
            {
                return __Log;
            }
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

        public string CultureCode
        {
            get
            {
                return __CultureCode;
            }
        }        
    }
}
