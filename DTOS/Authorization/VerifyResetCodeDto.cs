namespace Minimart_Api.DTOS.Authorization
{
    public class VerifyResetCodeDto
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }
}
