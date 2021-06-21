using CapstoneAPI.DataSets.SubjectGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.Major
{
    public class UpdateMajorParam
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int Status { get; set; }
        public CreateMajorSubjectGroup? SubjectGroup { get; set; }
    }
}
