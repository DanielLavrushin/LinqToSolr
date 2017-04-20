# LinqToSolr
This is a lightwave C# library which provides Linq support for Solr.
LinqToSolr implements IQueriable<> interface, which allows you to call Solr API with linq expressions directly.

## Supported Methods
* Where
* First
* FirstOrDefault
* Select
* GroupBy
* GroupByFacets
* Take
* Skip

## How to use
First, create a model Class which will represent your Solr Document

```c#

public class MyProduct{
  public int Id{get;set;}
  public string Name{get;set}
  public string Group{get;set}
  public bool IsDeleted{get;set}
}

```

Then initialize a configuration class for a serivce

```c#

var solrConfig = new LinqToSolrRequestConfiguration("http:/localhost:1433/") // url to solr instance
                .MapIndexFor<MyProduct>("MyProductIndex"); // the way to map your model to Solr Index

```

Create base service and provide the configuration to it

```c#

var solrService = new LinqToSolrService(solrConfig);

```

You could create your custom inherited LinqToSolrService

```c#

public class MyProductService: LinqToSolrService{

  private IQueryable<MyProduct> IsNotDeleted()
  {
       return AsQueryable<MyProduct>().Where(x=> !x.IsDeleted);
  }

  public ICollection<MyProduct> GetProductsByIds(params int[] ids)
  {
      return IsNotDeleted().Where(x=> ids.Contains(x.Id)).ToList();
  }
}

```

### Linq Query examples
#### Where
---
```c#

solrService.AsQueriable<MyProduct>().Where(x=>x.Group == "MyGroup1").ToList();

```

```c#
var groupArray = new[] { "MyGroup1", "MyGroup2", "MyGroup3", "MyGroup4" };
solrService.AsQueriable<MyProduct>().Where(x=> groupArray.Contains(x.Group)).ToList();

```

```c#

solrService.AsQueriable<MyProduct>().Where(x=>x.Group.Contains("MyGroup")).ToList();

```

```c#

solrService.AsQueriable<MyProduct>().Where(x=>x.Group.StartsWith("MyGroup")).ToList();

```

```c#

solrService.AsQueriable<MyProduct>().Where(x=>x.Group.EndsWith("oup1")).ToList();

```

```c#

solrService.AsQueriable<MyProduct>().Where(x=> !x.IsDeleted && x.Name.Contains("productName")).ToList();

```
