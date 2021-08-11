using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Test.DataSet
{
    public class TestParam
    {
        public int SubjectGroupId { get; set; }
    }

    public class SetSuggestedTestParam
    {
        public int TestId { get; set; }
        public bool IsSuggestTest { get; set; }
    }
}
