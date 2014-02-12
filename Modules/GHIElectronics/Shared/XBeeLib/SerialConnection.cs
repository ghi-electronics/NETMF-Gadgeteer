using System.IO.Ports;

namespace NETMF.OpenSource.XBee
{
    public class SerialConnection : IXBeeConnection
    {
        private readonly SerialPort _serialPort;
        private readonly byte[] _buffer;
        
        public SerialConnection(string portName, int baudRate)
        {
            _serialPort = new SerialPort(portName, baudRate);
            _buffer = new byte[1024];
        }

        public void Open()
        {
            // .NET MF 4.1 requires to subscribe to serial events after openning the port

            #if MF_FRAMEWORK_VERSION_V4_1
            _serialPort.Open();
            _serialPort.DataReceived += OnDataReceived;
            #else
            _serialPort.DataReceived += OnDataReceived;
            _serialPort.Open();
            #endif
        }

        public void Close()
        {
            _serialPort.DataReceived -= OnDataReceived;
            _serialPort.Close();
        }

        public bool Connected
        {
            get { return _serialPort.IsOpen; }
        }

        public void Send(byte[] data)
        {
            _serialPort.Write(data, 0, data.Length);
        }

        public void Send(byte[] data, int offset, int count)
        {
            _serialPort.Write(data, offset, count);
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs args)
        {
            while (_serialPort.BytesToRead > 0)
            {
                var bytesRead = _serialPort.Read(_buffer, 0, _serialPort.BytesToRead);

                if (bytesRead <= 0)
                    return;

                DataReceived(_buffer, 0, bytesRead);
            }
        }

        public event DataReceivedEventHandler DataReceived;
    }
}