using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Sorting;
public class SortDescriptor
{
    public string Field { get; set; } = default!;

    public SortDirection Dir { get; set; } = SortDirection.Asc;
}