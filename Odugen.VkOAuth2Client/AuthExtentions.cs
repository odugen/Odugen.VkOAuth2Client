using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Odugen.VkOAuth2Client
{
    /// <summary>
    /// From DotNetOpenAuth.Messaging.MessagingUtilities
    /// </summary>
    public static class AuthExtentions
    {
        public static void AppendQueryArgs(this UriBuilder builder, IEnumerable<KeyValuePair<string, string>> args)
        {
            if (args != null && args.Any())
            {
                var sb = new StringBuilder(50 + (args.Count() * 10));
                if (!String.IsNullOrEmpty(builder.Query))
                {
                    sb.Append(builder.Query.Substring(1));
                    sb.Append('&');
                }
                sb.Append(CreateQueryString(args));

                builder.Query = sb.ToString();
            }
        }


        /// <summary>
        /// Concatenates a list of name-value pairs as key=value&amp;key=value,
        /// taking care to properly encode each key and value for URL
        /// transmission according to RFC 3986.  No ? is prefixed to the string.
        /// </summary>
        /// <param name="args">The dictionary of key/values to read from.</param>
        /// <returns>The formulated querystring style string.</returns>
        internal static string CreateQueryString(IEnumerable<KeyValuePair<string, string>> args)
        {
            //Requires.NotNull(args, "args");
            //Contract.Ensures(Contract.Result<string>() != null);

            if (!args.Any())
            {
                return String.Empty;
            }
            var sb = new StringBuilder(args.Count() * 10);

            foreach (var p in args)
            {
                //ErrorUtilities.VerifyArgument(!string.IsNullOrEmpty(p.Key), MessagingStrings.UnexpectedNullOrEmptyKey);
                //ErrorUtilities.VerifyArgument(p.Value != null, MessagingStrings.UnexpectedNullValue, p.Key);
                sb.Append(EscapeUriDataStringRfc3986(p.Key));
                sb.Append('=');
                sb.Append(EscapeUriDataStringRfc3986(p.Value));
                sb.Append('&');
            }
            sb.Length--; // remove trailing &

            return sb.ToString();
        }

        /// <summary>
        /// Adds a key/value pair to the specified dictionary if the value is not null or empty.
        /// </summary>
        /// <param name="dictionary">
        /// The dictionary. 
        /// </param>
        /// <param name="key">
        /// The key. 
        /// </param>
        /// <param name="value">
        /// The value. 
        /// </param>
        public static void AddItemIfNotEmpty(this IDictionary<string, string> dictionary, string key, string value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (!string.IsNullOrEmpty(value))
            {
                dictionary[key] = value;
            }
        }
        internal static string EscapeUriDataStringRfc3986(string value)
        {
            //Requires.NotNull(value, "value");

            // Start with RFC 2396 escaping by calling the .NET method to do the work.
            // This MAY sometimes exhibit RFC 3986 behavior (according to the documentation).
            // If it does, the escaping we do that follows it will be a no-op since the
            // characters we search for to replace can't possibly exist in the string.
            var escaped = new StringBuilder(Uri.EscapeDataString(value));

            // Upgrade the escaping to RFC 3986, if necessary.
            foreach (string t in UriRfc3986CharsToEscape)
            {
                escaped.Replace(t, Uri.HexEscape(t[0]));
            }

            // Return the fully-RFC3986-escaped string.
            return escaped.ToString();
        }

        /// <summary>
        /// The set of characters that are unreserved in RFC 2396 but are NOT unreserved in RFC 3986.
        /// </summary>
        private static readonly string[] UriRfc3986CharsToEscape = new[] { "!", "*", "'", "(", ")" };
    }
}