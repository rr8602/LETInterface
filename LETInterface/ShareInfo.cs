using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LETInterface
{

    // uid, vsn, vin, line, floorpitch 값들을 넘기고
    // xml에서 Inclination_X_initial, Inclination_X_final, Lamp의 beam_type, side값 넘기기
    public class ShareInfo
    {
        private string uid;
        private int vsn;
        private string vin;
        private string line;
        private double floorpitch;

        private List<LampInclinationData> lampInclinationList = new List<LampInclinationData>();
        
        private string server = "ets.let.fortiddns.com";
        private int port = 2010;

        public ShareInfo(string uid, int vsn, string vin, string line, double floorpitch)
        {
            this.uid = uid;
            this.vsn = vsn;
            this.vin = vin;
            this.line = line;
            this.floorpitch = floorpitch;
        }

        public string CycleParameter(string uid, int vsn, string vin, string line, double floorpitch)
        {
            var taskService = new TaskService(server, port);
            var controller = new CycleControl(taskService);

            string result = controller.RunCycle(uid, vsn, vin, line, floorpitch);

            return result;
        }

        public void resultXmlData()
        {
            lampInclinationList.Clear();

            string result = CycleParameter(uid, vsn, vin, line, floorpitch);

            XDocument doc = XDocument.Parse(result);

            foreach (var lamp in doc.Descendants("Lamp"))
            {
                var data = new LampInclinationData
                {
                    beamType = (string)lamp.Attribute("beam_type"),
                    side = (string)lamp.Attribute("side"),
                    inclinationXInitial = (string)lamp.Element("Inclination_X_initial"),
                    inclinationYInitial = (string)lamp.Element("Inclination_Y_initial"),
                    inclinationXFinal = (string)lamp.Element("Inclination_X_final"),
                    inclinationYFinal = (string)lamp.Element("Inclination_Y_final")
                };

                lampInclinationList.Add(data);
            }

            Console.WriteLine(lampInclinationList);
        }

        private class LampInclinationData
        {
            public string beamType;
            public string side;
            public string inclinationXInitial;
            public string inclinationYInitial;
            public string inclinationXFinal;
            public string inclinationYFinal;
        }
    }
}
