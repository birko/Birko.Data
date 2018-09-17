using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Birko.Data.DataBase.Field
{
    public class DecimalField : AbstractField
    {
        public DecimalField(System.Reflection.PropertyInfo property, string name, bool primary = false, bool unique = false, bool autoincrement = false)
            : base(property, name, DbType.Decimal, primary, true, unique, autoincrement)
        {
        }

        public override void Read(object value, DbDataReader reader, int index)
        {
            Property.SetValue(value, reader.GetDecimal(index), null);
        }
    }

    public class NullableDecimalField : DecimalField
    {
        public NullableDecimalField(System.Reflection.PropertyInfo property, string name, bool primary = false, bool unique = false, bool autoincrement = false)
            : base(property, name, primary, unique, autoincrement)
        {
            IsNotNull = false;
        }

        public override void Read(object value, DbDataReader reader, int index)
        {
            if (reader.IsDBNull(index))
            {
                Property.SetValue(value, null, null);
            }
            else
            {
                base.Read(value, reader, index);
            }
        }
    }

    public class DecimalFunction : FunctionField
    {
        public DecimalFunction(System.Reflection.PropertyInfo property, string name, object[] parameters)
            : base(property, name, parameters, DbType.Decimal, true)
        {

        }

        public override void Read(object value, DbDataReader reader, int index)
        {
            Property.SetValue(value, reader.GetDecimal(index), null);
        }
    }

    public class NullableDecimalFunction : DecimalFunction
    {
        public NullableDecimalFunction(System.Reflection.PropertyInfo property, string name, object[] parameters)
            : base(property, name, parameters)
        {
            IsNotNull = false;
        }

        public override void Read(object value, DbDataReader reader, int index)
        {
            if (reader.IsDBNull(index))
            {
                Property.SetValue(value, null, null);
            }
            else
            {
                base.Read(value, reader, index);
            }
        }
    }
}
