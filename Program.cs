using LibUsbDotNet;
using LibUsbDotNet.Main;
using quest_bootloader_unlocker;

namespace Examples;

internal class Program
{

    public static readonly int DEFAULT_VID = 0x2833;
    public static readonly int DEFAULT_PID = 0x81;

    public static Dictionary<string, string>? GetDeviceInfo(FastbootUsbDevice device)
    {
        Dictionary<string, string> info = new Dictionary<string, string>();
        device.WriteS("oem device-info");
        Thread.Sleep(100);
        var data = device.ReadS();
        if (data == null)
            return null;
        foreach (var item in data.Split("\n"))
        {
            var splitted = item.Replace("INFO", "").Split(":");
            if (splitted.Length == 2)
            {
                info.Add(splitted[0].Trim(), splitted[1].Trim());
            }
        }
        if (info.Count == 0)
            return null;
        return info;
    }

    public static void Main(string[] args)
    {
        if (args.Length <= 0)
        {
            var patches = new Patches();
            using (var device = new FastbootUsbDevice(DEFAULT_VID, DEFAULT_PID))
            {
                if (!device.TryConnect())
                {
                    Console.WriteLine("Can't find the Fastboot Device!");
                    return;
                }
                var deviceInfo = GetDeviceInfo(device);
                if (deviceInfo == null)
                {
                    Console.WriteLine("Can't get device info!");
                    return;
                }
                string? buildNumber;
                if (!deviceInfo.TryGetValue("Build number", out buildNumber))
                {
                    Console.WriteLine("Can't get device build number!");
                    return;
                }
                Console.WriteLine($"Build number: {buildNumber}!");
                var selectedVersion = patches.GetVersion(buildNumber);
                if (selectedVersion == null)
                {
                    Console.WriteLine($"Build number: {buildNumber} not available!");
                    return;
                }
                var end = selectedVersion.MaxAddress + 1;
                var buffer = Enumerable.Repeat((byte)0x0C, selectedVersion.Overflow + end).ToArray();
                var firmware = File.ReadAllBytes(selectedVersion.File);
                selectedVersion.ApplyTo(ref firmware);
                Array.Copy(firmware, 0, buffer, selectedVersion.Overflow, end);

                Console.WriteLine("Unlock Device? y/n");
                if (Console.ReadKey(true).Key == ConsoleKey.Y)
                {
                    device.Write(buffer);
                    Thread.Sleep(100);
                    Console.WriteLine(device.ReadS());

                    device.WriteS("flash:unlock_token");
                    Thread.Sleep(100);
                    Console.WriteLine(device.ReadS());

                    device.Close();
                }
            }
            Console.WriteLine("Finished!");
            Console.WriteLine("Press any key to close...");
            Console.ReadKey(true);
        }
        else
        {
            if (args[0].Equals("test"))
            {
                Console.WriteLine("Devices:");
                foreach (UsbRegistry d in UsbDevice.AllDevices)
                {
                    Console.WriteLine($"{d.Name} - VID: {d.Vid.ToString("X4")} PID: {d.Pid.ToString("X4")}");
                }
                Console.WriteLine();
                FastbootUsbDevice device;
                if (args.Length >= 3)
                {
                    device = new FastbootUsbDevice(Convert.ToInt32(args[1], 16), Convert.ToInt32(args[2], 16));
                }
                else
                {
                    device = new FastbootUsbDevice(DEFAULT_VID, DEFAULT_PID);
                }
                if (!device.TryConnect())
                {
                    Console.WriteLine("Can't find the Fastboot Device!");
                    return;
                }
                var deviceInfo = GetDeviceInfo(device);
                if (deviceInfo == null)
                {
                    Console.WriteLine("Can't get device info!");
                    return;
                }
                Console.WriteLine("Device Info:");
                foreach (var info in deviceInfo)
                {
                    Console.WriteLine($"{info.Key}: {info.Value}");
                }
                var buffer = Enumerable.Repeat((byte)0x0C, 0x100000 + 0xFFFFF).ToArray();

                Console.WriteLine("Test Device? y/n");
                if (Console.ReadKey(true).Key == ConsoleKey.Y)
                {
                    device.Write(buffer);
                    Thread.Sleep(100);
                    Console.WriteLine(device.ReadS());

                    device.Close();
                }
                device.Dispose();
                Console.WriteLine("Finished, check for crash!");
                Console.WriteLine("Press any key to close...");
            }

        }
    }

}