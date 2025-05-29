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

        public async Task<HttpResponseMessage> TaskAddNew()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, Url("/tasks"));
            Logger.WriteLog("client -> server :\n" + Logger.HttpRequestToLogString(request), true); // 로그 기록

            var response = await _client.SendAsync(request);
            Logger.WriteLog("server -> client :\n" + Logger.HttpResponseToLogString(response), true); // 응답 로그

            return response;

            //return await _client.PostAsync(Url("/tasks"), null);
        }
        public async Task<HttpResponseMessage> TaskSpecify(string path, object content)
        {
            var json = JsonSerializer.Serialize(content);
            return await _client.PutAsync(Url(path), new StringContent(json, Encoding.UTF8, "application/json"));
        }
        public async Task<HttpResponseMessage> TaskGet(string path) => await _client.GetAsync(Url(path));
        public async Task<HttpResponseMessage> TaskDeleteAll() => await _client.DeleteAsync(Url("/tasks"));

        public async Task<string> GetResultByUid(string uid)
        {
            var response = await _client.GetAsync(Url($"/results_by_uid/{uid}.xml"));
            if (!response.IsSuccessStatusCode) throw new Exception("Error: acquiring results by UID");
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetLastResult()
        {
            var response = await _client.GetAsync(Url("/last_result.xml"));
            if (!response.IsSuccessStatusCode) throw new Exception("Error: acquiring last result");
            return await response.Content.ReadAsStringAsync();
        }
    }
}
