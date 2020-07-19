using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KafkaAdapter.Components
{
    public static class Extensions
    {
        public static string GetHash(this string key)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
            {
                var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(key));

                return hash.Select(s => s.ToString("X2")).Aggregate((s1, s2) => s1 + s2);
            }
        }
    }
}
