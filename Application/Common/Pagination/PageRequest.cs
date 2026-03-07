using System;
using System.Collections.Generic;
using Application.Common.Filtering;
using Application.Common.Sorting;
using Application.Interfaces;

namespace Application.Common.Pagination;

public class PageRequest
{
    private const int MaxPageSize = 100;

    public int Page { get; set; } = 1;

    private int _pageSize = 20;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    public int Skip => (Page - 1) * PageSize;

    public List<SortDescriptor>? Sorts { get; set; }

    public IFilterDescriptor? Filter { get; set; }
}
