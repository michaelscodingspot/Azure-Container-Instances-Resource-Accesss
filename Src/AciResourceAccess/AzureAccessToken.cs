using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace AciResourceAccess
{
    public class AzureAccessToken
    {
        [JsonProperty("access_token")]
        public string AccessTokenStr { get; set; }
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        private readonly DateTime _creationTime;

        public AzureAccessToken()
        {
            _creationTime = DateTime.Now;
        }

        public bool IsExpired
        {
            get
            {
                var expiryTime = _creationTime + TimeSpan.FromSeconds(ExpiresIn);
                var nowWithExtra = DateTime.Now + TimeSpan.FromSeconds(10);
                return expiryTime <= nowWithExtra;
            }
        }

        public override string ToString()
        {
            return AccessTokenStr;
        }
    }
}
