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

    public static void Main(string[] args)
    {
        var patches = new Patches();
        using (var device = new QuestUsbDevice()) { 
            if (!device.TryConnect())
            {
                Console.WriteLine("Can't find the Quest Device!");
                return;
            }
            var selectedVersion = patches.Versions[0];
            var end = selectedVersion.MaxAddress + 1;
            var buffer = Enumerable.Repeat((byte)0x0C, 0x100000 + end).ToArray();
            var firmware = File.ReadAllBytes(selectedVersion.File);
            selectedVersion.ApplyTo(ref firmware);
            Array.Copy(firmware, 0, buffer, 0x100000, end);

            device.WriteS("getvar:serialno");
            Console.WriteLine("Read: " + device.ReadS());

            Console.WriteLine("Unlock Device? y/n");
            if(Console.ReadKey().Key == ConsoleKey.Y) { 
                device.Write(buffer);
                Console.WriteLine("Read: " + device.ReadS());

                device.WriteS("flash:unlock_token");
                Console.WriteLine("Read: " + device.ReadS());
                Console.WriteLine("Read: " + device.ReadS());

                device.Close();
            }
        }
        Console.WriteLine("Finished! Press any key to close...");
        Console.ReadKey();
    }

}