using Mango.Elasticsearch.Expressions;
using Mango.Elasticsearch.Extensions;
using Mango.Elasticsearch.Factory;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Mango.ElasticSearch.Handler
{
    public static class ExpressionEvaluationHandler<TParameter>
        where TParameter : class
    {
        public static BoolQuery CreateElasticSearchQuery(Expression<Func<TParameter, bool>> parameterToCheck)
        {
            return SplitLogicalExpressions(parameterToCheck.Body as BinaryExpression);
        }

        private static BoolQuery SplitLogicalExpressions(BinaryExpression parameterToCheck)
        {
            var evaluatedExpressions = new List<EvaluatedExpression>();
            BinaryExpression binaryExpression = parameterToCheck;

            if (binaryExpression.NodeType == ExpressionType.OrElse)
            {
                return new BoolQuery() { Should = new QueryContainer[] { SplitLogicalExpressions(binaryExpression.Left as BinaryExpression), SplitLogicalExpressions(binaryExpression.Right as BinaryExpression) } };
            }

            GetExpressionResults(binaryExpression, evaluatedExpressions, binaryExpression.NodeType);

            var must = evaluatedExpressions
                .Where(x => x.Operation != ExpressionType.NotEqual)
                .Select(x => QueryFactory.CreateContainer(x));

            var should = evaluatedExpressions
                .Where(x => x.Operation != ExpressionType.NotEqual && x.CombineOperation == ExpressionType.OrElse)
                .Select(x => QueryFactory.CreateContainer(x));

            var mustnot = evaluatedExpressions
                .Where(x => x.Operation == ExpressionType.NotEqual)
                .Select(x => QueryFactory.CreateContainer(x));

            return CreateBoolQuery(must, should, mustnot);
        }

        private static void GetExpressionResults(BinaryExpression binayExpression, List<EvaluatedExpression> evaluatedExpressions, ExpressionType combineOperation)
        {
            if (binayExpression?.Left != null && (binayExpression.Left.NodeType != ExpressionType.MemberAccess && binayExpression.Left.NodeType != ExpressionType.Constant))
                GetExpressionResults(binayExpression.Left as BinaryExpression, evaluatedExpressions, binayExpression.NodeType);

            if (binayExpression?.Right != null && (binayExpression.Right.NodeType != ExpressionType.MemberAccess && binayExpression.Right.NodeType != ExpressionType.Constant))
                GetExpressionResults(binayExpression.Right as BinaryExpression, evaluatedExpressions, binayExpression.NodeType);

            if (binayExpression != null && binayExpression.Left is MemberExpression && binayExpression.Right != null)
            {
                object result = null;

                switch (binayExpression.Right.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        result = (binayExpression.Right as MemberExpression).GetValue();
                        break;
                    case ExpressionType.Constant:
                        result = (binayExpression.Right as ConstantExpression).Value;
                        break;
                }

                evaluatedExpressions.Add(new EvaluatedExpression
                {
                    PropertyName = (binayExpression.Left as MemberExpression).Member.Name,
                    Operation = binayExpression.NodeType,
                    CombineOperation = combineOperation,
                    Value = result
                });
            }
        }
        private static BoolQuery CreateBoolQuery(IEnumerable<QueryContainer> must, IEnumerable<QueryContainer> should, IEnumerable<QueryContainer> mustnot)
        {
            if (should.Count() != 0 && must.Count() == 0)
            {
                return new BoolQuery()
                {
                    Should = should.Select(x => new QueryContainer(new BoolQuery() { Must = new QueryContainer[] { x } })).ToArray(),
                    MustNot = mustnot
                };
            }
            else if (should.Count() != 0)
            {
                return new BoolQuery()
                {
                    Should = new QueryContainer[] { new BoolQuery() { Must = must }, new BoolQuery() { Must = should } },
                    MustNot = mustnot
                };
            }
            return new BoolQuery()
            {
                Must = must,
                Should = should,
                MustNot = mustnot,
            };
        }
    }
}