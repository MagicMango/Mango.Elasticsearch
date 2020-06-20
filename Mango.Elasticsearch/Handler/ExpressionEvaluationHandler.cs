using Mango.Elasticsearch.Expressions;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TinySnouts.Elasticsearch.Factory;

namespace Mango.ElasticSearch.Handler
{
    public static class ExpressionEvaluationHandler<TParameter>
        where TParameter : class
    {
        public static BoolQuery CreateElasticSearchQuery(Expression<Func<TParameter, bool>> parameterToCheck)
        {
            return HandleExpression(parameterToCheck.Body);
        }

        private static BoolQuery HandleExpression(Expression parameterToCheck)
        {
            if (parameterToCheck is BinaryExpression)
            {
                return SplitLogicalExpressions(parameterToCheck as BinaryExpression);
            }
            if (parameterToCheck is MethodCallExpression)
            {
                return SplitLogicalExpressions(parameterToCheck as MethodCallExpression);
            }
            return null;
        }

        private static BoolQuery SplitLogicalExpressions(BinaryExpression parameterToCheck)
        {
            var evaluatedExpressions = new List<EvaluatedExpression>();
            BinaryExpression binaryExpression = parameterToCheck;

            if (binaryExpression.NodeType == ExpressionType.OrElse && binaryExpression.Left is BinaryExpression && binaryExpression.Right is BinaryExpression)
            {
                return new BoolQuery() { Should = new QueryContainer[] { SplitLogicalExpressions(binaryExpression.Left as BinaryExpression), SplitLogicalExpressions(binaryExpression.Right as BinaryExpression) } };
            }
            else if (binaryExpression.NodeType == ExpressionType.OrElse && binaryExpression.Left is MethodCallExpression && binaryExpression.Right is BinaryExpression)
            {
                return new BoolQuery() { Should = new QueryContainer[] { SplitLogicalExpressions(binaryExpression.Left as MethodCallExpression), SplitLogicalExpressions(binaryExpression.Right as BinaryExpression) } };
            }
            else if (binaryExpression.NodeType == ExpressionType.OrElse && binaryExpression.Left is BinaryExpression && binaryExpression.Right is MethodCallExpression)
            {
                return new BoolQuery() { Should = new QueryContainer[] { SplitLogicalExpressions(binaryExpression.Left as BinaryExpression), SplitLogicalExpressions(binaryExpression.Right as MethodCallExpression) } };
            }
            else if (binaryExpression.NodeType == ExpressionType.OrElse && binaryExpression.Left is MethodCallExpression && binaryExpression.Right is MethodCallExpression)
            {
                return new BoolQuery() { Should = new QueryContainer[] { SplitLogicalExpressions(binaryExpression.Left as MethodCallExpression), SplitLogicalExpressions(binaryExpression.Right as MethodCallExpression) } };
            }

            GetExpressionResults(binaryExpression, evaluatedExpressions, binaryExpression.NodeType);

            return CreateExpressionsForElasticsearch(evaluatedExpressions);
        }

        private static BoolQuery SplitLogicalExpressions(MethodCallExpression methodCallExpression)
        {
            var evaluatedExpressions = new List<EvaluatedExpression>();
            EvaluatedExpression evaluatedExpression = methodCallExpression.Arguments.Count switch
            {
                2 => EvaluationExpressionHandler.GetEvaluatedExpression(methodCallExpression.Arguments[0], methodCallExpression.Arguments[1], methodCallExpression.NodeType),
                _ => EvaluationExpressionHandler.GetEvaluatedExpression(methodCallExpression.Arguments[0], methodCallExpression.Object, methodCallExpression.NodeType),
            };
            evaluatedExpression.CallMethod = methodCallExpression.Method.Name;
            evaluatedExpressions.Add(evaluatedExpression);

            return CreateExpressionsForElasticsearch(evaluatedExpressions);
        }
        private static BoolQuery CreateExpressionsForElasticsearch(List<EvaluatedExpression> evaluatedExpressions)
        {
            var must = evaluatedExpressions
                            .Where(x => x.Operation != ExpressionType.NotEqual)
                            .Select(x => QueryFactory.CreateContainer(x));

            var should = evaluatedExpressions
                .Where(x => x.Operation != ExpressionType.NotEqual && x.CombineOperation == ExpressionType.OrElse)
                .Select(x => QueryFactory.CreateContainer(x));

            var mustnot = evaluatedExpressions
                .Where(x => x.Operation == ExpressionType.NotEqual)
                .Select(x => QueryFactory.CreateContainer(x));

            return BoolQueryHandler.CreateBoolQuery(must, should, mustnot);
        }

        private static void GetExpressionResults(BinaryExpression binayExpression, List<EvaluatedExpression> evaluatedExpressions, ExpressionType combineOperation)
        {
            if (binayExpression?.Left != null && (binayExpression.Left.NodeType != ExpressionType.MemberAccess && binayExpression.Left.NodeType != ExpressionType.Constant))
                GetExpressionResults(binayExpression.Left as BinaryExpression, evaluatedExpressions, binayExpression.NodeType);

            if (binayExpression?.Right != null && (binayExpression.Right.NodeType != ExpressionType.MemberAccess && binayExpression.Right.NodeType != ExpressionType.Constant))
                GetExpressionResults(binayExpression.Right as BinaryExpression, evaluatedExpressions, binayExpression.NodeType);

            if (binayExpression != null && binayExpression.Left is MemberExpression && binayExpression.Right != null)
            {
                evaluatedExpressions.Add(EvaluationExpressionHandler.GetEvaluatedExpression(binayExpression.Right, binayExpression.Left, binayExpression.NodeType, combineOperation));
            }
        }
    }
}