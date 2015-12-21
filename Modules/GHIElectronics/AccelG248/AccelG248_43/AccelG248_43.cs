using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics {
	/// <summary>An AccelG248 module for Microsoft .NET Gadgeteer</summary>
	public class AccelG248 : GTM.Module {
		private GTI.I2CBus i2c;
		private byte[] buffer1;
		private byte[] buffer2;
		private byte[] buffer6;

		/// <summary>Constructs a new instance.</summary>
		/// <param name="socketNumber">The socket that this module is plugged in to.</param>
		public AccelG248(int socketNumber) {
			Socket socket = Socket.GetSocket(socketNumber, true, this, null);
			socket.EnsureTypeIsSupported('I', this);

			this.buffer1 = new byte[1];
			this.buffer2 = new byte[2];
			this.buffer6 = new byte[6];

			this.i2c = GTI.I2CBusFactory.Create(socket, 0x1C, 400, this);
			this.i2c.Write(0x2A, 0x01);
		}

		/// <summary>Gets the current X acceleration value.</summary>
		/// <returns>The X acceleration between -2g and 2g.</returns>
		public double GetX() {
			this.ReadRegister(0x01, this.buffer2);

			double value = (this.buffer2[0] << 2) | (this.buffer2[1] >> 6);

			if (value > 511.0)
				value = value - 1024.0;

			value /= 256.0;

			return value;
		}

		/// <summary>Gets the current Y acceleration value.</summary>
		/// <returns>The Y acceleration between -2g and 2g.</returns>
		public double GetY() {
			this.ReadRegister(0x03, this.buffer2);

			double value = (this.buffer2[0] << 2) | (this.buffer2[1] >> 6);

			if (value > 511.0)
				value = value - 1024.0;

			value /= 256.0;

			return value;
		}

		/// <summary>Gets the current Z acceleration value.</summary>
		/// <returns>The Z acceleration between -2g and 2g.</returns>
		public double GetZ() {
			this.ReadRegister(0x05, this.buffer2);

			double value = (this.buffer2[0] << 2) | (this.buffer2[1] >> 6);

			if (value > 511.0)
				value = value - 1024.0;

			value /= 256.0;

			return value;
		}

		/// <summary>Gets the current acceleration values.</summary>
		/// <returns>The acceleration.</returns>
		public Acceleration GetAcceleration() {
			double x, y, z;

			this.GetXYZ(out x, out y, out z);

			return new Acceleration { X = x, Y = y, Z = z };
		}

		/// <summary>Gets the current acceleration values.</summary>
		/// <param name="x">The x acceleration between -2g and 2g.</param>
		/// <param name="y">The y acceleration between -2g and 2g.</param>
		/// <param name="z">The z acceleration between -2g and 2g.</param>
		public void GetXYZ(out double x, out double y, out double z) {
			this.ReadRegister(0x01, this.buffer6);

			x = (this.buffer6[0] << 2) | (this.buffer6[1] >> 6);
			y = (this.buffer6[2] << 2) | (this.buffer6[3] >> 6);
			z = (this.buffer6[4] << 2) | (this.buffer6[5] >> 6);

			if (x > 511.0)
				x -= 1024.0;

			if (y > 511.0)
				y -= 1024.0;

			if (z > 511.0)
				z -= 1024.0;

			x /= 256.0;
			y /= 256.0;
			z /= 256.0;
		}

		private void ReadRegister(byte register, byte[] read) {
			this.buffer1[0] = register;

			this.i2c.WriteRead(this.buffer1, read);
		}

		/// <summary>Represents an acceleration.</summary>
		public struct Acceleration {
			/// <summary>The x acceleration between -2g and 2g.</summary>
			public double X { get; set; }

			/// <summary>The y acceleration between -2g and 2g.</summary>
			public double Y { get; set; }

			/// <summary>The z acceleration between -2g and 2g.</summary>
			public double Z { get; set; }

			/// <summary>Returns the string representation of this object.</summary>
			/// <returns>The string representation.</returns>
			public override string ToString() {
				return "(" + this.X.ToString("F2") + ", " + this.Y.ToString("F2") + ", " + this.Z.ToString("F2") + ")";
			}
		}
	}
}