using System;
using Microsoft.SPOT;

using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.SocketInterfaces;

namespace Gadgeteer.Modules.GHIElectronics
{
	/// <summary>
	/// A SPlus module for Microsoft .NET Gadgeteer
	/// </summary>
	public class SPlus : GTM.Module
	{
		private GT.Socket sSocket;
		private GT.Socket ySocket;
		 
		private GT.Socket sx1;
		private GT.Socket sx2;

		/// <summary>Creates a new SPlus instance.</summary>
		/// <param name="socketNumber">The socket that this module is plugged in to.</param>
		/// <param name="socketNumberTwo">The second socket that this module is plugged in to.</param>
		public SPlus(int socketNumber, int socketNumberTwo)
		{
			this.sSocket = Socket.GetSocket(socketNumber, true, this, null);
			this.ySocket = Socket.GetSocket(socketNumberTwo, true, this, null);

			this.sSocket.EnsureTypeIsSupported('S', this);
			this.ySocket.EnsureTypeIsSupported('Y', this);

			this.sx1 = Socket.SocketInterfaces.CreateUnnumberedSocket("SPlus1");
			this.sx2 = Socket.SocketInterfaces.CreateUnnumberedSocket("SPlus2");

			this.sx1.SupportedTypes = this.sx2.SupportedTypes = new char[2] { 'S', 'X' };

			for (int i = 3; i <= 6; i++)
			{
				this.sx1.CpuPins[i] = this.sSocket.CpuPins[i];
				this.sx2.CpuPins[i] = this.ySocket.CpuPins[i];
			}

			this.sx1.CpuPins[7] = this.sx2.CpuPins[7] = this.sSocket.CpuPins[7];
			this.sx1.CpuPins[8] = this.sx2.CpuPins[8] = this.sSocket.CpuPins[8];
			this.sx1.CpuPins[9] = this.sx2.CpuPins[9] = this.sSocket.CpuPins[9];
			this.sx1.SPIModule = this.sx2.SPIModule = this.sSocket.SPIModule;

			Socket.SocketInterfaces.RegisterSocket(this.sx1);
			Socket.SocketInterfaces.RegisterSocket(this.sx2);
		}

		/// <summary>
		/// Returns the socket number for socket 1 on the hub.
		/// </summary>
		public int SHubSocket1 { get { return this.sx1.SocketNumber; } }

		/// <summary>
		/// Returns the socket number for socket 2 on the hub.
		/// </summary>
		public int SHubSocket2 { get { return this.sx2.SocketNumber; } }
	}
}
