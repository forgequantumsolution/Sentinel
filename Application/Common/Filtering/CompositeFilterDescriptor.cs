using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Filtering;

public class CompositeFilterDescriptor
{
    public string Logic { get; set; } = "and";
    public List<object> Filters { get; set; } = new();
}