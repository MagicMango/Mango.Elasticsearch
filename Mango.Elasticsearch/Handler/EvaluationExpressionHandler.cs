using Mango.Elasticsearch.Expressions;
using Mango.Elasticsearch.Extensions;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Mango.ElasticSearch.Handler
{
    public class EvaluationExpressionHandler
    {
        public static EvaluatedExpression GetEvaluatedExpression(Expression value, Expression member, ExpressionType nodeOperation, ExpressionType combineOperation)
        {
            object result = null;
            string method = null;

            switch (value.NodeType)
            {
                case ExpressionType.MemberAccess:
                    result = (value as MemberExpression).GetValue();
                    break;
                case ExpressionType.Constant:
                    result = (value as ConstantExpression).Value;
                    break;
                case ExpressionType.Call:
                    method = (value as MethodCallExpression).Method.Name;
                    result = ((value as MethodCallExpression).Object as MemberExpression).GetValue();
                    break;
            }

            return new EvaluatedExpression
            {
                PropertyName = ExtractMemberName(member as MemberExpression),
                CallMethod = method,
                Operation = nodeOperation,
                CombineOperation = combineOperation,
                Value = result
            };
        }

        public static string ExtractMemberName(MemberExpression memberExpressionName)
        {
            List<string> memberNames = new List<string>();
            while (memberExpressionName?.Expression != null)
            {
                memberNames.Add(memberExpressionName.Member.Name.ToLowerCamelCase());
                memberExpressionName = memberExpressionName.Expression as MemberExpression;
            }
            memberNames.Reverse();
            return string.Join('.', memberNames);
        }

        public static EvaluatedExpression GetEvaluatedExpression(Expression member, Expression value, ExpressionType nodeOperation)
        {
            return GetEvaluatedExpression(member, value, nodeOperation, nodeOperation);
        }
    }
}
