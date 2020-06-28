using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace Mango.Elasticsearch.Extensions
{
    public static class ObjectExtensions
    {
        public static string GetFullTypeName(this object o)
        {
            var type = (Type)((o is Type) ? o : o.GetType());
            var typeName = type.Name;
            var args = type.GetGenericArguments();
            if (args.Count() == 0) return typeName;

            typeName = typeName.Remove(typeName.LastIndexOf("`"));
            return typeName
                     + "<"
                     + string.Join(", ", args.Select(a => a.IsGenericType ? GetFullTypeName(a) : a.Name))
                     + ">";
        }
        public static object GetValue(this MemberExpression member)
        {
            if (member != null)
            {
                var objectMember = Expression.Convert(member, typeof(object));
                var getterLambda = Expression.Lambda<Func<object>>(objectMember);
                var getter = getterLambda.Compile();
                return getter();
            }
            return null;
        }
        public static string ToStringExtendend(this object o) => o switch
        {
            DateTime time => time.ToString(CultureInfo.InvariantCulture.DateTimeFormat.SortableDateTimePattern),
            _ => o.ToString()
        };
        public static TEntity ExecuteFunc<TEntity>(Func<TEntity> funcToRun)
            where TEntity: new()
        {
            return funcToRun();
        }
        public static bool IsNumeric(this object o) => o is byte || o is sbyte || o is ushort || o is uint || o is ulong || o is short || o is int || o is long || o is float || o is double || o is decimal;
    }
}
