using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Birko.Data.DataBase.Field;

namespace Birko.Data.DataBase.Table
{
    public class View
    {
        public string Name { get; private set; }
        public IEnumerable<Table> Tables { get; private set; }
        public IEnumerable<Condition.Join> Join { get; private set; }

        public View(IEnumerable<Table> tables = null, IEnumerable<Condition.Join> join = null, string name = null)
        {
            Tables = tables;
            Join = join;
            Name = name;
            if (!string.IsNullOrEmpty(name) && Tables != null && Tables.Any())
            {
                Name = string.Join(string.Empty, Tables.Select(x => x.Name).Where(x => !string.IsNullOrEmpty(x)).Distinct());
            }
        }

        public View AddTable(Table table)
        {
            return AddTable(table.Name, table.Fields.Values);
        }

        public View AddTable(string tableName, IEnumerable<Field.AbstractField> fields)
        {
            if (fields != null && fields.Any())
            {
                foreach (var field in fields)
                {
                    AddField(tableName, field);
                }
            }
            return this;
        }

        public View AddField(string tableName, AbstractField field)
        {
            if (!string.IsNullOrEmpty(tableName) && field != null)
            {
                Table table = null;
                if (Tables != null && Tables.Any() && Tables.Any(x => x.Name == Name))
                {
                    table = Tables.FirstOrDefault(x => x.Name == Name);
                }
                else
                {
                    table = new Table() { Name = tableName };
                    Tables = (Tables == null) ? new[] { table } : Tables.Concat(new[] { table });
                }
            }
            return this;
        }

        public View AddJoin(IEnumerable<Condition.Join> conditions)
        {
            if (conditions != null && conditions.Any())
            {
                foreach (var condition in conditions)
                {
                    AddJoin(condition);
                }
            }
            return this;
        }

        public View AddJoin(Condition.Join condition)
        {
            if (condition != null)
            {
                Join = (Join == null) ? new[] { condition } : Join.Concat(new[] { condition });
            }
            return this;
        }

        public IDictionary<int, string> GetSelectFields()
        {
            var result = new Dictionary<int, string>();
            int i = 0;
            foreach (var table in Tables)
            {
                var fields = table?.GetSelectFields();
                if (fields != null && fields.Any())
                {
                    foreach (var field in fields)
                    {
                        result.Add(i, table.Name + "." + field.Value);
                        i++;
                    }
                }
            }
            return result;
        }
    }
}
