﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Birko.Data.DataBase.Connector
{
    public abstract partial class AbstractConnector
    {
        public void Insert(object model)
        {
            if (model != null)
            {
                Insert(model.GetType(), model);
            }
        }

        public void Insert(Type type, object model)
        {
            Insert(DataBase.LoadTable(type), model);
        }

        public void Insert(Table.Table table, object model)
        {
            if (model != null)
            {
                Insert(table, new[] { DataBase.Write(table.Fields.Select(f => f.Value), model) });
            }
        }

        public void Insert(Table.Table table, IDictionary<string, object> values)
        {
            var tableName = table.Name;
            Insert(tableName, values);
        }

        public void Insert(Table.Table table, IEnumerable<object> models)
        {
            if (models != null && models.Any() && models.Any(x => x != null))
            {
                var tableName = table.Name;
                Insert(tableName, models.Where(x => x != null).Select(x => DataBase.Write(table.Fields.Select(f => f.Value), x)));
            }
        }

        private void Insert(string tableName, IDictionary<string, object> values)
        {
            Insert(tableName, new[] { values });
        }

        public void Insert(Table.Table table, IEnumerable<IDictionary<string, object>> values)
        {
            var tableName = table.Name;
            Insert(tableName, values);
        }

        public void Insert(string tableName, IEnumerable<IDictionary<string, object>> values)
        {
            if (values != null && values.Any() && values.All(x => x.Any()))
            {
                var first = values.First();
                using (var db = CreateConnection(_settings))
                {
                    db.Open();
                    using (var transaction = db.BeginTransaction())
                    {
                        try
                        {
                            using (var command = db.CreateCommand())
                            {
                                command.CommandText = "INSERT INTO " + tableName
                                    + " (" + string.Join(", ", first.Keys) + ")"
                                    + " VALUES"
                                    + " (" + string.Join(", ", first.Keys.Select(x => "@" + x)) + ")";
                                foreach (var kvp in first)
                                {
                                    var parameter = command.CreateParameter();
                                    parameter.ParameterName = "@" + kvp.Key;
                                    parameter.Value = kvp.Value;
                                    command.Parameters.Add(parameter);
                                }
                                foreach (var item in values)
                                {
                                    foreach (var kvp in item)
                                    {
                                        command.Parameters["@" + kvp.Key].Value = kvp.Value ?? DBNull.Value;
                                    }
                                    command.ExecuteNonQuery();
                                }
                            }
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            InitException(ex);
                        }
                    }
                }
            }
        }
    }
}