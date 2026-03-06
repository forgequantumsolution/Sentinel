using Application.Common.Filtering;
using Application.Common.Sorting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs;

public class BaseDTO
{
    public CompositeFilterDescriptor? Filter { get; set; }

    public List<SortDescriptor>? Sort { get; set; }

    public bool isPaginated { get; set; } = true;

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public int Skip => (Page - 1) * PageSize;
}
