using Mango.Elasticsearch.Expressions;
using Mango.Elasticsearch.Extensions;
using Nest;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Mango.Elasticsearch.Factory
{
    public static class QueryFactory
    {
        public static QueryContainer CreateContainer(EvaluatedExpression evaluatedExpression)
        {
            return evaluatedExpression switch
            {
                EvaluatedExpression e when e.Operation == ExpressionType.Call ||
                                           e.CallMethod != null => HandleMethodCalls(evaluatedExpression),
                EvaluatedExpression e when e.Operation == ExpressionType.Equal ||
                                           e.Operation == ExpressionType.NotEqual
                                                                                    => new QueryContainer(new MatchQuery()
                                                                                    {
                                                                                        Field = new Field(evaluatedExpression.PropertyName.ToLowerCamelCase() + ((evaluatedExpression.Value is string) ? ".keyword" : string.Empty)),
                                                                                        Query = evaluatedExpression.Value.ToStringExtendend()
                                                                                    }),
                EvaluatedExpression e when e.Value.IsNumeric() => HandleNumeric(evaluatedExpression),
                EvaluatedExpression e when e.Value is DateTime => HandleDateTime(evaluatedExpression),
                _ => null
            };
        }

        private static QueryContainer HandleMethodCalls(EvaluatedExpression evaluatedExpression)
        {
            var matchQuery = new MatchQuery()
            {
                Field = new Field(evaluatedExpression.PropertyName.ToLowerCamelCase())
            };
            return evaluatedExpression.CallMethod switch
            {
                "StartsWith" => ((Func<QueryContainer>)(() =>
                {
                    matchQuery.Query = evaluatedExpression.Value.ToString() + "*";
                    return new QueryContainer(matchQuery);
                }))(),
                "EndsWith" => ((Func<QueryContainer>)(() =>
                {
                    matchQuery.Query = "*" + evaluatedExpression.Value.ToString();
                    return new QueryContainer(matchQuery);
                }))(),
                "ToLower" => ((Func<QueryContainer>)(() =>
                {
                    matchQuery.Query = evaluatedExpression.Value.ToString().ToLower();
                    return new QueryContainer(matchQuery);
                }))(),
                "ToUpper" => ((Func<QueryContainer>)(() =>
                {
                    matchQuery.Query = evaluatedExpression.Value.ToString().ToUpper();
                    return new QueryContainer(matchQuery);
                }))(),
                "Contains" => EvaluateContains(evaluatedExpression.Value, matchQuery),
                _ => ((Func<QueryContainer>)(() =>
                {
                    matchQuery.Query = evaluatedExpression.Value.ToString();
                    return new QueryContainer(matchQuery);
                }))()
            };
        }

        private static QueryContainer EvaluateContains(object value, MatchQuery matchQuery)
        {
            var r = new List<QueryContainer>();
            foreach (var item in (IList)value)
            {
                r.Add(new QueryContainer(new BoolQuery() { Must = new QueryContainer[] { new MatchQuery() { Field = matchQuery.Field, Query = item.ToString() } } }));
            }
            return new QueryContainer(new BoolQuery() { Should = r });
        }

        private static QueryContainer HandleDateTime(EvaluatedExpression evaluatedExpression)
        {
            var dateRangeQuery = new DateRangeQuery()
            {
                Field = new Field(evaluatedExpression.PropertyName.ToLowerCamelCase()),
            };
            _ = (evaluatedExpression.Operation switch
            {
                ExpressionType.LessThan => dateRangeQuery.LessThan = Convert.ToDateTime(evaluatedExpression.Value),
                ExpressionType.LessThanOrEqual => dateRangeQuery.LessThanOrEqualTo = Convert.ToDateTime(evaluatedExpression.Value),
                ExpressionType.GreaterThan => dateRangeQuery.GreaterThan = Convert.ToDateTime(evaluatedExpression.Value),
                ExpressionType.GreaterThanOrEqual => dateRangeQuery.GreaterThanOrEqualTo = Convert.ToDateTime(evaluatedExpression.Value),
                _ => default
            });
            return new QueryContainer(dateRangeQuery);
        }
        private static QueryContainer HandleNumeric(EvaluatedExpression evaluatedExpression)
        {
            var numericRangeQuery = new NumericRangeQuery()
            {
                Field = new Field(evaluatedExpression.PropertyName.ToLowerCamelCase()),
            };
            _ = (evaluatedExpression.Operation switch
            {
                ExpressionType.LessThan => numericRangeQuery.LessThan = new double?(Convert.ToDouble(evaluatedExpression.Value)),
                ExpressionType.LessThanOrEqual => numericRangeQuery.LessThanOrEqualTo = new double?(Convert.ToDouble(evaluatedExpression.Value)),
                ExpressionType.GreaterThan => numericRangeQuery.GreaterThan = new double?(Convert.ToDouble(evaluatedExpression.Value)),
                ExpressionType.GreaterThanOrEqual => numericRangeQuery.GreaterThanOrEqualTo = new double?(Convert.ToDouble(evaluatedExpression.Value)),
                _ => default

            });
            return new QueryContainer(numericRangeQuery);
        }
    }
}