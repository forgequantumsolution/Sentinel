using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Filtering;

public class CompositeFilterDescriptor : IFilterDescriptor
{
    public string Logic { get; set; } = "and";
    public List<IFilterDescriptor> Filters { get; set; } = new();
}