using System;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace HttpIntegration
{
    public class HttpService : MonoBehaviour
    {
        #region Singleton pattern.
        private static HttpService _instance;
        public static HttpService Instance
        {
            get
            {
                return _instance ?? (_instance = FindAnyObjectByType<HttpService>());
            }
        }

        #endregion

        private const string SERVER_URL = "http://localhost:3000/api";

        public async Task<string> SendRequestAsync(string postfix, HttpMethod method, string serverUrl = SERVER_URL)
        {
            return await SendRequestAsync<object>(postfix, method, default, serverUrl);
        }

        public async Task<string> SendRequestAsync<T>(string postfix, HttpMethod method, T data = default, string serverUrl = SERVER_URL)
        {
            string url = serverUrl + postfix;

            using (UnityWebRequest request = CreateRequest(url, method, data))
            {
                request.downloadHandler = new DownloadHandlerBuffer();

                // Send the request and await completion.
                await request.SendWebRequestAsync(); // This is a custom method defined in Extensions.

                if (request.result == UnityWebRequest.Result.Success)
                {
                    return request.downloadHandler.text;
                }
                else if (!string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    throw new Exception(request.downloadHandler.text);
                }
                else
                    throw new Exception(request.error);
            }
        }

        private UnityWebRequest CreateRequest<T>(string url, HttpMethod method, T data = default)
        {
            UnityWebRequest request;

            switch (method)
            {
                case HttpMethod.GET:
                    request = UnityWebRequest.Get(url);
                    break;
                case HttpMethod.POST:
                    request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
                    break;
                case HttpMethod.PUT:
                    request = UnityWebRequest.Put(url, string.Empty);
                    break;
                case HttpMethod.DELETE:
                    request = UnityWebRequest.Delete(url);
                    break;
                default:
                    throw new ArgumentException("Unsupported HTTP method");
            }

            if ((method == HttpMethod.POST || method == HttpMethod.PUT) && data != null)
            {
                string json = JsonConvert.SerializeObject(data);
                byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.SetRequestHeader("Content-Type", "application/json");
            }

            return request;
        }
    }
}
