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

using TranslationApi.Baml;
using MainProject.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MainProject.UI.DocumentPages;
using TranslationApi;
using System.ComponentModel;

namespace MainProject.DocumentPages
{

    /// <summary>
    /// Interaction logic for home.xaml
    /// </summary>
    public partial class pp_ImportTranslationResult : Page, INotifyPropertyChanged
    {
        private Document __Document;
        private List<ImportTranslationResult> __ImportResults;

        public pp_ImportTranslationResult()
        {
            InitializeComponent();

            __Document = App.MainWindow.Document;
            DataContext = this;
        }

        public List<ImportTranslationResult> ImportResults
        {
            get
            {
                return __ImportResults;
            }

            set
            {
                __ImportResults = value;
                DoPropertyChanged("ImportResults");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void DoPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OpenIssues(object sender, RoutedEventArgs e)
        {
            MetroButton senderButton = (MetroButton)sender;
            ImportTranslationResult importTranslationResult = (ImportTranslationResult)senderButton.Tag;

            string log = CreateLogFromResult(importTranslationResult.Log);
            LogViewer viewer = new LogViewer();
            viewer.Title = StringUtils.String("ImportTranslationLogIssue", importTranslationResult.RelativeFileName);
            viewer.Log = log;
            viewer.Owner = App.MainWindow;
            viewer.Show();
        }

        private string CreateLogFromResult(List<ImportTranslationResultLogLine> list)
        {
            string result = "";
            foreach (var issue in list)
            {
                result += StringUtils.String("ImportTranslationLogEntry", GetErrorReasonString(issue.ErrorReason), 
                    issue.Sid, 
                    issue.OriginalString, 
                    issue.OriginalInFile,
                    issue.TranslatedString);
            }
            return result;
        }

        private object GetErrorReasonString(ImportTranslationStringErrorReason importTranslationStringErrorReason)
        {
            switch(importTranslationStringErrorReason)
            {
                case ImportTranslationStringErrorReason.OriginalStringChanged:
                    return StringUtils.String("ImportTranslationLogOriginalStringChanged");
                case ImportTranslationStringErrorReason.StringNotFound:
                    return StringUtils.String("ImportTranslationLogStringNotFound");
                case ImportTranslationStringErrorReason.TranslationIsTheSame:
                    return StringUtils.String("ImportTranslationTranslationIsTheSame");
            }

            return "";
        }
    }
}
