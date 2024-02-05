using LibUsbDotNet;
using LibUsbDotNet.LibUsb;
using LibUsbDotNet.Main;
using quest_bootloader_unlocker;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Examples;

internal class Program
{

    public static Dictionary<string, string>? GetDeviceInfo(QuestUsbDevice device)
    {
        Dictionary<string, string> info = new Dictionary<string, string>();
        device.WriteS("oem device-info");
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
        var patches = new Patches();
        using (var device = new QuestUsbDevice()) { 
            if (!device.TryConnect())
            {
                Console.WriteLine("Can't find the Quest Device!");
                return;
            }
            var deviceInfo = GetDeviceInfo(device);
            if(deviceInfo == null)
            {
                Console.WriteLine("Can't get device info!");
                return;
            }
            string? buildNumber;
            if(!deviceInfo.TryGetValue("Build number", out buildNumber))
            {
                Console.WriteLine("Can't get device build number!");
                return;
            }
            Console.WriteLine("Build number: " + buildNumber + "!");
            var selectedVersion = patches.GetVersion(buildNumber);
            if (selectedVersion == null)
            {
                Console.WriteLine("Build number: " + buildNumber + " not available!");
                return;
            }
            var end = selectedVersion.MaxAddress + 1;
            var buffer = Enumerable.Repeat((byte)0x0C, selectedVersion.Overflow + end).ToArray();
            var firmware = File.ReadAllBytes(selectedVersion.File);
            selectedVersion.ApplyTo(ref firmware);
            Array.Copy(firmware, 0, buffer, selectedVersion.Overflow, end);

            Console.WriteLine("Unlock Device? y/n");
            if(Console.ReadKey(true).Key == ConsoleKey.Y) { 
                device.Write(buffer);
                Console.WriteLine(device.ReadS());

                device.WriteS("flash:unlock_token");
                Console.WriteLine(device.ReadS());

                device.Close();
            }
        }
        Console.WriteLine("Finished!\nPress any key to close...");
        Console.ReadKey(true);
    }

}