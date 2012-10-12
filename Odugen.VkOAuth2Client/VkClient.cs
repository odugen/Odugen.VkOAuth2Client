using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using DotNetOpenAuth.AspNet.Clients;
using Newtonsoft.Json;

namespace Odugen.VkOAuth2Client
{
    public sealed class VkClient : OAuth2Client
    {
        #region Constants and Fields

        private readonly string _appId;
        /// <summary>
        /// The _app secret.
        /// </summary>
        private readonly string _appSecret;

        private VkAccessTokenResponse _accessTokenResponse;

        /// <summary>
        /// The authorization endpoint.
        /// </summary>
        private const string AuthorizationEndpoint = "https://oauth.vk.com/authorize";

        /// <summary>
        /// The token endpoint.
        /// </summary>
        private const string TokenEndpoint = "https://oauth.vk.com/access_token";

        /// <summary>
        /// Gets users info
        /// </summary>
        private const string GetUsersEndpoint = "https://api.vk.com/method/users.get";


        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetOpenAuth.AspNet.Clients.FacebookClient"/> class.
        /// </summary>
        /// <param name="appId">
        /// The app id.
        /// </param>
        /// <param name="appSecret">
        /// The app secret.
        /// </param>
        public VkClient(string appId, string appSecret)
            : base("VK")
        {
            _appSecret = appSecret;
            _appId = appId;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The get service login url.
        /// </summary>
        /// <param name="returnUrl">
        /// The return url.
        /// </param>
        /// <returns>An absolute URI.</returns>
        protected override Uri GetServiceLoginUrl(Uri returnUrl)
        {
            var builder = new UriBuilder(AuthorizationEndpoint);
            builder.AppendQueryArgs(
                new Dictionary<string, string> {
					{ "client_id", _appId },
					{ "redirect_uri", returnUrl.AbsoluteUri },
					{ "scope", "" },
					
				});

            return builder.Uri;
        }

        /// <summary>
        /// The get user data.
        /// </summary>
        /// <param name="accessToken">
        /// The access token.
        /// </param>
        /// <returns>A dictionary of profile data.</returns>
        protected override IDictionary<string, string> GetUserData(string accessToken)
        {
            var builder = new UriBuilder(GetUsersEndpoint);
            builder.AppendQueryArgs(
                new Dictionary<string, string> {
					{ "uid", _accessTokenResponse.user_id.ToString(CultureInfo.InvariantCulture) },
					{ "access_token", _accessTokenResponse.access_token }
				});


            using (var client = new WebClient { Encoding = Encoding.UTF8 })
            {
                string data = client.DownloadString(builder.Uri);
                if (string.IsNullOrEmpty(data))
                {
                    return null;
                }
                var response = JsonConvert.DeserializeObject<VkApiResponse>(data);
                if (response != null && response.response.Length == 1)
                {
                    var userData = new Dictionary<string, string>();
                    userData.AddItemIfNotEmpty("id", response.response[0].uid);
                    userData.AddItemIfNotEmpty("name", response.response[0].first_name + " " + response.response[0].last_name);

                    return userData;
                }
            }
            return new Dictionary<string, string>();
        }

        /// <summary>
        /// Obtains an access token given an authorization code and callback URL.
        /// </summary>
        /// <param name="returnUrl">
        /// The return url.
        /// </param>
        /// <param name="authorizationCode">
        /// The authorization code.
        /// </param>
        /// <returns>
        /// The access token.
        /// </returns>
        protected override string QueryAccessToken(Uri returnUrl, string authorizationCode)
        {
            // Note: Facebook doesn't like us to url-encode the redirect_uri value
            var builder = new UriBuilder(TokenEndpoint);
            builder.AppendQueryArgs(
                new Dictionary<string, string> {
					{ "client_id", _appId },
					{ "redirect_uri", NormalizeHexEncoding(returnUrl.AbsoluteUri) },
					{ "client_secret", _appSecret },
					{ "code", authorizationCode },
					{ "scope", "" },
				});

            using (var client = new WebClient())
            {
                string data = client.DownloadString(builder.Uri);
                if (string.IsNullOrEmpty(data))
                {
                    return null;
                }
                _accessTokenResponse = JsonConvert.DeserializeObject<VkAccessTokenResponse>(data);
                if (_accessTokenResponse != null)
                {
                    return _accessTokenResponse.access_token;
                }

                return string.Empty;
            }
        }

        internal class VkResponse
        {
            // ReSharper disable InconsistentNaming
            public string uid { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            // ReSharper restore InconsistentNaming
        }
        internal class VkApiResponse
        {
            // ReSharper disable InconsistentNaming
            public VkResponse[] response { get; set; }
            // ReSharper restore InconsistentNaming
        }
        internal class VkAccessTokenResponse
        {
            // ReSharper disable InconsistentNaming
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public int user_id { get; set; }
            public string error { get; set; }
            public string error_description { get; set; }
            // ReSharper restore InconsistentNaming
        }

        /// <summary>
        /// Converts any % encoded values in the URL to uppercase.
        /// </summary>
        /// <param name="url">The URL string to normalize</param>
        /// <returns>The normalized url</returns>
        /// <example>NormalizeHexEncoding("Login.aspx?ReturnUrl=%2fAccount%2fManage.aspx") returns "Login.aspx?ReturnUrl=%2FAccount%2FManage.aspx"</example>
        /// <remarks>
        /// There is an issue in Facebook whereby it will rejects the redirect_uri value if
        /// the url contains lowercase % encoded values.
        /// </remarks>
        private static string NormalizeHexEncoding(string url)
        {
            var chars = url.ToCharArray();
            for (int i = 0; i < chars.Length - 2; i++)
            {
                if (chars[i] == '%')
                {
                    chars[i + 1] = char.ToUpperInvariant(chars[i + 1]);
                    chars[i + 2] = char.ToUpperInvariant(chars[i + 2]);
                    i += 2;
                }
            }
            return new string(chars);
        }

        #endregion
    }
}
