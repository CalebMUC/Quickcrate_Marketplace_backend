using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Minimart_Api.DTOS.Authorization
{
    public class RefreshTokenRequest
    {
        public string UserID { get; set; }
    }
}
