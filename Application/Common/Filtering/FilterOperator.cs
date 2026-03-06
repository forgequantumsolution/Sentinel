using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Filtering;

public enum FilterOperator
{
    Eq,
    Neq,
    Gt,
    Gte,
    Lt,
    Lte,
    Contains,
    DoesNotContain,
    EndsWith,
    StartsWith,
    IsNull,
    IsNotNull,
    IsEmpty,
    IsNotEmpty,
    In
}
