using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Birko.Data.DataBase.Field;

namespace Birko.Data.DataBase.Table
{
    public class Table
    {
        public string Name { get; set; }
        public Dictionary<string, Field.AbstractField> Fields { get; set; }

        public IDictionary<int, string> GetSelectFields(bool withName  = false, bool notAggregate = false)
        {
            Dictionary<int, string> fields = new Dictionary<int, string>();
            var keys = Fields.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++)
            {
                var field = Fields[keys[i]];
                if (!notAggregate || !field.IsAggregate)
                {
                    var fieldName = (field.IsAggregate)
                        ? string.Format("{0}({1}) as {2}",
                            field.Name,
                            string.Join(",", (field as Field.FunctionField).Parameters?.Select(x=> string.Format("{0}{1}", (withName ? Name + "." : string.Empty), x)) ?? new string[0]),
                            keys[i])
                        : (withName ? Name + "." : string.Empty) + keys[i];
                    fields.Add(i, fieldName);
                }
            }
            return fields;
        }

        internal IEnumerable<AbstractField> GetTableFields(bool notAggregate)
        {
            List<AbstractField> tableFields = new List<Field.AbstractField>();
            foreach (var field in Fields.Where(x => x.Value != null))
            {
                if (!notAggregate || !field.Value.IsAggregate)
                {
                    tableFields.Add(field.Value);
                }
            }
            return tableFields;
        }

        public bool HasAggregateFields()
        {
            return Fields?.Any(x => x.Value?.IsAggregate ?? false) ?? false;
        }

        internal IEnumerable<Field.AbstractField> GetPrimaryFields()
        {
            return Fields?.Values.Where(x => x.IsPrimary);
        }

        internal Field.AbstractField GetField(string name)
        {
            return (Fields != null && Fields.Any() && Fields.ContainsKey(name)) ? Fields[name] : null;
        }

        internal Field.AbstractField GetFieldByPropertyName(string name)
        {
            return (Fields != null && Fields.Any() && Fields.Any(x=>x.Value.Property != null && x.Value.Property.Name == name))
                ? Fields.FirstOrDefault(x => x.Value.Property != null && x.Value.Property.Name == name).Value
                : null;
        }
    }
}
