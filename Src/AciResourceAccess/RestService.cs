using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AciResourceAccess
{
    public class RestService
    {
        
        public async Task<T> SendHttpGetRequest<T>(string url, string bearerToken, params KeyValuePair<string, object>[] queryParams)
        {
            url += GetQuery(queryParams);
            using (var client = new HttpClient())
            {
                if (bearerToken != null)
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + bearerToken);
                }

                var response = await client.GetAsync(url, HttpCompletionOption.ResponseContentRead);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(result);
            }
        }

        public async Task<string> SendHttpDeleteRequest(string url, string bearerToken, params KeyValuePair<string, object>[] queryParams)
        {
            url += GetQuery(queryParams);
            using (var client = new HttpClient())
            {
                if (bearerToken != null)
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + bearerToken);
                }

                var response = await client.DeleteAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        //public async Task<TResult> SendHttpPostRequest<TParam, TResult>(string url, TParam payload, bool useBearer, params KeyValuePair<string, object>[] queryParams) where TParam : class
        //{
        //    var bearerToken = useBearer ? await GetAuthBearerTokenFromIdentityServer() : null;
        //    return await SendHttpPostRequest<TParam, TResult>(url, payload, bearerToken, queryParams);
        //}

        //public async Task<TResult> SendHttpPutRequest<TParam, TResult>(string url, TParam payload, bool useBearer, params KeyValuePair<string, object>[] queryParams) where TParam : class
        //{
        //    var bearerToken = useBearer ? await GetAuthBearerTokenFromIdentityServer() : null;
        //    return await SendHttpPutRequest<TParam, TResult>(url, payload, bearerToken, queryParams);
        //}

        public async Task<TResult> SendHttpPostRequest<TParam, TResult>(string url, TParam payload, string bearerToken, params KeyValuePair<string, object>[] queryParams) where TParam : class
        {
            return await SendHttpRequest<TParam, TResult>((client, address, content) => client.PostAsync(address, content), url, payload, bearerToken, queryParams);
        }

        public async Task<TResult> SendHttpPutRequest<TParam, TResult>(string url, TParam payload, string bearerToken, params KeyValuePair<string, object>[] queryParams) where TParam : class
        {
            return await SendHttpRequest<TParam, TResult>((client, address, content) => client.PutAsync(address, content), url, payload, bearerToken, queryParams);
        }

        public async Task<TResult> SendHttpPostRequestWithXWWWFormEncoding<TResult>(string url, IEnumerable<KeyValuePair<string, string>> postData)
        {
            using (var httpClient = new HttpClient())
            {
                using (var content = new FormUrlEncodedContent(postData))
                {
                    content.Headers.Clear();
                    content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                    HttpResponseMessage response = await httpClient.PostAsync(url, content);

                    var responseText = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<TResult>(responseText);
                }
            }
        }

        private async Task<TResult> SendHttpRequest<TParam, TResult>(Func<HttpClient, string, HttpContent, Task<HttpResponseMessage>> method,
                                                                     string url,
                                                                     TParam payload,
                                                                     string bearerToken,
                                                                     params KeyValuePair<string, object>[] queryParams) where TParam : class
        {
            url += GetQuery(queryParams);

            HttpContent content = null;
            if (payload != null)
            {

                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented,
                    DateFormatString = "yyyy-MM-ddTHH:mm:ss"
                };

                var request = JsonConvert.SerializeObject(payload, jsonSerializerSettings);
                content = new StringContent(request, Encoding.UTF8, "application/json");
            }

            using (var client = new HttpClient())
            {
                if (bearerToken != null)
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + bearerToken);
                }

                HttpResponseMessage response = await method(client, url, content);
                response.EnsureSuccessStatusCode();
                string responseText = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TResult>(responseText);
            }
        }

        private static string GetQuery(KeyValuePair<string, object>[] queryParams)
        {
            if (queryParams == null || queryParams.Length == 0)
            {
                return null;
            }

            var qp = queryParams.Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value == null ? string.Empty : WebUtility.UrlEncode(kvp.Value.ToString())));
            return "?" + string.Join("&", qp.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        }

    }
}
