using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace Logic
{
    public static class Extensions
    {
        public static byte[] MD5Hash(this Stream stream)
        {
            using (MD5 md5 = MD5.Create())
            {
                return md5.ComputeHash(stream);
            }
        }

        public static byte[] MD5Hash(this string path)
        {
            using (FileStream stream = File.OpenRead(path))
            {
                return stream.MD5Hash();
            }
        }

        public static bool IsEqualTo(this byte[] a1, byte[] a2)
        {
            return StructuralComparisons.StructuralEqualityComparer.Equals(a1, a2);
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        public static string Remove(this string source, string text, StringComparison comparer = StringComparison.InvariantCultureIgnoreCase)
        {
            return source.Remove(source.IndexOf(text, comparer), text.Length);
        }
    }
}