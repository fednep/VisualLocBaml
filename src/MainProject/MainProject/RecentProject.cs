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
using System.Text;

namespace MainProject
{
    public class RecentProject
    {
        private string __ProjectFile;
        private DateTime __LastOpened;        

        public string ProjectFile
        {
            get
            {
                return __ProjectFile;
            }

            set
            {
                __ProjectFile = value;
            }
        }

        public DateTime LastOpened
        {
            get
            {
                return __LastOpened;
            }

            set
            {
                __LastOpened = value;
            }
        }

        public bool IsAvailable
        {
            get
            {
                return File.Exists(__ProjectFile);
            }
        }
    }
}
