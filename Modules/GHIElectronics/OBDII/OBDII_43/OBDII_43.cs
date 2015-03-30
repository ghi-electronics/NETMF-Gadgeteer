using Elm327.Core;
using Elm327.Core.ObdModes;
using System;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics {
	/// <summary>An OBDII module for Microsoft .NET Gadgeteer.</summary>
	[Obsolete]
	public class OBDII : GTM.Module {
		private string protocalType;
		private string fuelType;
		private string vin;
		private bool connected;
		private string serialPortName;
		private ElmDriver elm;

		/// <summary>The underlying class that handles all of the communication with the ELM327 chip.</summary>
		public ElmDriver Elm {
			get {
				return this.elm;
			}
		}

		/// <summary>Whether or not the module has successfully connected to both the ELM327 and to the vehicle's ECU.</summary>
		public bool Connected {
			get {
				return this.connected;
			}
		}

		/// <summary>The current speed of the vehicle (either in mph or km/h depending on the current unit selection).</summary>
		public double VehicleSpeed {
			get {
				if (!this.connected) throw new InvalidOperationException("You must call Connect first.");

				return this.elm.ObdMode01.VehicleSpeed;
			}
		}

		/// <summary>The engine RPM.</summary>
		public double EngineRpm {
			get {
				if (!this.connected) throw new InvalidOperationException("You must call Connect first.");

				return this.elm.ObdMode01.EngineRpm;
			}
		}

		/// <summary>The throttle position as a percent (0-100).</summary>
		public double ThrottlePosition {
			get {
				if (!this.connected) throw new InvalidOperationException("You must call Connect first.");

				return this.elm.ObdMode01.ThrottlePosition;
			}
		}

		/// <summary>The current engine coolant temperature (in celsius or fahrenheit depending on the current unit selection).</summary>
		public double EngineCoolantTemperature {
			get {
				if (!this.connected) throw new InvalidOperationException("You must call Connect first.");

				return this.elm.ObdMode01.EngineCoolantTemperature;
			}
		}

		/// <summary>The intake air temperature (in celsius or fahrenheit depending on the current unit selection).</summary>
		public double IntakeAirTemperature {
			get {
				if (!this.connected) throw new InvalidOperationException("You must call Connect first.");

				return this.elm.ObdMode01.IntakeAirTemperature;
			}
		}

		/// <summary>The ambient air temperature (in celsius or farenheit depending on the current unit selection).</summary>
		public double AmbientAirTemperature {
			get {
				if (!this.connected) throw new InvalidOperationException("You must call Connect first.");

				return this.elm.ObdMode01.AmbientAirTemperature;
			}
		}

		/// <summary>The battery voltage reading. Note that this value is read directly off the supply pin from the OBD port.</summary>
		public double BatteryVoltage {
			get {
				if (!this.connected) throw new InvalidOperationException("You must call Connect first.");

				return this.elm.BatteryVoltage;
			}
		}

		/// <summary>The current fuel level as a percentage value between 0 and 100.</summary>
		public double FuelLevel {
			get {
				if (!this.connected) throw new InvalidOperationException("You must call Connect first.");

				return this.elm.ObdMode01.FuelLevel;
			}
		}

		/// <summary>Gets the estimated distance per gallon (either miles per gallon or kilometers per gallon depending on the current unit selection).</summary>
		public double EstimatedDistancePerGallon {
			get {
				if (!this.connected) throw new InvalidOperationException("You must call Connect first.");

				return this.elm.ObdMode01.EstimatedDistancePerGallon;
			}
		}

		/// <summary>Tmount of time, in seconds, that the engine has been running.</summary>
		public int RunTimeSinceEngineStart {
			get {
				if (!this.connected) throw new InvalidOperationException("You must call Connect first.");

				return this.elm.ObdMode01.RunTimeSinceEngineStart;
			}
		}

		/// <summary>The current MAF rate in grams/sec.</summary>
		public double MassAirFlowRate {
			get {
				if (!this.connected) throw new InvalidOperationException("You must call Connect first.");

				return this.elm.ObdMode01.MassAirFlowRate;
			}
		}

		/// <summary>The current OBD protocol.</summary>
		public string OBDProtocolType {
			get {
				if (!this.connected) throw new InvalidOperationException("You must call Connect first.");

				return this.protocalType;
			}
		}

		/// <summary>The fuel type for this vehicle.</summary>
		public string VehicleFuelType {
			get {
				if (!this.connected) throw new InvalidOperationException("You must call Connect first.");

				return this.fuelType;
			}
		}

		/// <summary>The Vehicle Identification Number.</summary>
		public string VIN {
			get {
				if (!this.connected) throw new InvalidOperationException("You must call Connect first.");

				return this.vin;
			}
		}

		/// <summary>Constructs a new instance.</summary>
		/// <param name="socketNumber">The socket that this module is plugged in to.</param>
		public OBDII(int socketNumber) {
			this.protocalType = string.Empty;
			this.fuelType = string.Empty;
			this.vin = string.Empty;
			this.connected = false;

			Socket socket = Socket.GetSocket(socketNumber, true, this, null);
			socket.EnsureTypeIsSupported(new char[] { 'U', 'K' }, this);

			this.serialPortName = socket.SerialPortName;
		}

		/// <summary>Attempts to connect to the ECU.</summary>
		public void Connect() {
			this.Connect(ElmDriver.ElmObdProtocolType.Automatic, ElmDriver.ElmMeasuringUnitType.English);
		}

		/// <summary>Attempts to connect to the ECU.</summary>
		/// <param name="protocolType">The protocal type to use.</param>
		/// <param name="measurementUnitType">The measurement type to use.</param>
		public void Connect(ElmDriver.ElmObdProtocolType protocolType, ElmDriver.ElmMeasuringUnitType measurementUnitType) {
			this.elm = new ElmDriver(this.serialPortName, protocolType, measurementUnitType);

			switch (elm.Connect()) {
				case ElmDriver.ElmConnectionResultType.NoConnectionToElm:
					throw new InvalidOperationException("Failed to connect to the ELM327.");

				case ElmDriver.ElmConnectionResultType.NoConnectionToObd:
					throw new InvalidOperationException("Failed to connect to the vehicle's ECU.");

				case ElmDriver.ElmConnectionResultType.Connected:
					this.connected = true;
					this.protocalType = this.GetFriendlyObdProtocolModeTypeName(this.elm.ProtocolType);
					this.fuelType = this.GetFriendlyFuelTypeName(this.elm.ObdMode01.FuelType);
					this.vin = this.elm.ObdMode09.VehicleIdentificationNumber;

					break;
			}
		}

		private string GetFriendlyFuelTypeName(ObdGenericMode01.VehicleFuelType fuelType) {
			switch (fuelType) {
				case Elm327.Core.ObdModes.ObdGenericMode01.VehicleFuelType.BifuelMixedGasElectric:
				case Elm327.Core.ObdModes.ObdGenericMode01.VehicleFuelType.BifuelRunningCNG:
				case Elm327.Core.ObdModes.ObdGenericMode01.VehicleFuelType.BifuelRunningElectricity:
				case Elm327.Core.ObdModes.ObdGenericMode01.VehicleFuelType.BifuelRunningEthanol:
				case Elm327.Core.ObdModes.ObdGenericMode01.VehicleFuelType.BifuelRunningGasoline:
				case Elm327.Core.ObdModes.ObdGenericMode01.VehicleFuelType.BifuelRunningLPG:
				case Elm327.Core.ObdModes.ObdGenericMode01.VehicleFuelType.BifuelRunningMethanol:
				case Elm327.Core.ObdModes.ObdGenericMode01.VehicleFuelType.BifuelRunningProp:
					return "Bifuel";

				case Elm327.Core.ObdModes.ObdGenericMode01.VehicleFuelType.CNG:
					return "Compressed Natural Gas";

				case Elm327.Core.ObdModes.ObdGenericMode01.VehicleFuelType.Diesel:
					return "Diesel";

				case Elm327.Core.ObdModes.ObdGenericMode01.VehicleFuelType.Electric:
					return "Electric";

				case Elm327.Core.ObdModes.ObdGenericMode01.VehicleFuelType.Ethanol:
					return "Ethanol";

				case Elm327.Core.ObdModes.ObdGenericMode01.VehicleFuelType.Gasoline:
					return "Gasoline";

				case Elm327.Core.ObdModes.ObdGenericMode01.VehicleFuelType.HybridDiesel:
				case Elm327.Core.ObdModes.ObdGenericMode01.VehicleFuelType.HybridElectric:
				case Elm327.Core.ObdModes.ObdGenericMode01.VehicleFuelType.HybridEthanol:
				case Elm327.Core.ObdModes.ObdGenericMode01.VehicleFuelType.HybridGasoline:
				case Elm327.Core.ObdModes.ObdGenericMode01.VehicleFuelType.HybridMixedFuel:
				case Elm327.Core.ObdModes.ObdGenericMode01.VehicleFuelType.HybridRegenerative:
					return "Hybrid";

				case Elm327.Core.ObdModes.ObdGenericMode01.VehicleFuelType.LPG:
				case Elm327.Core.ObdModes.ObdGenericMode01.VehicleFuelType.Propane:
					return "Propane";

				case Elm327.Core.ObdModes.ObdGenericMode01.VehicleFuelType.Methanol:
					return "Methanol";

				default:
					return "Unknown";
			}
		}

		private string GetFriendlyObdProtocolModeTypeName(ElmDriver.ElmObdProtocolType protocolType) {
			switch (protocolType) {
				case ElmDriver.ElmObdProtocolType.Iso14230_4_Kwp:
					return "ISO 14230-4 KWP";

				case ElmDriver.ElmObdProtocolType.Iso14230_4_KwpFastInit:
					return "ISO 14230-4 KWP Fast Init";

				case ElmDriver.ElmObdProtocolType.Iso15765_4_Can11Bit:
					return "CAN 11 Bit 250kb";

				case ElmDriver.ElmObdProtocolType.Iso15765_4_Can11BitFast:
					return "CAN 11 Bit 500kb";

				case ElmDriver.ElmObdProtocolType.Iso15765_4_Can29Bit:
					return "CAN 29 Bit 250kb";

				case ElmDriver.ElmObdProtocolType.Iso15765_4_Can29BitFast:
					return "CAN 29 Bit 500kb";

				case ElmDriver.ElmObdProtocolType.Iso9141_2:
					return "ISO 9141-2";

				case ElmDriver.ElmObdProtocolType.SaeJ1850Pwm:
					return "SAE J1850 PWM";

				case ElmDriver.ElmObdProtocolType.SaeJ1850Vpw:
					return "SAE J1850 VPW";

				case ElmDriver.ElmObdProtocolType.SaeJ1939Can:
					return "SAE J1939 CAN";

				default:
					return "Automatic";
			}
		}

		private string GetFriendlyElmConnectionResultTypeName(ElmDriver.ElmConnectionResultType resultType) {
			switch (resultType) {
				case ElmDriver.ElmConnectionResultType.NoConnectionToElm:
					return "No ELM connection";

				case ElmDriver.ElmConnectionResultType.NoConnectionToObd:
					return "No OBD connection";

				default:
					return "Connected";
			}
		}
	}
}