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
using MainProject.UI.DocumentPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TranslationApi;

namespace MainProject
{
    public class StringsRetriever
    {
        public Document __Document;

        public StringsRetriever(Document document)
        {
            __Document = document;
        }

        public static async Task<LocalizationAppDomain> CreateAppDomain(string pathToBinaries)
        {
            return await Task.Run<LocalizationAppDomain>(() =>
            {
                return new LocalizationAppDomain(pathToBinaries);
            });
        }

        public static async Task DisposeAppDomain(LocalizationAppDomain appDomain)
        {
            await Task.Run(() =>
            {
                appDomain.Dispose();
            });
        }

        private static Task<T> StartSTATask<T>(Func<T> func)
        {
            var tcs = new TaskCompletionSource<T>();
            Thread thread = new Thread(() =>
            {
                try
                {
                    tcs.SetResult(func());
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        public async Task<List<BamlString>> ExtractTranslationLines(LoadedAssembly loadedAssembly)
        {
            return await StartSTATask<List<BamlString>>(() =>
            {
                return loadedAssembly.ExtractTranslationLines();
            });

            /*return await Task.Run<List<BamlString>>(() =>
            {
                return loadedAssembly.ExtractTranslationLines();
            });*/
        }        

        public async Task<object> RetrieveAllAssemblies(IProgress<string> progress, CancellationToken cancelToken)
        {
            LocalizationAppDomain appDomain = await CreateAppDomain(__Document.PathToBinaries);
            List<ImportResult> results = new List<ImportResult>();

            foreach (var assembly in __Document.Assemblies)
            {
                progress.Report(StringUtils.String("LoadingAssembly_0", assembly.Assembly.AssemblyFile));

                LoadedAssembly loadedAssembly = await Task.Run<LoadedAssembly>(() =>
                {
                    return appDomain.LoadAssembly(
                    System.IO.Path.Combine(__Document.PathToBinaries, assembly.Assembly.DefaultResourceFile));
                });

                var progressString = StringUtils.String("ImportingFromAssembly_0", assembly.Assembly.AssemblyFile);
                var progressReporter = new ProgressPercentageReporter(progress, progressString);

                var lines = await ExtractTranslationLines(loadedAssembly);
                lines = lines.Where(LinesFilter).ToList();

                ImportResult importResult = await __Document.ImportApi.ImportStrings(
                    progressReporter,
                    cancelToken,
                    assembly.Assembly.AssemblyFile, lines);

                results.Add(importResult);
            }

            progress.Report(StringUtils.String("Import_Finishing"));
            await DisposeAppDomain(appDomain);

            __Document.UpdateTranslatedCultures();
            return results;
        }

        public bool LinesFilter(BamlString line)
        {
            return line.Category != LocalizationCategory.None ||
                   line.ResourceKey.ToLower().Contains("system.string");
        }        
    }
}
