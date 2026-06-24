using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Minimart_Api.DTOS.Authorization
{
    public class UserLogin
    {
        public string EmailorPhone { get; set; }
        public string Password { get; set; }


    }
}
