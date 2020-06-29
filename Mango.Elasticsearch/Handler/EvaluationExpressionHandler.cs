using Mango.Elasticsearch.Expressions;
using Mango.Elasticsearch.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static Mango.Elasticsearch.Extensions.ObjectExtensions;

namespace Mango.ElasticSearch.Handler
{
    public class EvaluationExpressionHandler
    {
        public static EvaluatedExpression GetEvaluatedExpression(Expression value, Expression member, ExpressionType nodeOperation, ExpressionType combineOperation)
        {
            object result = null;
            string method = null;

            result = value.NodeType switch
            {
                ExpressionType.MemberAccess => (value as MemberExpression).GetValue(),
                ExpressionType.Constant     => (value as ConstantExpression).Value,
                ExpressionType.Call         => ExecuteFunc(()=> {
                                                    method = (value as MethodCallExpression).Method.Name;
                                                    return  ((value as MethodCallExpression).Object as MemberExpression).GetValue();
                                                }),
                _ => default
            };

            return new EvaluatedExpression
            {
                PropertyName = member switch
                {
                    MethodCallExpression m => ((Func<string>)(() => (m.Object as MemberExpression).Member.Name))(),
                    _ => ExtractMemberName(member as MemberExpression)
                },
                CallMethod = method,
                Operation = nodeOperation,
                CombineOperation = combineOperation,
                Value = result
            };
        }

        public static EvaluatedExpression GetEvaluatedExpression(Expression member, Expression value, ExpressionType nodeOperation)
        {
            return GetEvaluatedExpression(member, value, nodeOperation, nodeOperation);
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
    }
}