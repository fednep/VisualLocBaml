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

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MainProject
{
    /// <summary>
    /// Interaction logic for p_StartPage.xaml
    /// </summary>
    public partial class p_StartPage : Page, INotifyPropertyChanged
    {
        private static p_StartPage __Instance;

        public p_StartPage()
        {
            InitializeComponent();
            __Instance = this;
            DataContext = this;

            Loaded += p_StartPage_Loaded;
            
            CommandBindings.Add(new CommandBinding(Commands.Open, new ExecutedRoutedEventHandler(Open_Cmd),
                                                                    new CanExecuteRoutedEventHandler(Open_CanCmd)));

            CommandBindings.Add(new CommandBinding(Commands.OpenMore, new ExecutedRoutedEventHandler(OpenMore)));
            CommandBindings.Add(new CommandBinding(Commands.RemoveRecent, new ExecutedRoutedEventHandler(RemoveRecent)));            

            
            
        }



        public async void Open_Cmd(object sender, ExecutedRoutedEventArgs e)
        {

            string projectFileName = null;

            if (e.Parameter is RecentProject)
            {
                RecentProject project = e.Parameter as RecentProject;

                if (project.IsAvailable == false)
                {
                    if (await App.MainWindow.ShowMessage(
                            StringUtils.String("RecentProjectNotFound"), MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        RecentProjects.Instance.Remove(project);
                        DoPropertyChanged("Recents");
                    }

                    return;
                }

                projectFileName = project.ProjectFile;
            }

            if (projectFileName == null)
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = StringUtils.String("OpenSave_Filter");
                if (dialog.ShowDialog() == true)
                    projectFileName = dialog.FileName;
            }


            if (!String.IsNullOrWhiteSpace(projectFileName))
            {
                Document newDocument = null;

                MainWindow.DoAsync(() => newDocument = new Document(projectFileName));
                if (newDocument == null)
                    return;

                ((MainWindow)Application.Current.MainWindow).Document = newDocument;
            }
        }

        private void Open_CanCmd(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter == null)
            {
                e.CanExecute = true;
                return;
            }

            RecentProject project = e.Parameter as RecentProject;
            if (project == null)
            {
                e.CanExecute = false;
                return;
            }

            e.CanExecute = true;
        }


        private void RemoveRecent(object sender, ExecutedRoutedEventArgs e)
        {
            RecentProject recent = e.Parameter as RecentProject;
            if (recent == null)
                return;

            RecentProjects.Instance.Remove(recent);
            DoPropertyChanged("Recents");
        }        

        void p_StartPage_Loaded(object sender, RoutedEventArgs e)
        {
            RecentProjects.Instance.LoadFromRegistry();
            DoPropertyChanged("Recents");
            
        }

        public static Page Instance 
        {
            get
            {
                if (__Instance == null)
                    __Instance = new p_StartPage();
                
                return __Instance;
            }
        }

        public List<object> Recents
        {
            get
            {
                List<object> projectsToShow = new List<object>();
                for (int i = 0; i < RecentProjects.Instance.Count; i++)
                {
                    if (i < 3)
                    {
                        projectsToShow.Add(RecentProjects.Instance[i]);
                    } else 
                    {
                        List<RecentProject> leftoverProjects = new List<RecentProject>();
                        for (int k = i; k < RecentProjects.Instance.Count; k++)
                            leftoverProjects.Add(RecentProjects.Instance[k]);

                        projectsToShow.Add(new RecentProjectMore(leftoverProjects));
                        return projectsToShow;
                    }                    
                }

                return projectsToShow;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void DoPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OpenMore(object sender, ExecutedRoutedEventArgs e)
        {

            RecentProjectMore recent = e.Parameter as RecentProjectMore;
            if (recent == null)
                return;

            MorePopup.PlacementTarget = (UIElement)sender;
            MorePopup.IsOpen = true;
            MorePopup.ItemsSource = recent.Projects;
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                ((MainWindow)App.Current.MainWindow).ToggleMaximize();
        }
    }
}
