namespace Minimart_Api.Services.PasswordGenerator
{
    public interface IPasswordGeneratorService
    {
        string GenerateTemporaryPassword(int length = 12);
    }
}
