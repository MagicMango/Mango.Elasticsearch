using Mango.Elasticsearch.Expressions;
using Mango.Elasticsearch.Extensions;
using Nest;
using System;
using System.Linq.Expressions;

namespace TinySnouts.Elasticsearch.Factory
{
    public static class QueryFactory
    {
        public static QueryContainer CreateContainer(EvaluatedExpression evaluatedExpression)
        {
            QueryContainer queryContainer = null;

            if (evaluatedExpression.Operation == ExpressionType.Equal || evaluatedExpression.Operation == ExpressionType.NotEqual)
            {
                return new QueryContainer(new MatchQuery()
                {
                    Field = new Field(evaluatedExpression.PropertyName.ToLowerCamelCase() + ((evaluatedExpression.Value is string) ? ".keyword" : string.Empty)),
                    Query = evaluatedExpression.Value.ToString()
                });
            }

            if (evaluatedExpression.Operation == ExpressionType.Call)
            {
                return HandleMethodCalls(evaluatedExpression);
            }

            if (evaluatedExpression.Value.IsNumeric())
            {
                return HandleNumeric(evaluatedExpression);
            }

            if (evaluatedExpression.Value is DateTime)
            {
                return HandleDateTime(evaluatedExpression);
            }

            return queryContainer;
        }

        private static QueryContainer HandleMethodCalls(EvaluatedExpression evaluatedExpression)
        {
            var matchQuery = new MatchQuery()
            {
                Field = new Field(evaluatedExpression.PropertyName.ToLowerCamelCase())
            };
            switch (evaluatedExpression.CallMethod)
            {
                case "StartsWith":
                    matchQuery.Query = evaluatedExpression.Value.ToString() + "*";
                    break;
                case "EndsWith":
                    matchQuery.Query = "*" + evaluatedExpression.Value.ToString();
                    break;
                default:
                    matchQuery.Query = evaluatedExpression.Value.ToString();
                    break;
            }
            return new QueryContainer(matchQuery);
        }

        private static QueryContainer HandleDateTime(EvaluatedExpression evaluatedExpression)
        {
            var dateRangeQuery = new DateRangeQuery()
            {
                Field = new Field(evaluatedExpression.PropertyName.ToLowerCamelCase()),
            };
            switch (evaluatedExpression.Operation)
            {
                case ExpressionType.LessThan:
                    dateRangeQuery.LessThan = Convert.ToDateTime(evaluatedExpression.Value);
                    break;
                case ExpressionType.LessThanOrEqual:
                    dateRangeQuery.LessThanOrEqualTo = Convert.ToDateTime(evaluatedExpression.Value);
                    break;
                case ExpressionType.GreaterThan:
                    dateRangeQuery.GreaterThan = Convert.ToDateTime(evaluatedExpression.Value);
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    dateRangeQuery.GreaterThanOrEqualTo = Convert.ToDateTime(evaluatedExpression.Value);
                    break;
            }
            return new QueryContainer(dateRangeQuery);
        }
        private static QueryContainer HandleNumeric(EvaluatedExpression evaluatedExpression)
        {
            var numericRangeQuery = new NumericRangeQuery()
            {
                Field = new Field(evaluatedExpression.PropertyName.ToLowerCamelCase()),
            };
            switch (evaluatedExpression.Operation)
            {
                case ExpressionType.LessThan:
                    numericRangeQuery.LessThan = new double?(Convert.ToDouble(evaluatedExpression.Value));
                    break;
                case ExpressionType.LessThanOrEqual:
                    numericRangeQuery.LessThanOrEqualTo = new double?(Convert.ToDouble(evaluatedExpression.Value));
                    break;
                case ExpressionType.GreaterThan:
                    numericRangeQuery.GreaterThan = new double?(Convert.ToDouble(evaluatedExpression.Value));
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    numericRangeQuery.GreaterThanOrEqualTo = new double?(Convert.ToDouble(evaluatedExpression.Value));
                    break;
                default:
                    numericRangeQuery = null;
                    break;

            }
            return new QueryContainer(numericRangeQuery);
        }
    }
}