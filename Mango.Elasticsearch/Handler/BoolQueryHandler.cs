using Nest;
using System.Collections.Generic;
using System.Linq;

namespace Mango.ElasticSearch.Handler
{
    public static class BoolQueryHandler
    {
        public static BoolQuery CreateBoolQuery(IEnumerable<QueryContainer> must, IEnumerable<QueryContainer> should, IEnumerable<QueryContainer> mustnot)
        {
            return ((must.Any(), should.Any(), mustnot.Any()) switch
            {
                (false, true, _) => new BoolQuery()
                {
                    Should = should.Select(x => new QueryContainer(new BoolQuery() { Must = new QueryContainer[] { x } })).ToArray(),
                    MustNot = mustnot
                },
                (_, true, _) => new BoolQuery()
                {
                    Should = new QueryContainer[] { new BoolQuery() { Must = must }, new BoolQuery() { Must = should } },
                    MustNot = mustnot
                },
                _ => new BoolQuery()
                {
                    Must = must,
                    Should = should,
                    MustNot = mustnot,
                }
            });
        }
    }
}
