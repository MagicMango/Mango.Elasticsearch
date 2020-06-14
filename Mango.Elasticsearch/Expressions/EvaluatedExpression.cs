using System.Linq.Expressions;

namespace Mango.Elasticsearch.Expressions
{
    public class EvaluatedExpression
    {
        public string PropertyName { get; set; }
        public ExpressionType Operation { get; set; }
        public object Value { get; set; }
        public ExpressionType CombineOperation { get; set; }
    }
}
