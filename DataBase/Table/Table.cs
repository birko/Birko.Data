using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Birko.Data.DataBase.Table
{
    public class Table
    {
        public string Name { get; set; }
        public Dictionary<string, Field.AbstractField> Fields { get; set; }

        public IDictionary<int, string> GetSelectFields()
        {
            Dictionary<int, string> fields = new Dictionary<int, string>();
            var keys = Fields.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++)
            {
                fields.Add(i, keys[i]);
            }
            return fields;
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
