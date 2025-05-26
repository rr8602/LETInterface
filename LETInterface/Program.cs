using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LETInterface
{
    class Program
    {
        static string server = "127.0.0.1";
        static int port = 2010;
        static HttpClient client = new HttpClient();

        static string Url(string path) => $"http://{server}:{port}{path}";

        static async Task<HttpResponseMessage> TaskAddNew() => await client.PostAsync(Url("/tasks"), null);
        static async Task<HttpResponseMessage> TaskSpecify(string path, object content)
        {
            var json = JsonSerializer.Serialize(content);
            return await client.PutAsync(Url(path), new StringContent(json, Encoding.UTF8, "application/json"));
        }
        static async Task<HttpResponseMessage> TaskGet(string path) => await client.GetAsync(Url(path));
        static async Task<HttpResponseMessage> TaskDeleteAll() => await client.DeleteAsync(Url("/tasks"));

        static async Task TasksProcessTask(object content)
        {
            var response = await TaskAddNew();
            if (response.StatusCode != System.Net.HttpStatusCode.Created)
                throw new Exception("Error: adding new task");

            string location = response.Headers.Location.ToString();

            response = await TaskSpecify(location, content);
            if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
                throw new Exception("Error: specifying task");

            while (true)
            {
                response = await TaskGet(location);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    throw new Exception("Error: inspecting task state");

                var result = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                string state = result.RootElement.GetProperty("state").GetString();

                if (state == "finished") break;
                await Task.Delay(500); // 0.5초 마다 State 확인 요청
            }
        }

        static async Task StartCycle(string uid)
        {
            var req = new { context = "System", activity = "start-cycle", uid = uid };
            await TasksProcessTask(req);
        }

        static async Task VehicleSelection(int? vsn, string vin)
        {
            var req = new { context = "System", activity = "vehicle-selection", vehicle = vsn, identification = vin };
            await TasksProcessTask(req);
        }

        static async Task PerformTests(string line, double? floorpitch)
        {
            var req = new { context = "System", activity = "perform-tests", line = line, floor_pitch = floorpitch };
            await TasksProcessTask(req);
        }

        static async Task EndCycle()
        {
            var req = new { context = "System", activity = "end-cycle" };
            await TasksProcessTask(req);
        }

        static async Task<string> GetResultByUid(string uid)
        {
            var response = await client.GetAsync(Url($"/results_by_uid/{uid}.xml"));
            if (!response.IsSuccessStatusCode) throw new Exception("Error: acquiring results by UID");
            return await response.Content.ReadAsStringAsync();
        }

        static async Task<string> GetLastResult()
        {
            var response = await client.GetAsync(Url("/last_result.xml"));
            if (!response.IsSuccessStatusCode) throw new Exception("Error: acquiring last result");
            return await response.Content.ReadAsStringAsync();
        }

        static async Task Main(string[] args)
        {
            // Args 설정 예: --server 127.0.0.1 --uid UID123 --vsn 1 --vin VIN123 --line LineA --floorpitch -0.5
            foreach (var arg in args)
            {
                if (arg.StartsWith("--server")) server = arg.Split('=')[1];
                if (arg.StartsWith("--port")) port = int.Parse(arg.Split('=')[1]);
            }

            string uid = "UID123"; // 바코드로 받은 UID
            int vsn = 1;
            string vin = "VIN123";
            string line = "LineA";
            double floorpitch = -0.5;

            Console.WriteLine("===Running a cycle===");

            await TaskDeleteAll();
            await StartCycle(uid);
            await VehicleSelection(vsn, vin);
            await PerformTests(line, floorpitch);
            await EndCycle();
            await TaskDeleteAll();

            string result = !string.IsNullOrEmpty(uid) ? await GetResultByUid(uid) : await GetLastResult();
            Console.WriteLine(result);

            Console.WriteLine("===DONE===");
        }
    }

}
