namespace Minimart_Api.DTOS.Authorization
{
    public class LoginResponseStatus
    {
        //public int ResponseCode { get; set; }
        //public string ResponseMessage { get; set; }

        public int ResponseStatusId { get; set; }
        public bool ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public virtual ICollection<UserInfo> Users { get; set; }
    }
}
