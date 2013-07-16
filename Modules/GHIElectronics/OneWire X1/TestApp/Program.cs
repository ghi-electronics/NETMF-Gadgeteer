using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GTM = Gadgeteer.Modules;

namespace TestApp
{
	public partial class Program
	{
		private GTM.GHIElectronics.OneWire_X1 module;
		private OneWire oneWire;

		void ProgramStarted()
		{
			this.module = new GTM.GHIElectronics.OneWire_X1(4);
			this.oneWire = this.module.Interface;

			ArrayList addresses = oneWire.FindAllDevices();
			if (addresses.Count == 0)
			{
				Debug.Print("No devices present");
				return;
			}

			new Thread(() =>
			{
				int rawTemp;

				while (true)
				{
					oneWire.TouchReset();
					oneWire.WriteByte((byte)Command.SkipROM);
					oneWire.WriteByte((byte)Command.StartTemperatureConversion);

					while (oneWire.ReadByte() == 0)
						;

					oneWire.TouchReset();
					oneWire.WriteByte((byte)Command.SkipROM);
					oneWire.WriteByte((byte)Command.ReadScratchPad);

					rawTemp = oneWire.ReadByte() | (oneWire.ReadByte() << 8);

					Debug.Print((rawTemp * 0.0625 * 1.8 + 32).ToString());

					Thread.Sleep(1000);
				}
			}).Start();
		}

		enum Command : byte
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