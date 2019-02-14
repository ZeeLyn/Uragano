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

        public CachingConfig GenerateKeyPlaceholder(ICachingOptions cachingOptions, string route, MethodInfo methodInfo)
        {
            var noMethodCache = methodInfo.GetCustomAttribute<NonCachingAttribute>();
            var noServiceCache = methodInfo.DeclaringType?.GetCustomAttribute<NonCachingAttribute>();
            if (noMethodCache != null || noServiceCache != null)
                return new CachingConfig();

            var attr = methodInfo.GetCustomAttribute<CachingAttribute>();
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(cachingOptions.KeyPrefix))
                sb.AppendFormat("{0}{1}", cachingOptions.KeyPrefix, LinkString);
            if (attr == null || string.IsNullOrWhiteSpace(attr.Key))
            {
                sb.AppendFormat("{0}", route);
                if (methodInfo.GetParameters().Length > 0)
                    sb.Append(LinkString + "{0}");

            }
            else
                sb.Append(attr.Key);

            return new CachingConfig
            {
                Enable = true,
                Key = sb.ToString(),
                CustomKey = attr != null && !string.IsNullOrWhiteSpace(attr.Key),
                Expire = attr?.Expire != null ? TimeSpan.FromSeconds(attr.Expire.Value) : cachingOptions.Expire
            };
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
