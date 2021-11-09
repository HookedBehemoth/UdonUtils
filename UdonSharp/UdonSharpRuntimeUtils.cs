using System;
using System.Security.Cryptography;
using System.Text;

namespace UdonSharp {
    namespace Internal {
        public static class UdonSharpInternalUtility
        {
            public static long GetTypeID(System.Type type)
            {
                SHA256 typeHash = new SHA256CryptoServiceProvider();
                byte[] hash = typeHash.ComputeHash(Encoding.UTF8.GetBytes(type.FullName));
                return BitConverter.ToInt64(hash, 0);
            }

            public static string GetTypeName(System.Type type)
            {
                return type.Name;
            }
        }
    }
}