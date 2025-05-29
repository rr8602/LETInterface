using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LETInterface
{
    public class CycleControl
    {
        private readonly TaskService _service;

        public CycleControl(TaskService service)
        {
            _service = service;
        }

        public async Task StartCycle(string uid)
        {
            var req = new { context = "System", activity = "start-cycle", uid = uid };
            await TasksProcessTask(req);
        }

        public async Task VehicleSelection(int? vsn, string vin)
        {
            var req = new { context = "System", activity = "vehicle-selection", vehicle = vsn, identification = vin };
            await TasksProcessTask(req);
        }

        public async Task PerformTests(string line, double? floorpitch)
        {
            var req = new { context = "System", activity = "perform-tests", line = line, floor_pitch = floorpitch };
            await TasksProcessTask(req);
        }

        public async Task EndCycle()
        {
            var req = new { context = "System", activity = "end-cycle" };
            await TasksProcessTask(req);
        }

        public async Task TasksProcessTask(object content)
        {
            var response = await _service.TaskAddNew();

            if (response.StatusCode != System.Net.HttpStatusCode.Created)
                throw new Exception("Error: adding new task");

            string location = response.Headers.Location.ToString();

            response = await _service.TaskSpecify(location, content);

            if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
                throw new Exception("Error: specifying task");

            while (true)
            {
                response = await _service.TaskGet(location);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    throw new Exception("Error: inspecting task state");

                var result = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                string state = result.RootElement.GetProperty("state").GetString();

                if (state == "finished") break;

                await Task.Delay(500); // 0.5초 마다 state 확인 요청
            }
        }

        public async Task RunCycle(string uid, int vsn, string vin, string line, double floorpitch)
        {
            Console.WriteLine("===Running a cycle===");

            await _service.TaskDeleteAll();
            await StartCycle(uid);
            await VehicleSelection(vsn, vin);
            await PerformTests(line, floorpitch);
            await EndCycle();
            await _service.TaskDeleteAll();

            string result = !string.IsNullOrEmpty(uid) ?
                await _service.GetResultByUid(uid) :
                await _service.GetLastResult();

            Console.WriteLine(result);
            Console.WriteLine("===DONE===");
        }
    }
}
