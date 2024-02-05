using LibUsbDotNet.Main;
using LibUsbDotNet;
using System.Text;

namespace quest_bootloader_unlocker
{
    public class FastbootUsbDevice : IDisposable
    {

        private UsbDeviceFinder _usbDeviceFinder;

        private UsbDevice? _device;
        private UsbEndpointReader? _readEnpoint;
        private UsbEndpointWriter? _writeEndpoint;

        private object _readBufferLock;
        private List<byte> _readBuffer;

        public FastbootUsbDevice(int vid, int pid)
        {
            _usbDeviceFinder = new UsbDeviceFinder(vid, pid);
            _readBufferLock = new object();
            _readBuffer = new List<byte>();

            _device = null;
            _readEnpoint = null;
            _writeEndpoint = null;
        }

        public bool IsConnected()
        {
            return UsbDevice.AllDevices.Select(v => v.DevicePath).Contains(_device?.DevicePath);
        }

        public bool TryConnect()
        {
            _device = UsbDevice.OpenUsbDevice(_usbDeviceFinder);
            if (_device == null)
                return false;

            _readEnpoint = _device.OpenEndpointReader(ReadEndpointID.Ep01);
            _writeEndpoint = _device.OpenEndpointWriter(WriteEndpointID.Ep01);
            _readEnpoint.DataReceived += (sender, args) =>
            {
                lock(_readBufferLock)
                {
                    var buffer = new byte[args.Count];
                    Array.Copy(args.Buffer, buffer, args.Count);
                    _readBuffer.AddRange(buffer);
                    _readBuffer.Add((byte)'\n');
                }
            };
            _readEnpoint.DataReceivedEnabled = true;
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
            lock(_readBufferLock)
            {
                if (_readBuffer.Count == 0)
                    return null;
                var data = _readBuffer.ToArray();
                _readBuffer.Clear();
                return data;
            }
        }

        public string? ReadS()
        {
            if (!IsConnected())
                return null;
            var data = Read();
            if(data == null)
                return null;
            return Encoding.ASCII.GetString(data);
        }

        public void Dispose()
        {
            _readEnpoint?.Dispose();
            _writeEndpoint?.Dispose();
        }

    }
}
