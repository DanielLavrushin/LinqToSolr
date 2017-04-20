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

```c
public class MyProduct{
  public int Id{get;set;}
  public string Name{get;set}
}


```
