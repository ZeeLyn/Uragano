using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Uragano.Abstractions
{
    public class CachingKeyGenerator : ICachingKeyGenerator
    {
        private ICodec Codec { get; }


        public CachingKeyGenerator(ICodec codec)
        {
            Codec = codec;
        }

        private const string LinkString = ":";

        public string GenerateKeyPlaceholder(string keyPrefix, int globalExpire, string route, MethodInfo methodInfo, CachingAttribute cachingAttribute = default)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(keyPrefix))
                sb.AppendFormat("{0}{1}", keyPrefix, LinkString);
            if (cachingAttribute == null || string.IsNullOrWhiteSpace(cachingAttribute.Key))
            {
                sb.AppendFormat("{0}", route);
                if (methodInfo.GetParameters().Length > 0)
                    sb.Append(LinkString + "{0}");
            }
            else
                sb.Append(cachingAttribute.Key);

            return sb.ToString();
        }

        public string ReplacePlaceholder(string keyPlaceholder, bool customKey, object[] args)
        {
            if (args == null || args.Length <= 0) return keyPlaceholder;
            return customKey ? string.Format(keyPlaceholder, args) : string.Format(keyPlaceholder, MD5(Codec.Serialize(args)));
        }

        private string MD5(byte[] bytes)
        {
            using (MD5 md5 = new MD5CryptoServiceProvider())
            {
                var hash = md5.ComputeHash(bytes);
                md5.Clear();
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}
