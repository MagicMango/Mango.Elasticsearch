using Mango.Elasticsearch.Expressions;
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
        public static BoolQuery CreateElasticSearchQuery(Expression<Func<TParameter, bool>> rootExpression)
        {
            return CheckForHandle(rootExpression.Body, ExpressionType.Default);
        }
        public static BoolQuery CheckForHandle(Expression expression, ExpressionType operandType)
        {
            return expression switch
            {
                Expression e when e is BinaryExpression     => HandleExpression(e as BinaryExpression, operandType),
                Expression e when e is UnaryExpression      => HandleExpression(e as UnaryExpression, operandType),
                Expression e when e is MemberExpression     => HandleExpression(e as MemberExpression, operandType),
                Expression e when e is MethodCallExpression => HandleExpression(e as MethodCallExpression, operandType),
                _ => new BoolQuery()
            };
        }
        private static BoolQuery HandleExpression(MemberExpression memberExpression, ExpressionType operandType)
        {
            throw new InvalidOperationException();
        }
        public static BoolQuery HandleExpression(BinaryExpression binaryExpression, ExpressionType operandType)
        {
            return (binaryExpression.NodeType, operandType, binaryExpression.Left.NodeType, binaryExpression.Right.NodeType) switch
            {
                (_, ExpressionType.Not, _, ExpressionType.MemberAccess)     => new BoolQuery()
                {
                    MustNot = new QueryContainer[] {
                        CreateExpressionsForElasticsearch(new[] 
                        { 
                            EvaluationExpressionHandler
                                .GetEvaluatedExpression(binaryExpression.Left, binaryExpression.Right, binaryExpression.NodeType, operandType) 
                        }.ToList())
                    }
                },
                (_, ExpressionType.Not, ExpressionType.MemberAccess, _)     => new BoolQuery()
                {
                    MustNot = new QueryContainer[] 
                    {
                        CreateExpressionsForElasticsearch(new[] 
                        { 
                            EvaluationExpressionHandler
                                .GetEvaluatedExpression(binaryExpression.Right, binaryExpression.Left, binaryExpression.NodeType, operandType) 
                        }.ToList())
                    }
                },
                (_, ExpressionType.Not, _, _)                               => new BoolQuery()
                {
                    MustNot = new QueryContainer[]
                    {
                        CheckForHandle(binaryExpression.Left, binaryExpression.NodeType), 
                        CheckForHandle(binaryExpression.Right, binaryExpression.NodeType)
                    }
                },
                (ExpressionType.OrElse, _, _, _)                            => new BoolQuery()
                {
                    Should = new QueryContainer[]
                    {
                        CheckForHandle(binaryExpression.Left, binaryExpression.NodeType), 
                        CheckForHandle(binaryExpression.Right, binaryExpression.NodeType)
                    }
                },
                (ExpressionType.AndAlso, _, _, _)                           => new BoolQuery()
                {
                    Must = new QueryContainer[]
                    {
                        CheckForHandle(binaryExpression.Left, binaryExpression.NodeType), 
                        CheckForHandle(binaryExpression.Right, binaryExpression.NodeType)
                    }
                },
                _ => CreateExpressionsForElasticsearch(new[] 
                    { 
                        EvaluationExpressionHandler
                            .GetEvaluatedExpression(binaryExpression.Right, binaryExpression.Left, binaryExpression.NodeType, operandType) 
                    }.ToList())
            };
        }
        public static BoolQuery HandleExpression(UnaryExpression unaryExpression, ExpressionType operandType)
        {
            return CheckForHandle(unaryExpression.Operand, unaryExpression.NodeType);
        }
        public static BoolQuery HandleExpression(MethodCallExpression methodCallExpression, ExpressionType operandType)
        {
            var evaluatedExpressions = new List<EvaluatedExpression>();
            EvaluatedExpression evaluatedExpression = methodCallExpression.Arguments.Count switch
            {
                2 => EvaluationExpressionHandler.GetEvaluatedExpression(methodCallExpression.Arguments[0], methodCallExpression.Arguments[1], methodCallExpression.NodeType),
                _ => EvaluationExpressionHandler.GetEvaluatedExpression(methodCallExpression.Arguments[0], methodCallExpression.Object, methodCallExpression.NodeType),
            };
            evaluatedExpression.CallMethod = methodCallExpression.Method.Name;
            evaluatedExpression.CombineOperation = operandType;
            evaluatedExpressions.Add(evaluatedExpression);
            return CreateExpressionsForElasticsearch(evaluatedExpressions);
        }
        private static BoolQuery CreateExpressionsForElasticsearch(List<EvaluatedExpression> evaluatedExpressions)
        {
            var must = evaluatedExpressions
                .Where(x => x.Operation != ExpressionType.NotEqual && x.CombineOperation != ExpressionType.OrElse)
                .Select(x => QueryContainerFactory.CreateContainer(x))
                .ToList();

            var should = evaluatedExpressions
                .Where(x => x.Operation != ExpressionType.NotEqual && x.CombineOperation == ExpressionType.OrElse)
                .Select(x => QueryContainerFactory.CreateContainer(x))
                .ToList();

            var mustnot = evaluatedExpressions
                .Where(x => x.Operation == ExpressionType.NotEqual)
                .Select(x => QueryContainerFactory.CreateContainer(x))
                .ToList();

            return BoolQueryHandler.CreateBoolQuery(must, should, mustnot);
        }
    }
}