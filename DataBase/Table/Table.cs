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
    }
}
