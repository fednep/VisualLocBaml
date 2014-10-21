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
using System.Data.SQLite;
using System.Data;

namespace TranslationApi
{

    public class SQLiteInterface : ISQLInterface
    {
        private SQLiteConnection __Connection;
        private object __Lock;

        public SQLiteInterface()
        {
            __Lock = new object();
        }

        public void Open(string databaseFileName)
        {
            __Connection = new SQLiteConnection(
                String.Format("Data source={0}", databaseFileName));

            __Connection.Open();
        }

        private SQLiteCommand CreateCommandWithParameters(string query, Dictionary<string, object> parameters)
        {
            SQLiteCommand result = new SQLiteCommand(__Connection);

            result.CommandText = query;
            foreach (var parameterName in parameters.Keys)
                result.Parameters.Add(new SQLiteParameter(parameterName, parameters[parameterName]));

            return result;
        }

        public void ExecuteQuery(string query)
        {
            ExecuteQuery(query, new Dictionary<string, object>() { });
        }

        public void ExecuteQuery(string query, Dictionary<string, object> parameters)
        {
            using (var command = CreateCommandWithParameters(query, parameters))
            {
                command.ExecuteNonQuery();
            }

        }

        public DataTable SelectAll(string query)
        {
            return SelectAll(query, new Dictionary<string, object>() { });
        }

        public DataTable SelectAll(string query, Dictionary<string, object> parameters)
        {
            using (var command = CreateCommandWithParameters(query, parameters))
            {
                //    using (SQLiteDataReader reader = command.ExecuteReader())
                //    {
                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(command))
                {

                    //adapter.d
                    DataTable result = new DataTable();
                    adapter.Fill(result);
                    //result.Load(reader);
                    return result;
                }
            }
        }

        public object SelectOne(string query)
        {
            return SelectOne(query, new Dictionary<string, object>());
        }

        public object SelectOne(string query, Dictionary<string, object> parameters)
        {
            using (var command = CreateCommandWithParameters(query, parameters))
            {
                return command.ExecuteScalar();
            }
        }

        public void Close()
        {
            __Connection.Close();
        }


        public int LastInsertedId(string tableName)
        {
            return Int32.Parse(SelectOne("SELECT last_insert_rowid() FROM " + tableName).ToString());
        }


        public void BeginTransaction()
        {
            ExecuteQuery("BEGIN TRANSACTION");
        }

        public void Rollback()
        {
            ExecuteQuery("ROLLBACK");
        }

        public void Commit()
        {
            ExecuteQuery("COMMIT");
        }


        public string NowFunction()
        {
            return "DATE(\"NOW\")";
        }

    }
}
