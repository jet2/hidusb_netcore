using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidSharp;
using HidSharp.Reports;
using HidSharp.Reports.Input;


namespace USBhidNames
{
    internal class Program
    {
        private static DeviceList list;

        public static HidDevice yscanner;

        private static HidDeviceInputReceiver inputReceiver;
        private static DeviceItemInputParser inputParser;
        private static DeviceItem deviceItem;

        private static HidStream hidStream;
        private static ReportDescriptor reportDescriptor;
        static byte[] inputReportBuffer;

        static Report inputReport;
        static int readerState = 0;
        static string dev_path_linux0 = "/sys/devices/pci0000:00/0000:00:06.0/usb1/1-2/1-2:1.0/0003:046E:52C3.0002/hidraw/hidraw1";
        static string dev_path_linux = "/sys/devices/sys/devices/pci0000:00/0000:00:1d.0/usb2/2-1/2-1.2/2-1.2:1.0/0003:046E:52C3.0002/hidraw/hidraw1";
        static string dev_path_win = @"\\?\hid#vid_046e&pid_52c3&col02#8&14b012ba&3&0001#{4d1e55b2-f16f-11cf-88cb-001111000030}";
        static string papath="";

        static void Main(string[] args)
        {
            list = DeviceList.Local;
            //list.Changed += DevicePlugHandler;
            //DeviceFilter DeviceFilterDelegateInstance = new DeviceFilter(DeviceFilterDelegate);
            var hidDeviceList = list.GetAllDevices().ToArray();
            foreach (var hidDevice in hidDeviceList)
            {
                Console.WriteLine(hidDevice.DevicePath);
            }
            Console.WriteLine("Start");
            ReaderInit();
            //Console.ReadLine();

        }

        private static async void ReaderInit()
        {
            list = DeviceList.Local;
            list.Changed += DevicePlugHandler;
            DeviceFilter DeviceFilterDelegateInstance = new DeviceFilter(DeviceFilterDelegate);
            var hidDeviceList = list.GetAllDevices(DeviceFilterDelegateInstance).ToArray();
            await handleListChanged(hidDeviceList);
        }


        private static async Task handleListChanged(Device[] hidDeviceList)
        {
            if (hidDeviceList.Count() > 0)
            {
                //Console.WriteLine($"Connected");

                yscanner = (HidDevice)hidDeviceList[0];
                string ppp = yscanner.DevicePath;
                try
                {
                    yscanner.Open();
                }catch (Exception ex)
                {
                    Console.WriteLine($"ПРОБЛЕМА\n{ex.StackTrace}");
                    

                }
                
                if (yscanner.TryOpen(out hidStream))
                {
                    Console.WriteLine($"Connected {yscanner.DevicePath}");
                    readerState = 1;
                    Console.WriteLine($"{{\"isstate\":1,\"statevalue\":1, \"card\":\"\"}}");
                    reportDescriptor = yscanner.GetReportDescriptor();
                    inputReport = reportDescriptor.FeatureReports.FirstOrDefault();
                    inputReportBuffer = new byte[yscanner.GetMaxInputReportLength()];
                    inputParser = inputReport.DeviceItem.CreateDeviceItemInputParser();

                    inputReceiver = reportDescriptor.CreateHidDeviceInputReceiver();
                    inputReceiver.Start(hidStream);
                    inputReceiver.Received -= ReportHandler;
                    inputReceiver.Received += ReportHandler;
                }
                else { Console.WriteLine($"Still Closed"); }

            }
            else
            {
                readerState = 0;
                Console.WriteLine($"{{\"isstate\":1,\"statevalue\":0, \"card\":\"\"}}");
                Console.WriteLine($"Disconnected {papath}");
            }

        }

        private static async void ReportHandler(object sender, EventArgs e)
        {
            if (inputReceiver.TryRead(inputReportBuffer, 0, out inputReport))
            {

                // Parse the report if possible.
                // This will return false if (for example) the report applies to a different DeviceItem.
                if (inputParser.TryParseReport(inputReportBuffer, 0, inputReport))
                {
                    string finalString = Encoding.ASCII.GetString(inputReportBuffer).TrimEnd('\0');
                    if (inputReportBuffer[2] != 0)
                    {
                        //Console.WriteLine($"Report {finalString[1..]}={finalString.Length}");
                        //public int isstate;
                        //public int statevalue;
                        //public string card;
                        finalString = finalString[1..];
                        Console.WriteLine($"{{\"isstate\":0,\"statevalue\":0, \"card\":\"{finalString}\"}}");
                        //await _hub.Clients.All.SendAsync("readerreport", finalString[1..]);
                    }
                }
            }
        }
        private static async void DevicePlugHandler(object sender, EventArgs e)
        {
            DeviceFilter DeviceFilterDelegateInstance = new DeviceFilter(DeviceFilterDelegate);
            var hidDeviceList = list.GetAllDevices(DeviceFilterDelegateInstance).ToArray();
            // Console.WriteLine(hidDeviceList.Count()>0?"Connected":"DisConnected");
            await handleListChanged(hidDeviceList);
        }


        public static  bool DeviceFilterDelegate(Device device)
        {
            bool result = false;

            var xarr= File.ReadAllLines("device.txt");
            papath = "";
            if (xarr.Length > 0)
            {
                papath = xarr[0];
            }
            /*
            papath = dev_path_win;
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                papath = dev_path_linux;
            }
            */
            if (device.DevicePath == papath)
            {
                result = true;
            }
            return result;
        }
    }






}
