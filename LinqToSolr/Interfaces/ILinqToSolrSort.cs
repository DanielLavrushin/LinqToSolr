﻿using LinqToSolr.Models;

namespace LinqToSolr.Interfaces
{

    public interface ILinqToSolrSort
    {

        string Field { get; set; }
        SolrSortTypes Order { get; set; }

    }
}
