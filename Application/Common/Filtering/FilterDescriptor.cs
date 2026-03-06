using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Filtering;

public class FilterDescriptor
{
    public string Field { get; set; } = default!;
    public FilterOperator Operator { get; set; }
    public object? Value { get; set; }
    public bool IgnoreCase { get; set; } = true;
}

