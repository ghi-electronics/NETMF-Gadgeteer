using System;
using NETMF.OpenSource.XBee.Util;

namespace NETMF.OpenSource.XBee.Api
{
    /// <summary>
    /// Packages a frame data array into an XBee packet.
    /// </summary>
    public static class XBeePacket
    {
        public enum SpecialByte
        {
            StartByte = 0x7e, // ~
            Escape = 0x7d, // }
            Xon = 0x11,
            Xoff = 0x13
        }

        /// <summary>
        /// Performs the necessary activities to construct an XBee packet from the frame data.
        /// This includes: computing the checksum, escaping the necessary bytes, adding the start byte and length bytes.
        /// The format of a packet is as follows:
        /// start byte - msb length byte - lsb length byte - frame data - checksum byte
        /// </summary>
        /// <param name="request"></param>
        public static byte[] GetBytes(XBeeRequest request)
        {
            var frameData = request.GetFrameData();

            // packet size is frame data + start byte + 2 length bytes + checksum byte
            var bytes = new byte[frameData.Length + 4];
            bytes[0] = (byte)SpecialByte.StartByte;

            // Packet length does not include escape bytes or start, length and checksum bytes
            var length = (ushort)frameData.Length;

            // msb length (will be zero until maybe someday when > 255 bytes packets are supported)
            bytes[1] = UshortUtils.Msb(length);
            // lsb length
            bytes[2] = UshortUtils.Lsb(length);

            Array.Copy(frameData, 0, bytes, 3, frameData.Length);

            // set last byte as checksum
            // note: if checksum is not correct, XBee won't send out packet or return error.  ask me how I know.

            bytes[bytes.Length - 1] = Checksum.Compute(frameData, 0, frameData.Length);

            var preEscapeLength = bytes.Length;

            // TODO save escaping for the serial out method. this is an unnecessary operation
            bytes = EscapePacket(bytes);

            var escapeLength = bytes.Length;

            var packetStr = "Packet: ";
            for (var i = 0; i < escapeLength; i++)
            {
                packetStr += ByteUtils.ToBase16(bytes[i]);

                if (i < escapeLength - 1)
                    packetStr += " ";
            }

            Logger.LowDebug(packetStr);
            Logger.LowDebug("pre-escape packet size is " + preEscapeLength + ", post-escape packet size is " + escapeLength);

            return bytes;
        }

        /// <summary>
        /// Escape all bytes in packet after start byte, and including checksum
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        private static byte[] EscapePacket(byte[] packet)
        {
            var escapeBytes = 0;

            // escape packet.  start at one so we don't escape the start byte 
            for (var i = 1; i < packet.Length; i++)
            {
                if (!IsSpecialByte(packet[i])) 
                    continue;
                
                Logger.LowDebug("escapeFrameData: packet byte requires escaping byte " + ByteUtils.ToBase16(packet[i]));
                escapeBytes++;
            }

            if (escapeBytes == 0)
			    return packet;

            Logger.LowDebug("packet requires escaping");

            var escapePacket = new byte[packet.Length + escapeBytes];
			
            var pos = 1;

            escapePacket[0] = (byte)SpecialByte.StartByte;
				
            for (var i = 1; i < packet.Length; i++) 
            {
                if (IsSpecialByte(packet[i])) 
                {
                    escapePacket[pos] = (byte)SpecialByte.Escape;
                    escapePacket[++pos] = (byte) (0x20 ^ packet[i]);
                    Logger.LowDebug("escapeFrameData: xor'd byte is 0x" + ByteUtils.ToBase16(escapePacket[pos]));
                } 
                else 
                {
                    escapePacket[pos] = packet[i];
                }
				
                pos++;
            }
			
            return escapePacket;
        }

        public static byte[] UnEscapePacket(byte[] packet)
        {
		    var escapeBytes = 0;

            foreach (var b in packet)
                if (b == (byte)SpecialByte.Escape)
                    escapeBytes++;
		
		    if (escapeBytes == 0)
			    return packet;

            var unEscapedPacket = new byte[packet.Length - escapeBytes];
		
		    var pos = 0;
		
		    for (var i = 0; i < packet.Length; i++) 
            {
                if (packet[i] == (byte)SpecialByte.Escape) 
                {
				    // discard escape byte and un-escape following byte
				    unEscapedPacket[pos] = (byte) (0x20 ^ packet[++i]);
			    } 
                else 
                {
				    unEscapedPacket[pos] = packet[i];
			    }
			
			    pos++;
		    }
		
		    return unEscapedPacket;
        }

        public static bool IsSpecialByte(byte b)
        {
            switch ((SpecialByte)b)
            {
                case SpecialByte.StartByte:
                case SpecialByte.Escape:
                case SpecialByte.Xon:
                case SpecialByte.Xoff:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsStartByte(byte b)
        {
            return b == (byte)SpecialByte.StartByte;
        }

        /// <summary>
        /// Returns true if the packet is valid
        /// </summary>
        /// <param name="packet"></param>
        /// <returns> true if the packet is valid</returns>
        public static bool Verify(byte[] packet)
        {
            try
            {
                if (packet[0] != (byte)SpecialByte.StartByte || packet.Length < 4)
                    return false;
 
                // first need to unescape packet
                var unEscaped = UnEscapePacket(packet);

                var packetChecksum = unEscaped[unEscaped.Length - 1];
                var validChecksum = Checksum.Compute(unEscaped, 3, unEscaped.Length - 4);

                return packetChecksum == validChecksum;
            }
            catch (Exception e)
            {
                throw new Exception("Packet verification failed with error: ", e);
            }
        }
    }
}