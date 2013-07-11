using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GTM = Gadgeteer.Modules;

namespace TestApp
{
	public partial class Program
	{
		private byte[] id = new byte[8] { 40, 138, 168, 154, 3, 0, 0, 40 };
		private GTM.GHIElectronics.OneWire_X1 oneWire;
		private OneWire ow;
		private OutputPort port;

		void ProgramStarted()
		{
			this.oneWire = new GTM.GHIElectronics.OneWire_X1(2);
			this.ow = this.oneWire.Interface;
			this.port = this.oneWire.Port;

			new Thread(() =>
			{
				Debug.Print("TouchReset: " + ow.TouchReset());
				ArrayList list = ow.FindAllDevices();
				byte[] address = null;

				foreach (byte[] s in list)
				{
					for (int i = 0; i < 8; i++)
						if (s[i] != id[i])
							break;

					address = s;
				}

				if (address == null)
				{
					Debug.Print("Incorrect ID");
					return;
				}

				while (true)
				{
					Debug.Print(GetTempertureF(address).ToString());
					Thread.Sleep(1000);
				}
			}).Start();
		}

        private double GetTempertureF(byte[] ow_address)
        {
            byte[] scratchpad = new byte[9];
            ushort tempLow = 0;
            ushort tempHigh = 0;
            ushort temp = 0x0;
            int temptemp;
            float TemperatureC = 0;
            float TemperatureF = 0;
            ow.TouchReset();
            ow.WriteByte((byte)Command.SkipROM);
            for (int i = 0; i < 8; i++)
            {
                ow.WriteByte((byte)ow_address[i]);
            }


            ow.WriteByte((byte)Command.StartTemperatureConversion);
            port.Write(true);

            for (int i = 0; i < 8; i++)
            {
                ow.WriteByte((byte)ow_address[i]);
            }

            Thread.Sleep(750);
            port.Write(false);

            while (ow.ReadByte() == 0) { }
            ow.TouchReset();
            ow.WriteByte((byte)Command.SkipROM);
            for (int i = 0; i < 8; i++)
            {
                ow.WriteByte((byte)ow_address[i]);
            }

            ow.WriteByte((byte)Command.ReadScratchPad);
            for (int i = 0; i < 9; i++)
            {
                scratchpad[i] = (byte)ow.ReadByte();
            }

            ow.TouchReset();

            tempLow = scratchpad[0];
            tempHigh = scratchpad[1];
            byte config = scratchpad[4];
            temp = tempLow;
            temp |= (ushort)(tempHigh << 8);
            temptemp = (((int)(tempHigh) << 8) | tempLow);
            TemperatureC = temptemp * 0.0625f;
            return ((TemperatureC * 1.8) + 32);
        }

        enum Command
        {
            SearchROM = 0xF0,
            ReadROM = 0x33,
            MatchROM = 0x55,
            SkipROM = 0xCC,
            AlarmSearch = 0xEC,
            StartTemperatureConversion = 0x44,
            StartVoltageConversion = 0xB4,
            ReadScratchPad = 0xBE,
            WriteScratchPad = 0x4E,
            CopySratchPad = 0x48,
            RecallEEPROM = 0xB8,
            ReadPowerSupply = 0xB4,
            ScratchPadPage0 = 0x00,
            ScratchPadPage3 = 0x03
        }
	}
}
