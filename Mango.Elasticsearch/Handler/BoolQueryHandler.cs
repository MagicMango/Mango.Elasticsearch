using Nest;
using System.Collections.Generic;
using System.Linq;

namespace Mango.ElasticSearch.Handler
{
    public static class BoolQueryHandler
    {
        public static BoolQuery CreateBoolQuery(IEnumerable<QueryContainer> must, IEnumerable<QueryContainer> should, IEnumerable<QueryContainer> mustnot)
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
