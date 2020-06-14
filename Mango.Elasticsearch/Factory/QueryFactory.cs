using Mango.Elasticsearch.Expressions;
using Mango.Elasticsearch.Extensions;
using Nest;
using System;
using System.Linq.Expressions;

namespace Mango.Elasticsearch.Factory
{
    public static class QueryFactory
    {
        public static QueryContainer CreateContainer(EvaluatedExpression evaluatedExpression)
        {
            QueryContainer queryContainer = null;

            if (evaluatedExpression.Operation == ExpressionType.Equal)
            {
                return new QueryContainer(new MatchQuery()
                {
                    Field = new Field(evaluatedExpression.PropertyName.ToLowerCamelCase() + ((evaluatedExpression.Value is string) ? ".keyword" : string.Empty)),
                    Query = evaluatedExpression.Value.ToString()
                });
            }

            if (evaluatedExpression.Operation == ExpressionType.NotEqual)
            {
                return new QueryContainer(new MatchQuery()
                {
                    Field = new Field(evaluatedExpression.PropertyName.ToLowerCamelCase() + ((evaluatedExpression.Value is string) ? ".keyword" : string.Empty)),
                    Query = evaluatedExpression.Value.ToString()
                });
            }

            if (evaluatedExpression.Operation == ExpressionType.LessThan && evaluatedExpression.Value.IsNumeric())
            {
                return new QueryContainer(new NumericRangeQuery()
                {
                    Field = new Field(evaluatedExpression.PropertyName.ToLowerCamelCase()),
                    LessThan = new double?(Convert.ToDouble(evaluatedExpression.Value))
                });
            }

            if (evaluatedExpression.Operation == ExpressionType.LessThanOrEqual && evaluatedExpression.Value.IsNumeric())
            {
                return new QueryContainer(new NumericRangeQuery()
                {
                    Field = new Field(evaluatedExpression.PropertyName.ToLowerCamelCase()),
                    LessThanOrEqualTo = new double?(Convert.ToDouble(evaluatedExpression.Value))
                });
            }

            if (evaluatedExpression.Operation == ExpressionType.GreaterThan && evaluatedExpression.Value.IsNumeric())
            {
                return new QueryContainer(new NumericRangeQuery()
                {
                    Field = new Field(evaluatedExpression.PropertyName.ToLowerCamelCase()),
                    GreaterThan = new double?(Convert.ToDouble(evaluatedExpression.Value))
                });
            }

            if (evaluatedExpression.Operation == ExpressionType.GreaterThanOrEqual && evaluatedExpression.Value.IsNumeric())
            {
                return new QueryContainer(new NumericRangeQuery()
                {
                    Field = new Field(evaluatedExpression.PropertyName.ToLowerCamelCase()),
                    GreaterThanOrEqualTo = new double?(Convert.ToDouble(evaluatedExpression.Value))
                });
            }

            if (evaluatedExpression.Operation == ExpressionType.LessThan && evaluatedExpression.Value is DateTime)
            {
                return new QueryContainer(new DateRangeQuery()
                {
                    Field = new Field(evaluatedExpression.PropertyName.ToLowerCamelCase()),
                    LessThan = Convert.ToDateTime(evaluatedExpression.Value)
                });
            }

            if (evaluatedExpression.Operation == ExpressionType.LessThanOrEqual && evaluatedExpression.Value is DateTime)
            {
                return new QueryContainer(new DateRangeQuery()
                {
                    Field = new Field(evaluatedExpression.PropertyName.ToLowerCamelCase()),
                    LessThanOrEqualTo = Convert.ToDateTime(evaluatedExpression.Value)
                });
            }

            if (evaluatedExpression.Operation == ExpressionType.GreaterThan && evaluatedExpression.Value is DateTime)
            {
                return new QueryContainer(new DateRangeQuery()
                {
                    Field = new Field(evaluatedExpression.PropertyName.ToLowerCamelCase()),
                    GreaterThan = Convert.ToDateTime(evaluatedExpression.Value)
                });
            }

            if (evaluatedExpression.Operation == ExpressionType.GreaterThanOrEqual && evaluatedExpression.Value is DateTime)
            {
                return new QueryContainer(new DateRangeQuery()
                {
                    Field = new Field(evaluatedExpression.PropertyName.ToLowerCamelCase()),
                    GreaterThanOrEqualTo = Convert.ToDateTime(evaluatedExpression.Value)
                });
            }

            return queryContainer;
        }
    }
}