using CardMakerGachaSqlGenerator.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CardMakerGachaSqlGenerator
{
    public static class SqlGenerator
    {
        record TableNameValuePair(string Name, object Value, Type Type);

        public static string GenerateSqlNameValuePairs(object obj)
        {
            var type = obj.GetType();
            var fieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            var propInfos = type.GetProperties().Concat(type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)).DistinctBy(x => x.Name);

            var pairs = fieldInfos.Select(x =>
            {
                var attr = x.GetCustomAttribute<SqlColnumAttribute>();
                if (attr != null)
                    return new TableNameValuePair(attr.SqlColnumName, x.GetValue(obj), x.FieldType);
                return default;
            }).Concat(propInfos.Select(x =>
            {
                var attr = x.GetCustomAttribute<SqlColnumAttribute>();
                if (attr != null)
                    return new TableNameValuePair(attr.SqlColnumName, x.GetValue(obj), x.PropertyType);
                return default;
            })).Where(x => x != null).OrderBy(x => x.Name).ToArray();

            var names = $"({string.Join(",", pairs.Select(x => x.Name))})";
            var values = $"({string.Join(",", pairs.Select(x =>
            {
                if (x.Type == typeof(DateTime))
                {
                    //2024-06-17 00:11:12
                    return $"'{x.Value:yyyy-MM-dd HH:mm:ss}'";
                }

                if (x.Type == typeof(string))
                {
                    return $"'{x.Value}'";
                }

                if (x.Type.IsEnum)
                {
                    return $"{(int)x.Value}";
                }

                if (x.Value is null)
                {
                    return "NULL";
                }

                return x.Value.ToString();
            }))})";

            return $"{names} VALUES {values}";
        }

        public static string GenerateInsert(string table, object obj)
        {
            var pairs = GenerateSqlNameValuePairs(obj);
            return $"insert into {table} {pairs};";
        }

        public static string GenerateReplace(string table, object obj)
        {
            var pairs = GenerateSqlNameValuePairs(obj);
            return $"replace into {table} {pairs};";
        }
    }
}
