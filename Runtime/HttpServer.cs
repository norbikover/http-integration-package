using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace HttpIntegration
{
    public class HttpServer : MonoBehaviour
    {
        #region Singleton pattern.
        private static HttpServer _instance;
        public static HttpServer Instance
        {
            get
            {
                return _instance ?? (_instance = FindAnyObjectByType<HttpServer>());
            }
        }

        #endregion

        [SerializeField] private int port = 8080;
        [SerializeField] private bool debugLog = true;

        private HttpListener _listener;
        private readonly ConcurrentDictionary<string, Func<HttpListenerRequest, Task<HttpResponse>>> _routes =
            new ConcurrentDictionary<string, Func<HttpListenerRequest, Task<HttpResponse>>>();

        private bool _serverStarted = false;

        public void StartServer()
        {
            if (_serverStarted) return;
            _serverStarted = true;

            string localIp = GetLocalIPAddress();

            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://127.0.0.1:{port}/");
            _listener.Prefixes.Add($"http://localhost:{port}/");
            if (!string.Equals(localIp, "127.0.0.1")) _listener.Prefixes.Add($"http://{GetLocalIPAddress()}:{port}/");
            
            _listener.Start();
            _ = ListenLoop();
            if (debugLog) Debug.Log($"HTTP server running on port {port}. Local ip: {GetLocalIPAddress()}");
        }

        public void Register(string path, Func<HttpListenerRequest, Task<HttpResponse>> handler)
        {
            _routes[path] = handler;
            if (debugLog) Debug.Log($"Registered endpoint {path}");
        }

        public void Unregister(string path) => _routes.TryRemove(path, out _);

        private async Task ListenLoop()
        {
            while (_listener != null && _listener.IsListening)
            {
                try
                {
                    var ctx = await _listener.GetContextAsync();
                    _ = Task.Run(() => Process(ctx));
                }
                catch (Exception e)
                {
                    if (debugLog) Debug.Log($"Listener stopped: {e.Message}");
                }
            }
        }

        private async Task Process(HttpListenerContext ctx)
        {
            var req = ctx.Request;
            var res = ctx.Response;
            HttpResponse reply;

            try
            {
                string path = req.Url.AbsolutePath;
                if (_routes.TryGetValue(path, out var handler))
                    reply = await handler(req);
                else
                    reply = new HttpResponse(404, "Endpoint not found");
            }
            catch (Exception ex)
            {
                reply = new HttpResponse(500, $"Internal error: {ex.Message}");
            }

            byte[] buffer = Encoding.UTF8.GetBytes(reply.ResponseMessage ?? "");
            res.StatusCode = reply.StatusCode;
            res.ContentType = "application/json";
            res.ContentLength64 = buffer.Length;
            await res.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            res.Close();
        }

        private void OnDestroy() 
        {
            if(_listener != null)
            {
                Debug.Log("Destroy Http server");
                _listener.Stop();
                _listener.Close();
                _listener = null;
            }
        }

        // ---------- Helper utilities ----------
        public static async Task<T> ReadJsonAsync<T>(HttpListenerRequest req)
        {
            using var reader = new StreamReader(req.InputStream, req.ContentEncoding);
            string body = await reader.ReadToEndAsync();
            return JsonConvert.DeserializeObject<T>(body);
        }

        private string GetLocalIPAddress()
        {
            foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            return "127.0.0.1";
        }
    }

    [Serializable]
    public class HttpResponse
    {
        public int StatusCode;
        public string ResponseMessage;
        public HttpResponse(int code = 200, string message = "") { StatusCode = code; ResponseMessage = message; }
    }
}