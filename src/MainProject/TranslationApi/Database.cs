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
using System.IO;
using TranslationApi.Properties;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data;
using TranslationApi.Baml;

namespace TranslationApi
{

    public class Database
    {

        // Подразумевается, что база данных может находится в отдельном файле, или вообще в сети.
        // в этом случае в файле хранятся только настройки подключения, а все данных находятся в сетевой БД.
        // При сохранении проекта под другим именем, сохраняется только параметры настройки к БД.
        private string __DefinitionFile;
        private ISQLInterface __DB;

        /// <summary>
        /// Подразумевается, что база данных может находится в отдельном файле, или вообще в сети.
        /// в этом случае в файле хранятся только настройки подключения, а все данных находятся в сетевой БД.
        /// При сохранении проекта под другим именем, сохраняется только параметры настройки к БД.        
        /// </summary>
        /// <param name="fileName">Имя файла с базой или только с определением параметров подключения к БД</param>
        public Database(string fileName)
        {
            __DefinitionFile = fileName;

            if (!File.Exists(fileName))
                CreateDB(fileName);
            else
                OpenDB(fileName);
        }

        public void CloseAndMoveTo(string fileName)
        {
            if (fileName == __DefinitionFile)
                return;

            __DB.Close();

            try
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);

                File.Move(__DefinitionFile, fileName);
            }
            finally
            {

            }

