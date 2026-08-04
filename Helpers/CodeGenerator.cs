namespace PastebinSharer.Helpers
{
    public static class CodeGenerator
    {
        private static readonly Random _random = new Random();
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public static string GenerateCode(int length = 6)
        {
            char[] stringChars = new char[length];
            for (int i = 0; i < length; i++)
            {
                stringChars[i] = Chars[_random.Next(Chars.Length)];
            }
            return new string(stringChars);
        }
    }
}