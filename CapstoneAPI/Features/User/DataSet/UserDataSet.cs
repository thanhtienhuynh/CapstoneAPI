using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.User.DataSet
{
    public class UserDataSet
    {
        public int Id { get; set; }
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string AvatarUrl { get; set; }
        public bool IsActive { get; set; }
        public int RoleId { get; set; }
    }

    public class UpdateUserParam
    {
        public int Id { get; set; }
        public bool? IsActive { get; set; }
        public int? Role { get; set; }
    }

    public class AdminUserFilter
    {
        public string Fullname { get; set; }
        public string Email { get; set; }
        public int? Role { get; set; }
        public bool? IsActive { get; set; }
    }
}
