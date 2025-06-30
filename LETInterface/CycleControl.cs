using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
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

        public void StartCycle(string uid)
        {
            var req = new { context = "System", activity = "start-cycle", uid = uid };
            TasksProcessTask(req);
        }

        public void VehicleSelection(int? vsn, string vin)
        {
            var req = new { context = "System", activity = "vehicle-selection", vehicle = vsn, identification = vin };
            TasksProcessTask(req);
        }

        public void PerformTests(string line, double? floorpitch)
        {
            var req = new { context = "System", activity = "perform-tests", line = line, floor_pitch = floorpitch };
            TasksProcessTask(req);
        }

        public void EndCycle()
        {
            var req = new { context = "System", activity = "end-cycle" };
            TasksProcessTask(req);
        }

        public void TasksProcessTask(object content)
        {
            var response = _service.TaskAddNew();

            if (response.StatusCode != System.Net.HttpStatusCode.Created)
                throw new Exception("Error: adding new task");

            string location = response.Headers.Location.ToString();

            response = _service.TaskSpecify(location, content);

            if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
                throw new Exception("Error: specifying task");

            while (true)
            {
                response = _service.TaskGet(location);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    throw new Exception("Error: inspecting task state");

                var result = JsonDocument.Parse(response.Content.ReadAsStringAsync().Result);
                string state = result.RootElement.GetProperty("state").GetString();

                if (state == "finished") break;

                Thread.Sleep(500); // 0.5초 마다 state 확인 요청
            }
        }

        public string RunCycle(string uid, int vsn, string vin, string line, double floorpitch)
        {
            // 아래 메서드 하나당 TaskService 클래스의 주기를 한 번 돌아야 함
            _service.TaskDeleteAll();
            StartCycle(uid);
            VehicleSelection(vsn, vin);
            PerformTests(line, floorpitch);
            EndCycle();
            _service.TaskDeleteAll();

            string result = !string.IsNullOrEmpty(uid) ?
                _service.GetResultByUid(uid) :
                _service.GetLastResult();

            Console.WriteLine(result);

            return result;
        }
    }
}
