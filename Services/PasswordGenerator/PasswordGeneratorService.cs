using System.Text;

namespace Minimart_Api.Services.PasswordGenerator
{
    public class PasswordServiceGenerator : IPasswordGeneratorService
    {
        private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
        private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Numbers = "0123456789";
        private const string SpecialChars = "!@#$%&*";

        public string GenerateTemporaryPassword(int length = 12)
        {
            var allChars = Lowercase + Uppercase + Numbers + SpecialChars;
            var random = new Random();
            var password = new StringBuilder();

            // Ensure at least one character from each category
            password.Append(Lowercase[random.Next(Lowercase.Length)]);
            password.Append(Uppercase[random.Next(Uppercase.Length)]);
            password.Append(Numbers[random.Next(Numbers.Length)]);
            password.Append(SpecialChars[random.Next(SpecialChars.Length)]);

            // Fill the rest randomly
            for (int i = 4; i < length; i++)
            {
                password.Append(allChars[random.Next(allChars.Length)]);
            }

            // Shuffle the password
            return new string(password.ToString().ToCharArray().OrderBy(x => random.Next()).ToArray());
        }
    }
}
