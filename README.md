# Mango.Elasticsearch
### Testclass to illustrate
```csharp
 public class ToTest
    {
        public int Id { get; set; }
        public string MyPropertyS { get; set; }
        public DateTime MyDateProperty { get; set; }
        public ToTest Child { get; set; }
    }
```
### Linq querys which can be translated to Elasticsearch queries testet with NEST 7.7.1
```csharp
BoolQuery query = ExpressionEvaluationHandler<ToTest>
                .CreateElasticSearchQuery(
                    x =>
                        x.Id == 5
                        ||
                        x.MyPropertyS.EndsWith("7")
                        ||
                        x.Child.MyPropertyS == "Hello World: 2"
                        ||
                        x.Id == 6 && x.MyPropertyS == ts
                        ||
                        ids.Contains(x.Id)
					);

var response = elasticClient
                .Search<ToTest>(new SearchRequest<ToTest>("[someIndex]") 
                    { 
                      Size = 1000, 
                      Query = query 
                    });
```
