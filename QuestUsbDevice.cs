using LibUsbDotNet.Main;
using LibUsbDotNet;
using System.Text;

namespace quest_bootloader_unlocker
{
    public class QuestUsbDevice : IDisposable
    {

        private static readonly UsbDeviceFinder UsbDeviceFinder = new UsbDeviceFinder(0x2833, 0x81);

        private UsbDevice _device;
        private UsbEndpointWriter _writeEndpoint;
        private UsbEndpointReader _readEnpoint;

        public bool IsConnected()
        {
            return UsbDevice.AllDevices.Select(v => v.DevicePath).Contains(_device?.DevicePath);
        }

        public bool TryConnect()
        {
            _device = UsbDevice.OpenUsbDevice(UsbDeviceFinder);
            if (_device == null)
                return false;

            _readEnpoint = _device.OpenEndpointReader(ReadEndpointID.Ep01);
            _writeEndpoint = _device.OpenEndpointWriter(WriteEndpointID.Ep01);
            return true;
        }

        public bool Close()
        {
            if (_device == null)
            {
                return false;
            }
            if (!IsConnected())
                return false;
            return _device.Close();
        }

        public int? Write(byte[] buffer)
        {
            if (!IsConnected())
                return null;
            int bytesWritten;
            _writeEndpoint.Write(buffer, 10000, out bytesWritten);
            return bytesWritten;
        }

        public int? WriteS(string data)
        {
            if (!IsConnected())
                return null;
            return Write(Encoding.ASCII.GetBytes(data));
        }

        public byte[]? Read()
        {
            if(!IsConnected())
                return null;

            List<byte> buffer = new List<byte>();
            int readBytes = 0;
            do
            {
                var readBuffer = new byte[512];
                _readEnpoint.Read(readBuffer, 1000, out readBytes);
                buffer.AddRange(readBuffer);
            } while (readBytes == 512);
            return buffer.ToArray();
        }

        public string? ReadS()
        {
            if (!IsConnected())
                return null;
            return Encoding.ASCII.GetString(Read());
        }

        public void Dispose()
        {
            _readEnpoint?.Dispose();
            _writeEndpoint?.Dispose();
        }

    }
}
