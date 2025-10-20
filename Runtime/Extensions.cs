using System;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace HttpIntegration
{
    public static class Extensions
    {
        public static Task<UnityWebRequest> SendWebRequestAsync(this UnityWebRequest request)
        {
            var tcs = new TaskCompletionSource<UnityWebRequest>();

            request.SendWebRequest().completed += operation =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    tcs.SetResult(request);
                }
                else if (!string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    tcs.SetException(new Exception(request.downloadHandler.text));
                }
                else
                {
                    tcs.SetException(new Exception(request.error));
                }
            };

            return tcs.Task;
        }
    }
}