            return;
        }

        public void Close()
        {
            __DB.Close();
        }

        private void CreateDB(string fileName)
        {
            SQLiteInterface dbInterface = new SQLiteInterface();
            dbInterface.Open(fileName);
            __DB = dbInterface;

            CreateTables();
        }

        private void OpenDB(string fileName)
        {
            SQLiteInterface dbInterface = new SQLiteInterface();
            dbInterface.Open(fileName);

            __DB = dbInterface;
        }

        private void CreateTables()
        {
            __DB.ExecuteQuery(Resources.transdb_sqlite);
        }

        public int? Revision(int sid)
        {
            return __DB.SelectOne("SELECT max_revision FROM string_ids where id=@sid",
                new Dictionary<string, object>() { { "@sid", sid } }) as int?;
        }

        public void StringAddRevision(int sid, int revision, string text, bool isApproved = false, bool isDeleted = false)
        {
            __DB.ExecuteQuery("INSERT INTO original_strings (string_id, date_added, revision, string, is_approved, is_deleted) " +
                                        " VALUES(@sid, " + __DB.NowFunction() + ", @revision, @text, @is_approved, @is_deleted)",
                        new Dictionary<string, object>() {
                            {"@sid", sid}, 
                            {"@revision", revision}, 
                            {"@text", text}, 
                            {"@is_approved", isApproved}, 
                            {"@is_deleted", isDeleted}});

            __DB.ExecuteQuery("UPDATE string_ids set is_deleted=@is_deleted, max_revision=@max_revision where id=@sid", new Dictionary<string, object>()
                {
                    {"@sid", sid},
                    {"@is_deleted", isDeleted ? 1 : 0},
                    {"@max_revision", revision}
                });
        }

        public int? StringId(string filename, string baml, string uid)
        {
            return (int?)UnboxNumeric(__DB.SelectOne("SELECT id FROM string_ids WHERE filename=@filename AND baml=@baml AND uid=@uid", new Dictionary<string, object>() {
                {"@filename", filename.ToLower()}, 
                {"@baml", baml},
                {"@uid", uid}}));
        }

        private static Dictionary<string, object> KeyDict(string key)
        {
            var keyDict = new Dictionary<string, object>()
            {
                {"@key", key}
            };
            return keyDict;
        }

        private static Dictionary<string, object> KeyValueDict(string key, object value)
        {
            var keyValueDict = new Dictionary<string, object>()
            {
                {"@key", key},
                {"@value", value}
            };
            return keyValueDict;
        }

        public string GetStringValue(string key, string defaultValue)
        {
            var value = __DB.SelectOne("SELECT value FROM settings where key=@key", KeyDict(key));

            if (value == null)
                return defaultValue;

            return System.Text.Encoding.UTF8.GetString((byte[])value);

        }

        public void SetStringValue(string key, string value)
        {
            SetValue(key, value);
        }

        private void SetValue(string key, object value)
        {
            var keyCount = __DB.SelectOne("select count(*) from settings where key=@key", KeyDict(key));

            if (UnboxNumeric(keyCount) == 0)
                __DB.ExecuteQuery("INSERT INTO settings (key, value) VALUES(@key, @value)", KeyValueDict(key, value));
            else
                __DB.ExecuteQuery("UPDATE settings SET value=@value WHERE key=@key", KeyValueDict(key, value));
        }

        private long? UnboxNumeric(object value)
        {
            if (value == null)
                return null;

            if (typeof(int) == value.GetType())
                return (long)(int)value;

            if (typeof(short) == value.GetType())
                return (long)(short)value;

            if (typeof(long) == value.GetType())
                return (long)value;

            throw new InvalidCastException(String.Format("Type {0} is unexpected in UnboxNumber", value));
        }

        private StringRow StringRowFromTableRow(DataRow tableRow)
        {
            StringRow stringRow = new StringRow();
            stringRow.Sid = Int32.Parse(tableRow["id"].ToString());
            stringRow.DllFile = (string)tableRow["filename"];
            stringRow.BamlFile = (string)tableRow["baml"];
            stringRow.ResourceKey = (string)tableRow["uid"];
            stringRow.Category = (string)tableRow["category"];
            stringRow.Readability = (string)tableRow["readability"];
            stringRow.Localizability = (string)tableRow["localizability"];
            stringRow.Comments = (string)tableRow["description"];
            stringRow.Revision = (int)UnboxNumeric(tableRow["max_revision"]);

            return stringRow;
        }

        private List<StringRow> RowListFromDataTable(DataTable table)
        {
            List<StringRow> result = new List<StringRow>();
            foreach (DataRow tableRow in table.Rows)
                result.Add(StringRowFromTableRow(tableRow));

            return result;
        }

        public List<LocalizableAssembly> LocalizableAssemblies()
        {
            var assemblies = __DB.SelectAll("SELECT * FROM assemblies");
            List<LocalizableAssembly> result = new List<LocalizableAssembly>();

            for (int i = 0; i < assemblies.Rows.Count; i++)
            {
                DataRow row = assemblies.Rows[i];

                result.Add(new LocalizableAssembly(
                    (string)row["assembly_file"],
                    (string)row["default_culture"],
                    (string)row["default_resource"]));
            }

            return result;
        }

        public void SaveAssemblies(List<LocalizableAssembly> assemblies)
        {
            BeginTransaction();
            try
            {
                var assembliesInDB = __DB.SelectAll("SELECT * from Assemblies");                

                foreach (DataRow assemblyRow in assembliesInDB.Rows)
                {
                    
                    string assemblyDBFile = assemblyRow.Field<string>("assembly_file");
                    var foundCount = assemblies.Count( 
                                        (assembly) => 
                                            assembly.AssemblyFile.ToLower() == assemblyDBFile.ToLower());
                    if (foundCount == 0)
                    {
                        __DB.ExecuteQuery("DELETE from Assemblies where assembly_file=@assembly_file", new Dictionary<string,object>() 
                            {
                                {"@assembly_file", assemblyDBFile}
                            });
                    }
                }

                foreach (var assembly in assemblies)
                {
                    var isAssemblyInDB = __DB.SelectOne("Select count(*) from Assemblies where assembly_file=@assembly_file", new Dictionary<string,object>()
                        {
                            {"@assembly_file", assembly.AssemblyFile}
                        } );

                    if (UnboxNumeric(isAssemblyInDB) == 0)
                    {
                        __DB.ExecuteQuery("INSERT INTO Assemblies(assembly_file, default_culture, default_resource) VALUES (@file, @culture, @resource)", new Dictionary<string, object>() {
                            {"@file", assembly.AssemblyFile},
                            {"@culture", assembly.DefaultCulture},
                            {"@resource", assembly.DefaultResourceFile}
                        });
                    }
                }

                Commit();
            }
            catch
            {
                Rollback();
                throw;
            }
        }

        internal List<StringRow> SelectAllSids(string dllFile = null)
        {
            DataTable table;

            if (dllFile == null)
                table = __DB.SelectAll("SELECT * from string_ids where is_deleted=0");
            else
                table = __DB.SelectAll("SELECT * from string_ids where filename=@dllFile and is_deleted = 0", new Dictionary<string, object>()
                {
                    {"dllFile", dllFile.ToLower()}
                });

            return RowListFromDataTable(table);
        }

        internal bool IsDeleted(int sid)
        {
            int isDeleted = Int32.Parse(
                __DB.SelectOne("select is_deleted from string_ids where id=@sid", new Dictionary<string, object>()
            {
                {"@sid", sid},                
            }).ToString());

            return isDeleted != 0;
        }

        /// <summary>
        /// Поменить строку как удаленную.
        /// </summary>
        /// <param name="sid"></param>
        internal void Delete(int sid)
        {
            int? maxRevision = Revision(sid);
            if (maxRevision == null)
                return;

            int newRevision = (int)maxRevision + 1;
            StringAddRevision(sid, newRevision, "", false, true);
        }

        internal int NewStringId(string dllFile, BamlString line)
        {
            __DB.ExecuteQuery("insert into string_ids " +
                               "(filename, baml, uid, category, readability, localizability, description) " +
                "values (@filename, @baml, @uid, @category, @readability, @localizability, @description)",
                new Dictionary<string, object>() {
                    {"@filename", dllFile.ToLower()},
                    {"@baml", line.BamlFile},
                    {"@uid", line.ResourceKey},
                    {"@category", (int)line.Category},
                    {"@readability", line.Readability},
                    {"@localizability", line.Modifiable},
                    {"@description", line.Comments}
                });

            return __DB.LastInsertedId("string_ids");
        }

        internal StringRow StringForId(int sid)
        {

            DataTable stringIdDataRows =
                __DB.SelectAll("SELECT * from string_ids inner join original_strings on string_ids.id = original_strings.string_id where string_ids.id=@sid and original_strings.revision = string_ids.max_revision", new Dictionary<string, object>() 
            {
                {"@sid", sid}
            });

            if (stringIdDataRows == null)
                return null;

            if (stringIdDataRows.Rows.Count == 0)
                return null;

            DataRow stringIdDataRow = stringIdDataRows.Rows[0];
            StringRow row = StringRowFromTableRow(stringIdDataRow);
            row.Content = (string)stringIdDataRow["string"];

            return row;
        }

        internal void BeginTransaction()
        {
            __DB.BeginTransaction();
        }

        internal void Rollback()
        {
            __DB.Rollback();
        }

        internal void Commit()
        {
            __DB.Commit();
        }

        internal int ActiveStringsCount()
        {
            return (int)UnboxNumeric(__DB.SelectOne(@"SELECT count(*) from string_ids WHERE is_deleted=0"));
        }

        public void AddCulture(string cultureCode)
        {
            var cultures = GetCultureCodes();
            if (cultures.Where(c => c == cultureCode).Count() > 0)
                return;

            __DB.ExecuteQuery("INSERT INTO cultures (culture_code) VALUES(@culture_code)", new Dictionary<string, object>() 
                {
                    {"culture_code", cultureCode}
                });
        }

        public void RemoveCulture(string cultureCode)
        {
            __DB.ExecuteQuery("DELETE FROM cultures where culture_code=@culture_code", new Dictionary<string, object>()
                {
                    {"culture_code", cultureCode}
                });
        }

        public List<string> GetCultureCodes()
        {
            List<string> result = new List<string>();
            foreach (DataRow culture in __DB.SelectAll("SElECT culture_code from cultures").Rows)
                result.Add(culture["culture_code"].ToString());

            return result;
        }

        public List<TranslationCulture> GetCultureCodesWithStatistics()
        {
            var cultures = __DB.SelectAll(
                              @"SELECT culture_code, 
                                (SELECT count(*) from translated_strings 
                                 INNER JOIN string_ids ON string_ids.id = translated_strings.string_id 
                                 WHERE string_ids.max_revision=translated_strings.revision AND 
                                       translated_strings.culture_code=cultures.culture_code) as count FROM cultures");

            List<TranslationCulture> result = new List<TranslationCulture>();
            foreach (DataRow row in cultures.Rows)
                result.Add(new TranslationCulture((string)row["culture_code"], (int)UnboxNumeric(row["count"])));

            return result;
        }



        internal List<StringTranslation> GetTranslationsForCulture(string cultureCode, bool includeTranslated, 
            string assemblyFileName = "")
        {

            DataTable originalString;
            if (assemblyFileName == "")
                originalString = GetTranslationsDataTableForCulture(cultureCode);
            else
                originalString = __DB.SelectAll(@"SELECT string_ids.id, string_ids.filename, 
                                                         string_ids.baml, string_ids.max_revision, 
                                                         string_ids.uid, string_ids.category, 
                                                         string_ids.readability, string_ids.localizability,
                                                         string_ids.description,
                                                         original_strings.string, 
                                                                (SELECT translation FROM translated_strings WHERE string_id=string_ids.id 
                                                                                                              AND revision=string_ids.max_revision 
                                                                                                              AND culture_code=@cultureCode) as translation
                                                          FROM string_ids, original_strings 
                                                          WHERE string_ids.id = original_strings.string_id 
                                                            AND string_ids.filename = @assemblyFileName
                                                            AND original_strings.is_deleted = 0 
                                                            AND original_strings.revision = string_ids.max_revision 
                                                            AND string <> ''",
                new Dictionary<string, object>() {
                            {"@cultureCode", cultureCode},
                            {"@assemblyFileName", assemblyFileName.ToLower()}
                        });


            List<StringTranslation> result = new List<StringTranslation>();

            foreach (DataRow row in originalString.Rows)
            {
                StringRow stringRow = new StringRow();
                stringRow.Sid = (int)UnboxNumeric(row["id"]);
                stringRow.DllFile = (string)row["filename"];
                stringRow.BamlFile = (string)row["baml"];
                stringRow.ResourceKey = (string)row["uid"];
                stringRow.Localizability = (string)row["localizability"];
                stringRow.Readability = (string)row["readability"];
                stringRow.Category = (string)row["category"];
                stringRow.Comments = (string)row["description"];
                stringRow.Content = (string)row["string"];
                stringRow.MaxRevision = (int)UnboxNumeric(row["max_revision"]);

                string translation = null;
                if (!(row["translation"] is DBNull))
                    translation = (string)row["translation"];

                StringTranslation translationRow = new StringTranslation(stringRow, translation);

                if (!String.IsNullOrEmpty(translation) && includeTranslated)
                    result.Add(translationRow);

                if (String.IsNullOrEmpty(translation))
                    result.Add(translationRow);
            }

            return result;
        }

        public DataTable GetTranslationsDataTableForCulture(string cultureCode)
        {
            DataTable originalString;
            originalString = __DB.SelectAll(@"SELECT string_ids.id, string_ids.filename, 
                                                         string_ids.baml, string_ids.max_revision, 
                                                         string_ids.uid, string_ids.category, 
                                                         string_ids.readability, string_ids.localizability,
                                                         string_ids.description,                                                         
                                                         original_strings.string, 
                                                         @cultureCode as culture_code,
                                                         (SELECT translation FROM translated_strings WHERE string_id=string_ids.id 
                                                                                                              AND revision=string_ids.max_revision 
                                                                                                              AND culture_code=@cultureCode) as translation
                                                          FROM string_ids, original_strings 
                                                          WHERE string_ids.id = original_strings.string_id 
                                                            AND original_strings.is_deleted = 0 
                                                            AND original_strings.revision = string_ids.max_revision 
                                                            AND string <> ''",
            new Dictionary<string, object>() {
                            {"@cultureCode", cultureCode}
                        });
            return originalString;
        }


        internal void SetTranslation(DataRow dataRow, object translatedString)
        {
            SetTranslation(
                (int) UnboxNumeric(dataRow["id"]),
                (int) UnboxNumeric(dataRow["max_revision"]),
                (string)dataRow["culture_code"],
                dataRow["translation"] as string,
                translatedString as string);
        }

        internal void SetTranslation(int sid, int revision, string cultureCode, string translationFromDB, string translation)
        {
            if (translationFromDB == null)
            {
                __DB.ExecuteQuery(@"INSERT INTO translated_strings (string_id, revision, culture_code, translation)
                                                VALUES(@sid, @revision, @culture_code, @translation)", 
                        new Dictionary<string,object>()
                        {
                            {"@sid", sid},
                            {"@revision", revision},
                            {"@culture_code", cultureCode},
                            {"@translation", translation}
                        });
            } else
            {
                __DB.ExecuteQuery(@"UPDATE translated_strings SET translation=@translation 
                                                WHERE string_id=@sid and revision=@revision and culture_code=@culture_code",
                        new Dictionary<string, object>()
                        {
                            {"@sid", sid},
                            {"@translation", translation},
                            {"@revision", revision},
                            {"@culture_code", cultureCode}
                        });
            }
        }        
    }
}