using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LETInterface
{
    public class TaskService
    {
        private readonly string _server;
        private readonly int _port;
        private readonly HttpClient _client;

        public TaskService(string server, int port)
        {
            _server = server;
            _port = port;
            _client = new HttpClient();
        }

        public string Url(string path) => $"http://{_server}:{_port}{path}";

        public HttpResponseMessage TaskAddNew()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, Url("/tasks"));
            string reqLog = Logger.HttpMessageToLogString(request);
            Logger.WriteLog(reqLog, true); // 로그 기록

            var response = _client.SendAsync(request).Result;
            string respLog = Logger.HttpMessageToLogString(response);
            Logger.WriteLog(respLog, true); // 응답 로그

            return response;
        }

        public HttpResponseMessage TaskSpecify(string path, object content)
        {
            var json = JsonSerializer.Serialize(content);

            var request = new HttpRequestMessage(HttpMethod.Put, Url(path));
            string reqLog = Logger.HttpMessageToLogString(request);
            Logger.WriteLog(reqLog, true);

            var response = _client.PutAsync(Url(path), new StringContent(json, Encoding.UTF8, "application/json")).Result;
            string respLog = Logger.HttpMessageToLogString(response);
            Logger.WriteLog(respLog, true);

            return response;
        }
        public HttpResponseMessage TaskGet(string path)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Url(path));
            string reqLog = Logger.HttpMessageToLogString(request);
            Logger.WriteLog(reqLog, true);

            var response = _client.SendAsync(request).Result;
            string respLog = Logger.HttpMessageToLogString(response);
            Logger.WriteLog(respLog, true);

            return response;
        }

        public HttpResponseMessage TaskDeleteAll()
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, Url("/tasks"));
            string reqLog = Logger.HttpMessageToLogString(request);
            Logger.WriteLog(reqLog, true);

            var response = _client.SendAsync(request).Result;
            string respLog = Logger.HttpMessageToLogString(response);
            Logger.WriteLog(respLog, true);

            return response;
        }

        public string GetResultByUid(string uid)
        {
            var response = _client.GetAsync(Url($"/results_by_uid/{uid}.xml")).Result;
            if (!response.IsSuccessStatusCode) throw new Exception("Error: acquiring results by UID");
            return response.Content.ReadAsStringAsync().Result;
        }

        public string GetLastResult()
        {
            var response = _client.GetAsync(Url("/last_result.xml")).Result;
            if (!response.IsSuccessStatusCode) throw new Exception("Error: acquiring last result");
            return response.Content.ReadAsStringAsync().Result;
        }
    }
}
