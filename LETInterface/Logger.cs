using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LETInterface
{
    public class Logger
    {
        private static string filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LETInterface_LOG.txt");
        private static Queue<string> logQueue = new Queue<string>();
        private static readonly object writeLock = new object();

        public static void WriteLog(string str, bool bwritenow = false)
        {
            Debug.WriteLine(str);
            logQueue.Enqueue(str);

            if (bwritenow == true)
            {
                lock (writeLock)
                {
                    using (StreamWriter fw = new StreamWriter(filename, true))
                    {
                        while (logQueue.Count > 0)
                        {
                            string logEntry = logQueue.Dequeue();
                            fw.WriteLine(logEntry);
                        }
                    }
                }
            }
        }

        public static string HttpRequestToLogString(HttpRequestMessage request)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"{request.Method} {request.RequestUri.PathAndQuery} HTTP/{request.Version}");

            // Host Header
            sb.AppendLine($"Host: {request.RequestUri.Host}{(request.RequestUri.Port != 80 && request.RequestUri.Port != 443 ? ":" + request.RequestUri.Port : "")}");

            // 기타 Header
            foreach (var header in request.Headers)
            {
                sb.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }
            if (request.Content != null)
            {
                foreach (var header in request.Content.Headers)
                {
                    sb.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
                }
            }
            return sb.ToString();
        }

        public static string HttpResponseToLogString(HttpResponseMessage response)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"HTTP/{response.Version} {(int)response.StatusCode} {response.ReasonPhrase}");

            // Header
            foreach (var header in response.Headers)
            {
                sb.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }
            if (response.Content != null)
            {
                foreach (var header in response.Content.Headers)
                {
                    sb.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
                }
            }
            return sb.ToString();
        }

    }
}
