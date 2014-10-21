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

    public enum ImportTranslationStringErrorReason { StringNotFound, OriginalStringChanged, TranslationIsTheSame }

    public class ImportTranslationResultLogLine
    {
        private int __Sid;
        private string __OriginalString;
        private string __OriginalInFileString;
        private string __TranslatedString;

        private ImportTranslationStringErrorReason __ErrorReason;

        public ImportTranslationResultLogLine(int sid, string originalString, string originalInFileString, string translatedString, ImportTranslationStringErrorReason errorReason)
        {
            __Sid = sid;
            __OriginalString = originalString;
            __OriginalInFileString = originalInFileString;
            __TranslatedString = translatedString;

            __ErrorReason = errorReason;
        }

        public int Sid
        {
            get
            {
                return __Sid;
            }
        }

        public string OriginalString
        {
            get
            {
                return __OriginalString;
            }
        }

        public string TranslatedString
        {
            get
            {
                return __TranslatedString;
            }
        }

        public ImportTranslationStringErrorReason ErrorReason
        {
            get
            {
                return __ErrorReason;
            }
        }


        public string OriginalInFile 
        {
            get
            {
                return __OriginalInFileString;
            }
        }
    }
}
