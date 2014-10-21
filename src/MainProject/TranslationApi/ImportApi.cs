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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TranslationApi.KBCsv;

namespace TranslationApi
{
    public class ImportApi
    {
        private Database __DB;

        public ImportApi(Database db)
        {
            __DB = db;
        }

        ///
        private List<int> FindDeletedStrings(string dllFile, List<int> usedLines)
        {
            List<StringRow> allSids = __DB.SelectAllSids(dllFile);
            List<int> sidsToDelete = new List<int>();

            foreach (StringRow row in allSids)
            {
                if (__DB.IsDeleted(row.Sid))
                {
                    if (!usedLines.Any(t => t == row.Sid))
                    {
                        sidsToDelete.Add(row.Sid);
                    }
                }
            }

            return sidsToDelete;
        }

        public async Task<ImportResult> ImportStrings(IProgress<int> progress, CancellationToken cancellationToken, string dllFile, List<BamlString> lines)
        {
            List<int> newStrings = new List<int>();
            List<int> sidsList = new List<int>();
            List<int> stringsUpadted = new List<int>();
            List<int> sidsToDelete = new List<int>();

            __DB.BeginTransaction();
            try
            {
                int line_no = 0, max = lines.Count;
                var enumerator = lines.GetEnumerator();

                bool finished = !enumerator.MoveNext();
                while (!finished)
                {
                    await Task.Run(() =>
                    {
                        for (int i = 0; i < 100; i++)
                        {
                            BamlString line = enumerator.Current;

                            int? sid = __DB.StringId(dllFile, line.BamlFile, line.ResourceKey);

                            if (!string.IsNullOrEmpty(line.Content))
                            {                                
                                if (sid == null)
                                {
                                    sid = __DB.NewStringId(dllFile, line);
                                    newStrings.Add((int)sid);
                                }

                                StringRow strRow = __DB.StringForId((int)sid);
                            
                                if (strRow == null)
                                    __DB.StringAddRevision((int)sid, 1, line.Content);
                                else if (strRow.Content != line.Content)
                                {
                                    __DB.StringAddRevision((int)sid, strRow.Revision + 1, line.Content);
                                    stringsUpadted.Add((int)sid);
                                }
                                sidsList.Add((int)sid);
                            }
                            else
                            {
                                if (sid != null)
                                {
                                    StringRow strRow = __DB.StringForId((int)sid);
                                    // Если строка стала пустой - пометить ее как удаленную.
                                    if (strRow != null)
                                    {
                                        __DB.Delete((int)sid);
                                    }
                                }
                            }

                            line_no += 1;
                            if (!enumerator.MoveNext())
                            {
                                finished = true;
                                break;
                            }
                        }
                    });

                    progress.Report(line_no * 100 / max);                    
                }

                await Task.Run(() =>
                {

                    sidsToDelete = FindDeletedStrings(dllFile, sidsList);
                    foreach (var sid in sidsToDelete)
                        __DB.Delete(sid);

                    __DB.Commit();
                });
            }
            catch
            {
                __DB.Rollback();
                throw;
            }

            ImportResult result = new ImportResult(dllFile);
            result.SidsList = sidsList;
            result.NewStrings = newStrings;
            result.StringsUpdated = stringsUpadted;
            result.DeletedStrings = sidsToDelete;

            return result;
        }

        public int ActiveStringsCount()
        {
            return __DB.ActiveStringsCount();
        }


        public async Task<ImportTranslationResult> ImportTranslation(string fileName, string cultureCode)
        {
            ImportTranslationResult result = new ImportTranslationResult(fileName, cultureCode);

            FileStream fileStream;

            try
            {
                fileStream = new FileStream(fileName, FileMode.Open);
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message;
                return result;
            }

            await Task.Run(() =>
                {
                    List<StringTranslation> originalAndTranslations = __DB.GetTranslationsForCulture(cultureCode, true);

                    CsvReader reader = new CsvReader(fileStream, Encoding.UTF8);
                    var headerRecord = reader.ReadHeaderRecord();

                    var rows = reader.ReadDataRecords();
                    result.Records = rows;

                    List<int> allSids = new List<int>();
                    List<int> sidsUpdated = new List<int>();
                    List<int> sidsWithErrors = new List<int>();

                    __DB.BeginTransaction();
                    try
                    {
                        foreach (DataRecord row in rows)
                        {
                            int sid = int.Parse(row[0]);
                            string original = row[1];
                            string translation = row[2];

                            allSids.Add(sid);

                            StringTranslation stringRow = originalAndTranslations.Find(x => x.Row.Sid == sid);

                            if (stringRow == null)
                            {
                                sidsWithErrors.Add(sid);
                                result.Log.Add(new ImportTranslationResultLogLine(sid, "",
                                    original, translation, ImportTranslationStringErrorReason.StringNotFound));
                            }
                            else
                                if (stringRow.Row.Content != original)
                                {
                                    sidsWithErrors.Add(sid);
                                    result.Log.Add(new ImportTranslationResultLogLine(sid, stringRow.Row.Content,
                                        original, translation, ImportTranslationStringErrorReason.OriginalStringChanged));
                                }
                                else
                                    if (stringRow.Translation == translation ||
                                        (String.IsNullOrEmpty(stringRow.Translation) && String.IsNullOrEmpty(translation)))
                                    {
                                        // now do nothing
                                    }
                                    else
                                    {
                                        __DB.SetTranslation(sid, stringRow.Row.MaxRevision, cultureCode, stringRow.Translation, translation);
                                        sidsUpdated.Add(sid);
                                    }
                        }

                        __DB.Commit();
                    }
                    catch
                    {
                        __DB.Rollback();
                        throw;
                    }

                    result.AllSids = allSids;
                    result.SidsUpdated = sidsUpdated;
                    result.SidsWithErrors = sidsWithErrors;
                }
            );

            return result;
        }

        public async Task ImportTranslationStrings(IProgress<int> progress, CancellationToken cancelToken,
                                                string assemblyFile, string neutralCulture, string cultureCode, List<BamlString> lines)
        {
            __DB.BeginTransaction();
            try
            {
                int line_no = 0, max = lines.Count;
                var enumerator = lines.GetEnumerator();

                bool finished = !enumerator.MoveNext();

                while (!finished)
                {
                    await Task.Run(() =>
                    {
                        for (int t = 0; t < 100; t++)
                        {
                            BamlString line = enumerator.Current;

                            string bamlFile = line.BamlFile.ToLower();
                            bamlFile = bamlFile.Replace(cultureCode.ToLower(), neutralCulture.ToLower());
                            int? sid = __DB.StringId(assemblyFile, bamlFile, line.ResourceKey);
                            if (sid != null)
                            {

                                if (!string.IsNullOrEmpty(line.Content))
                                {
                                    StringRow strRow = __DB.StringForId((int)sid);
                                    if (strRow != null)
                                        __DB.SetTranslation((int)sid, strRow.Revision, cultureCode, null, line.Content);
                                }
                            }

                            line_no += 1;

                            finished = !enumerator.MoveNext();

                            if (finished)
                                break;
                        }
                    });

                    progress.Report(line_no * 100 / max);
                }
            }
            catch
            {
                __DB.Rollback();
                throw;
            }

            __DB.Commit();
        }
    }
}
