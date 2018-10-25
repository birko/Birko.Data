﻿using Birko.Data.DataBase.Field;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Birko.Data.DataBase
{
    public static partial class DataBase
    {
        private static Dictionary<Type, IEnumerable<Field.AbstractField>> _fieldsCache = null;

        private static IEnumerable<AbstractField> LoadFields(Type type)
        {
            if (_fieldsCache == null)
            {
                _fieldsCache = new Dictionary<Type, IEnumerable<Field.AbstractField>>();
            }
            if (!_fieldsCache.ContainsKey(type))
            {
                List<AbstractField> list = new List<AbstractField>();
                foreach (var field in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                {
                    list.AddRange(LoadField(field));
                }
                _fieldsCache.Add(type, list.ToArray());
            }
            return _fieldsCache[type];
        }

        public static IEnumerable<AbstractField> LoadField(PropertyInfo field)
        {
            List<AbstractField> list = new List<AbstractField>();
            object[] fieldAttrs = field.GetCustomAttributes(typeof(Attribute.Field), true);
            if (fieldAttrs != null)
            {
                foreach (Attribute.Field fieldAttr in fieldAttrs)
                {
                    var tableField = Field.AbstractField.CreateAbstractField(field, fieldAttr);

                    if (tableField != null)
                    {
                        list.Add(tableField);
                    }
                }
            }
            else
            {
                throw new Exceptions.FieldAttributeException("No field attributes in type");
            }
            return list.ToArray();
        }

        public static AbstractField GetField<T, P>(Expression<Func<T, P>> expr)
        {
            PropertyInfo propInfo = null;
            if (expr.Body is UnaryExpression expression)
            {
                propInfo = (expression.Operand as MemberExpression).Member as PropertyInfo;

            }
            else if(expr.Body is  MemberExpression memberExpression)
            {
                propInfo = memberExpression.Member as PropertyInfo;
            }
            var fields = LoadField(propInfo);
            return fields.First();
        }

        public static IEnumerable<AbstractField> GetPrimaryFields(Type type)
        {
            var table = LoadTable(type);
            return table?.GetPrimaryFields() ?? new AbstractField[0];
        }

        public static int Read(IEnumerable<Field.AbstractField> fields, DbDataReader reader, object data, int index = 0)
        {
            if (fields != null)
            {
                foreach (var tableField in fields)
                {
                    tableField.Read(data, reader, index);
                    index++;
                }
            }
            return index;
        }

        public static Dictionary<string, object> Write(IEnumerable<Field.AbstractField> fields, object data)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            if (fields != null)
            {
                foreach (var tableField in fields)
                {
                    result.Add(tableField.Name, tableField.Write(data));
                }
            }
            return result;
        }
    }
}
