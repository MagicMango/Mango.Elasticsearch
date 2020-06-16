# Mango.Elasticsearch
### Testclass to illustrate
```csharp
public class ToTest
    {
        public int Id { get; set; }
        public string MyPropertyS { get; set; }
        public DateTime MyDateProperty { get; set; }
    }
```
### Linq querys which can be translated to Elasticsearch queries
```csharp
BoolQuery query = ExpressionEvaluationHandler<ToTest>
                .CreateElasticSearchQuery(
                    x => x.Id ==5 || 
                    x.MyPropertyS.Contains("Hello World: 4") ||
                    x.Id == 6 && x.MyPropertyS == "Hello World: 5");

var response = elasticClient
                .Search<ToTest>(new SearchRequest<ToTest>("[someIndex]") 
                    { 
                      Size = 1000, 
                      Query = query 
                    });
```