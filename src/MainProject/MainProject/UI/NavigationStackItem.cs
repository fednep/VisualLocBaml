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

using MainProject.UI.DocumentPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace MainProject.UI
{
    class NavigationStackItem
    {
        private Frame __Frame;
        private object __Content;
        private NavigationAnimation __Animation;

        public NavigationStackItem(Frame frame, object content, NavigationAnimation animation)
        {
            __Frame = frame;
            __Content = content;
            __Animation = animation;
        }

        public Frame Frame
        {
            get
            {
                return __Frame;
            }
        }

        public object Content
        {
            get
            {
                return __Content;
            }
        }

        public NavigationAnimation Animation
        {
            get
            {
                return __Animation;
            }
        }
    }
}
