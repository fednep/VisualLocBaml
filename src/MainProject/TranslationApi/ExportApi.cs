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
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TranslationApi.KBCsv;

namespace TranslationApi
{
    public class ExportApi
    {
        private Database __DB;

        public ExportApi(Database db)
        {
            __DB = db;

        }

        public async Task<ExportResult> ExportStrings(string fileName, string cultureCode, bool includeTranslated)
        {
            ExportResult result =
               await Task<ExportResult>.Run(() =>
               {
                   List<StringTranslation> rows = __DB.GetTranslationsForCulture(cultureCode, includeTranslated);
                   FileStream fileStream = null;
                   try
                   {
                       fileStream = new FileStream(fileName, FileMode.Create);
                   }
                   catch (Exception e)
                   {

                       return new ExportResult(fileName, cultureCode, ExportCSVResult.Fail, 0, e.Message) ;
                   }

                   using (CsvWriter writer = new CsvWriter(fileStream, Encoding.UTF8))
                   {
                       writer.ValueDelimiter = '"';
                       writer.ValueSeparator = ',';

                       writer.WriteHeaderRecord("Id", "Original string", "Translation");

                       foreach (var row in rows)
                           writer.WriteDataRecord(row.Row.Sid, row.Row.Content, row.Translation);
                   }

                   return new ExportResult(fileName, cultureCode, ExportCSVResult.Success, rows.Count);
               });

            return result;
        }

        public void SetTranslation(int sid, int revision, string cultureCode, string translationFromDB, string translation)
        {
            __DB.SetTranslation(sid, revision, cultureCode, translationFromDB, translation);
        }

        public DataTable GetTranslationStringsDataTable(string cultureCode)
        {
            return __DB.GetTranslationsDataTableForCulture(cultureCode);
        }

        public List<BamlString> GetTranslatedStrings(string assemblyFile, string cultureCode)
        {
            List<StringTranslation> strings = __DB.GetTranslationsForCulture(cultureCode, true, assemblyFile);
            List<BamlString> result = new List<BamlString>();
            TypeConverter stringCatConverter = TypeDescriptor.GetConverter(LocalizationCategory.Text);

            foreach (var stringTranslation in strings)
            {
                if (stringTranslation.Row.DllFile.ToLower() == assemblyFile.ToLower() && 
                    !string.IsNullOrEmpty(stringTranslation.Translation))
                {

                    LocalizationCategory category = (LocalizationCategory)stringCatConverter.ConvertFrom(stringTranslation.Row.Category);
                    result.Add(
                        new BamlString(stringTranslation.Row.BamlFile, 
                                       stringTranslation.Row.ResourceKey, 
                                       category, 
                                       stringTranslation.Row.Readability, 
                                       stringTranslation.Row.Localizability, 
                                       stringTranslation.Row.Comments,
                                       stringTranslation.Translation));
                }
            }

            return result;
        }

        public void SetTranslation(DataRow dataRow, object translatedString)
        {
            __DB.SetTranslation(dataRow, translatedString);
        }
    }
}
