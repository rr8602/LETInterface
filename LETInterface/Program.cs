using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LETInterface
{
    class Program
    {
        static void Main(string[] args)
        {
            string server = "ets.let.fortiddns.com";
            int port = 2010;

            // 바코드로 받은 정보들 (예시 -> 차후에 수정 필요)
            // 바코드에서 정보들이 어떻게 넘어오는지 확인 필요
            string uid = "UID123";
            int vsn = 1;
            string vin = "VIN123";
            string line = "LineA";
            double floorpitch = -0.5;

            var taskService = new TaskService(server, port);
            var controller = new CycleControl(taskService);

            controller.RunCycle(uid, vsn, vin, line, floorpitch);
        }
    }
}
