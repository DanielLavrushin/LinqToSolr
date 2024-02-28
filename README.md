# LinqToSolr

![](https://img.shields.io/badge/net8.0-compatible-green.svg) ![](https://img.shields.io/badge/netstandard2.1-compatible-green.svg) ![](https://img.shields.io/badge/netstandard2.0-compatible-green.svg) ![](https://img.shields.io/badge/netstandard1.6-compatible-green.svg) ![](https://img.shields.io/badge/netstandard1.3-compatible-green.svg) ![](https://img.shields.io/badge/netstandard1.2-compatible-green.svg) ![](https://img.shields.io/badge/netstandard1.1-compatible-green.svg) ![](https://img.shields.io/badge/net45-compatible-green.svg)

LinqToSolr is an intuitive and lightweight C# library designed to enhance Solr queries with the power of Linq. It seamlessly integrates Solr search capabilities into .NET applications, facilitating cleaner and more maintainable code. LinqToSolr is suitable for developing complex search solutions or simplifying query logic.

nuget package:
[![](https://img.shields.io/badge/nuget.org-package-blue.svg)](https://www.nuget.org/packages/LinqToSolr/)

To install:

```dotnet
dotnet add package LinqToSolr
```

## Supported Methods

The library supports a variety of Linq methods, including but not limited to:

- `Where`
- `First`
- `FirstOrDefault`
- `Select`
- `Contains`
- `Array.Contains(solrField)`
- `StartsWith`
- `EndsWith`
- `GroupBy`
- `Take`
- `Skip`
- `OrderBy`
- `ThenBy`
- `OrderByDescending`
- `ThenByDescending`
- `ToListAsync`
- `AddOrUpdateAsync`
- `DeleteAsync`

## Usage Instructions

### Defining a Model

First, define a model class to represent your Solr document:

```csharp
public class MyProduct{
  public int Id{get;set;}

  [LinqToSolrField("username")]
  public string Name{get;set}
  public string Group{get;set}

  [LinqToSolrField("deleted")]
  public bool IsDeleted{get;set}
  public string[] Tags{get;set;}

  [LinqToSolrFieldIgnore]
  public string CustomField{}
}
```

### Configuration

Initialize a configuration class for the service:

```csharp
var solrConfig = new LinqToSolrConfiguration("http://localhost:8983/") // URL to Solr instance, if solr has different location (not under/solr) set it to  http://localhost:8983/somecustomlocation
                .MapCoreFor<MyProduct>("MyProductIndex"); // Mapping your model to Solr Index
```

#### Creating a Service

Instantiate the base service and provide the configuration:

```csharp
var solrService = new LinqToSolrService(solrConfig);
```

#### Extending LinqToSolrService

You may create a custom service that inherits from LinqToSolrService:

```csharp
public class MyProductService: LinqToSolrService{

  private  Task<IQueryable<>> IQueryable<MyProduct> IsNotDeleted()
  {
       return  AsQueryable<MyProduct>().Where(x=> !x.IsDeleted);
  }

  public async Task<ICollection<MyProduct>> GetProductsByIds(params int[] ids)
  {
      return await IsNotDeleted().Where(x=> ids.Contains(x.Id)).ToListAsync();
  }
}
```

### Linq Query Examples

##### Where Clause

```csharp
await solrService.AsQueryable<MyProduct>().Where(x => x.Group == "MyGroup1").ToListAsync();
```

##### FirstOrDefault

```csharp
var groupArray = new[] { "MyGroup1", "MyGroup2", "MyGroup3", "MyGroup4" };
var product = await solrService.AsQueryable<MyProduct>().FirstOrDefault(x => x.Id == 123);
```

##### OrderBy & OrderByDescending

```csharp
var orderedProducts = await solrService.AsQueryable<MyProduct>()
                                        .Where(x => x.Group == "MyGroup1")
                                        .OrderBy(x => x.Id)
                                        .ThenByDescending(x => x.Name)
                                        .ToListAsync();
```

##### Contains Examples

```csharp
solrService.AsQueriable<MyProduct>().Where(x=> x.Name.Contains("productName")).ToList();
```

```csharp
var array = new[]{"str2", "str3", "str4"};
solrService.AsQueriable<MyProduct>().Where(x=> array.Contains(x.Name)).ToList();
```

```csharp
solrService.AsQueriable<MyProduct>().Where(x=> x.Tags.Contains("tag1")).ToList();
```

##### OrderBy & OrderByDescending

```csharp
await solrService.AsQueriable<MyProduct>().Where(x=> x.Group == "MyGroup1").OrderBy(x=>x.Id).ThenByDescending(x=>x.Name).ToListAsync();
```

##### Select Clause

```csharp
solrService.AsQueriable<MyProduct>().Where(x=> x.Group == "MyGroup1").Select(x=x.Name).ToList();
```

```csharp
solrService.AsQueriable<MyProduct>().Where(x=> x.Group == "MyGroup1").Select(x=x new {x.Name, x.Group}).ToList();
```

### Solr Document Interaction

Adding or Updating Documents
Add or update documents in a Solr core:

```csharp
var products = new[] {
    new MyProduct { Id = 1, Name = "Product One" },
    new MyProduct { Id = 2, Name = "Product Two" },
    new MyProduct { Id = 3, Name = "Product Three" }
};

await solrService.AddOrUpdateAsync(products);
```

Deleting Documents
Delete documents by specifying an array of ids:

```csharp
await solrService.Where(x => x.Id == 123).DeleteAsync<MyProduct>();
```
