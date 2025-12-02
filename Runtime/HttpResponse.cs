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
    [Serializable]
    public class HttpResponse
    {
        public int StatusCode;
        public string ResponseMessage;
        public HttpResponse(int code = 200, string message = "") { StatusCode = code; ResponseMessage = message; }
    }
}