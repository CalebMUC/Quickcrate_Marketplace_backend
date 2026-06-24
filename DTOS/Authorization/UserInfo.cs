using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Minimart_Api.Models;

namespace Minimart_Api.DTOS.Authorization
{
    public class UserInfo
    {
        public int? UserInfoId { get; set; } // Nullable if UserInfoId might be NULL
        public string Name { get; set; }
        public string Email { get; set; }
        public string RefreshToken { get; set; }
        public string phonenumber { get; set; }
        public string Password { get; set; }
        public string RoleID { get; set; }
        public int StatusId { get; set; }

        //public int isAdmin { get; set; }

        //public DateTime CreatedAt { get; set; }
        public virtual LoginResponseStatus Status { get; set; }
    }

}
