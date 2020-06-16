using Mango.Elasticsearch.Expressions;
using Mango.Elasticsearch.Extensions;
using System.Linq.Expressions;

namespace Mango.ElasticSearch.Handler
{
    public class EvaluationExpressionHandler
    {
        public static EvaluatedExpression GetEvaluatedExpression(Expression member, Expression value, ExpressionType nodeOperation, ExpressionType combineOperation)
        {
            object result = null;

            switch (member.NodeType)
            {
                case ExpressionType.MemberAccess:
                    result = (member as MemberExpression).GetValue();
                    break;
                case ExpressionType.Constant:
                    result = (member as ConstantExpression).Value;
                    break;
            }

            return new EvaluatedExpression
            {
                PropertyName = (value as MemberExpression).Member.Name,
                Operation = nodeOperation,
                CombineOperation = combineOperation,
                Value = result
            };
        }
    }
}
