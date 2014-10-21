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
using System.Linq;
using System.Text;

namespace MainProject
{
    public class RecentProjects : ObservableCollection<RecentProject>
    {
        const string REGISTRY_HOME_RECENTS = "Software\\WPFLocalizer\\Recent";

        RegistryKey __RegistryKey = Registry.CurrentUser;

        private static RecentProjects __Instance;

        public static RecentProjects Instance
        {
            get
            {
                if (__Instance == null)
                    __Instance = new RecentProjects();

                return __Instance;
            }
        }

        private RecentProjects() : base()
        {
            
        }

        public void LoadFromRegistry()
        {            
            List<RecentProject> recentItems = new List<RecentProject>();
            using (RegistryKey recentFile = __RegistryKey.OpenSubKey(REGISTRY_HOME_RECENTS, false))
            {
                if (recentFile == null)
                    return;

                string[] files = recentFile.GetValueNames();


                foreach (var file in files)
                {
                    string lastOpenedString = recentFile.GetValue(file, "") as string;
                    DateTime lastOpened = DateTime.Now;

                    if (lastOpenedString != null && !string.IsNullOrWhiteSpace(lastOpenedString))
                    {
                        try
                        {
                            lastOpened = new DateTime(Int64.Parse(lastOpenedString));
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    RecentProject project = new RecentProject();
                    project.ProjectFile = file;
                    project.LastOpened = lastOpened;
                    recentItems.Add(project);
                }

            }

            recentItems.Sort(
                (RecentProject item1, RecentProject item2) =>
                    item2.LastOpened.CompareTo(item1.LastOpened)
                    );

            Clear();

            for (int i = 0; i < recentItems.Count; i++)
                Add(recentItems[i]);
        }

        public void UpdateUsed(string fileName)
        {
            foreach (var project in this)
            {
                if (project.ProjectFile == fileName)
                {
                    project.LastOpened = DateTime.Now;
                    SaveRecentToRegistry(project);
                    return;
                }
            }

            RecentProject newProject = new RecentProject();
            newProject.ProjectFile = fileName;
            newProject.LastOpened = DateTime.Now;
            SaveRecentToRegistry(newProject);
        }

        private void SaveRecentToRegistry(RecentProject project)
        {
            using (var registryKey = __RegistryKey.CreateSubKey(REGISTRY_HOME_RECENTS, RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                if (registryKey == null)
                    return;

                registryKey.SetValue(project.ProjectFile, project.LastOpened.Ticks.ToString(), RegistryValueKind.String);
            }

            LoadFromRegistry();
        }

        public void Remove(string fileName)
        {
            
            RecentProject project = this.FirstOrDefault(proj => proj.ProjectFile == fileName);

            if (project == null)
                return;

            base.Remove(project);

        }

        protected override void RemoveItem(int index)
        {
            var obj = this[index];
            base.RemoveItem(index);

            using (var registryKey = __RegistryKey.CreateSubKey(REGISTRY_HOME_RECENTS, RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                if (registryKey == null)
                    return;

                registryKey.DeleteValue(obj.ProjectFile);
            }
        }

    }
}
