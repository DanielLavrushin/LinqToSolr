# LinqToSolr
This is a lightwave C# library which provides Linq support for Solr.
LinqToSolr implements IQueriable<> interface, which allows you to call Solr API with linq expressions directly.
[NUGET Package](https://www.nuget.org/packages/LinqToSolr/)
> PM> Install-Package LinqToSolr

## Supported Methods
* Where
* First
* FirstOrDefault
* Select
* GroupBy
* GroupByFacets
* Take
* Skip
* OrderBy
* ThenBy
* OrderByDescending
* ThenByDescending

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

#### FirstOrDefault
---
```c#

solrService.AsQueriable<MyProduct>().FirstOrDefault(x=>x.Id == 123);

```

#### OrderBy & OrderByDescending
---
```c#

solrService.AsQueriable<MyProduct>().Where(x=> x.Group == "MyGroup1").OrderBy(x=>x.Id).ThenByDescending(x=>x.Name).ToList();

```

#### Select
---
Below find an example of a custom service you could create in your project
```c#
solrService.AsQueriable<MyProduct>().Where(x=> x.Group == "MyGroup1").Select(x=x.Name).ToList();
```

```c#
solrService.AsQueriable<MyProduct>().Where(x=> x.Group == "MyGroup1").Select(x=x new {x.Name, x.Group}).ToList();
```


### Custom Service - Example
---
Below find an example of how to implement a custom service inherited from LinqToSolrService

```c#
public class MySolrService : LinqToSolrService 
{
    public MySolrService(LinqToSolrRequestConfiguration config) : base (config)
    {    }

    public IQueryable<MyProduct> NotDeleted()
    {
            return AsQueryable<MyProduct>().Where(x=> !x.IsDeleted);
    }

    public ICollection<MyProduct> GetProducts(params int[] ids)
    {
            return NotDeleted().Where(x=> ids.Contains(x.Id)).OrderBy(x=>x.Name).ToList();
    }

    public MyProduct GetProduct(id)
    {
            return NotDeleted().FirstOrDefault(x=> x.Id == id);
    }

    public string[] GetGroups(id)
    {
            return NotDeleted().GroupBy(x=> x.Group).ToArray();
    }
}
```

## Solr Documents Interaction
It is also possible to Add, Update or Delete documents

### Methods
Add and Update methods are combined to one C# Method. If the document with the solr unique key already exists in a core, Solr will just update it or create a new one if the document is new.

LinqToSolrService Service contains 2 methods to add or delete documents:
* AddOrUpdate<T>(params T[] documents) - Add or update documents.
* Delete<T>(params object[] documentIds) - delete documents by provided collection of ids
* Delete<T>(Func<T, bool> query) - delete documents by query (the overload method for Delete)

### How to add or update
Here is an example to change the document data
```c#
 var products = new [] {
    new MyProduct{Id = "Product1", Name= "Product One"},
    new MyProduct{Id = "Product2", Name= "Product Two"},
    new MyProduct{Id = "Product3", Name= "Product Three"} 
 };
 
 solrService.AddOrUpdate(products); // done, solr just added/updated 3 documents
```

Example: change product group
```c#
 var products = solrService.AsQueryable<MyProduct>().Where(x=> x.Group == "Group1").ToList();
 foreach(var p in products)
 {
    p.Group = "Group2";
 }
 solrService.AddOrUpdate(products);
```
### How to Delete
To delete specific products just provide an array of ids (or one document id) to a Delete method
```c#
  solrService.Delete<MyProduct>("Product123"); // delete one product with id Product123
 ```
 
 Or you could provide an array of ids to delete
 ```c#
  var productIds = solrService.AsQueryable<MyProduct>().Where(x=> x.Group == "Group1").Select(x=>x.Id).ToArray(); // select all ids by group
  
  solrService.Delete<MyProduct>(productIds); // delete documents by array of ids
```

 Or simply use query
 ```c#
  solrService.Delete<MyProduct>(x=> x.Group.Contains("Group to Delete"));
```
