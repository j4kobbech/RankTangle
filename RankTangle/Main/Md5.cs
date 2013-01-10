namespace RankTangle.Main
{
    using System;
    using System.Text;
    
    public static class Md5
    {
        // Calculates a MD5 hash from the given string and uses the given encoding.
        public static string CalculateMd5(string input, Encoding useEncoding)
        {
            var cryptoService = new System.Security.Cryptography.MD5CryptoServiceProvider();
 
            var inputBytes = useEncoding.GetBytes(input);
            inputBytes = cryptoService.ComputeHash(inputBytes);
            return BitConverter.ToString(inputBytes).Replace("-", string.Empty);
        }

        public static string CalculateMd5(string input)
        {
            return CalculateMd5(input, Encoding.Default);
        }

        public static string GetGravatarEmailHash(string email)
        {
            var gravatarUrl = "http://www.gravatar.com/avatar/" + CalculateMd5(email.ToLower().Trim(), Encoding.Default);
            return gravatarUrl.ToLower();
        }
    }
}