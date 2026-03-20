using System;
using System.Security.Cryptography;
using System.Text;

namespace CarRental.Utils
{
    public static class OneTimePasswordGenerator
    {
        public static string ComputeY(string username, double x)
        {
            if (string.IsNullOrEmpty(username)) username = "user";
            int a = username.Length;
            double y = Math.Exp(-a * x);
            return y.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);
        }


        public static double SecureRandomDouble(double maxExclusive = 10.0)
        {
            Span<byte> b = stackalloc byte[8];
            RandomNumberGenerator.Fill(b);
            ulong ul = BitConverter.ToUInt64(b) >> 11;
            double fraction = ul / (double)(1UL << 53);
            return fraction * maxExclusive;
        }


        public static string ComputeHash(string plain)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(plain);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
