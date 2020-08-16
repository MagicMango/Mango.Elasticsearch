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
            result = value switch
            {
                MemberExpression memberExpression => memberExpression?.GetValue(),
                ConstantExpression constantExpression => (value as ConstantExpression).Value,
                MethodCallExpression methodCallExpression => ExecuteFunc(() => {
                    method = methodCallExpression.Method.Name;
                    return methodCallExpression.Object switch
                    {
                        MemberExpression innerMemberExpression => innerMemberExpression?.GetValue(),
                        _ => default
                    };
                }),
                _ => default
            };
            return new EvaluatedExpression
            {
                PropertyName = member switch
                {
                    MethodCallExpression m => ExecuteFunc(() => m.Object switch
                    {
                        MemberExpression memberExpression => memberExpression?.Member.Name,
                        _ => null
                    }),
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
            if (memberExpressionName?.Member != null)
            {
                memberNames.Add(memberExpressionName.Member.Name.ToLowerCamelCase());
            }
            memberNames.Reverse();
            return string.Join('.', memberNames);
        }
    }
}