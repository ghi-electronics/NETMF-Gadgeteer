using System;
using NETMF.OpenSource.XBee.Util;

namespace NETMF.OpenSource.XBee.Api.Zigbee
{
    /// <summary>
    /// Series 2 XBee. Represents an I/O Sample response sent from a remote radio.
    /// Provides access to the XBee's 4 Analog (0-4), 11 Digital (0-7,10-12), and 1 Supply Voltage pins
    /// </summary>
    /// <remarks>
    /// Series 2 XBee does not support multiple samples (IT) per packet
    /// </remarks>
    public class IoSampleResponse : RxResponseBase
    {
        public enum Pin
        {
            A0 = 0,
            A1 = 1,
            A2 = 2,
            A3 = 3,
            D0 = 0,
            D1 = 1,
            D2 = 2,
            D3 = 3,
            D4 = 4,
            D5 = 5,
            D6 = 6,
            D7 = 7,
            /// <summary>
            /// The voltage supply threshold is set with the V+ command.  
            /// If the measured supply voltage falls below or equal to this threshold, 
            /// the supply voltage will be included in the IO sample set.
            /// V+ is set to 0 by default (do not include the supply voltage). 
            /// </summary>
            SupplyVoltage = 7,
            D10 = 10,
            D11 = 11,
            D12 = 12
        }

        private const byte SupplyVoltageIndex = 4;

        public ushort DigitalChannelMask { get; protected set; }
        public byte AnalogChannelMask { get; protected set; }
        public ushort Digital { get; protected set; }
        public int[] Analog { get; protected set; }

        public bool ContainsDigital { get { return DigitalChannelMask > 0; } }
        public bool ContainsAnalog { get { return AnalogChannelMask > 0; } }

        public static IoSampleResponse ParseIsSample(AtResponse response)
        {
            if (response.Command != (ushort) Common.AtCmd.ForceSample)
			    throw new ArgumentException("This is only applicable to the 'IS' AT command");
		
		    var input = new InputStream(response.Value);
		    var sample = new IoSampleResponse();
            //sample.ParseIoSample(input);
		
		    return sample;
	    }

        protected override void ParseFramePayload(IPacketParser parser)
        {
            // eat sample size.. always 1
            var size = parser.Read("ZNet RX IO Sample Size");

            if (size != 1)
                throw new XBeeParseException("Sample size is not supported if > 1 for ZNet I/O");

            DigitalChannelMask = UshortUtils.ToUshort(parser.Read("ZNet RX IO Sample Digital Mask 1"),
                                                      parser.Read("ZNet RX IO Sample Digital Mask 2"));

            // TODO apparent bug: channel mask on ZigBee Pro firmware has DIO10/P0 as enabled even though it's set to 01 (RSSI).  Digital value reports low. 
            DigitalChannelMask &= 0x1CFF; //11100 zero out all but bits 3-5

            AnalogChannelMask = parser.Read("ZNet RX IO Sample Analog Channel Mask");
            AnalogChannelMask &= 0x8f; //10001111 zero out n/a bits

            if (ContainsDigital)
                Digital = UshortUtils.ToUshort(parser.Read("ZNet RX IO DIO MSB"),
                                               parser.Read("ZNet RX IO DIO LSB"));

            // parse 10-bit analog values

            Analog = new int[5];

            for (var pin = Pin.A0; pin < Pin.A3; pin++)
            {
                if (!IsAnalogEnabled(pin))
                    continue;

                Analog[(byte)pin] = UshortUtils.Parse10BitAnalog(parser.Read(), parser.Read());
            }

            if (IsAnalogEnabled(Pin.SupplyVoltage))
                Analog[SupplyVoltageIndex] = UshortUtils.Parse10BitAnalog(parser.Read(), parser.Read());
        }

        public bool IsAnalogEnabled(Pin pin)
        {
            var pinNumber = (byte)pin;

            if ((pinNumber >= 0 && pinNumber <= 3) || pin == Pin.SupplyVoltage)
                return ByteUtils.GetBit(AnalogChannelMask, pinNumber);

            throw new ArgumentOutOfRangeException("Unsupported pin: " + pin);
        }

        public bool IsDigitalEnabled(Pin pin)
        {
            var pinNumber = (byte)pin;

            if (pinNumber >= 0 && pinNumber <= 7)
                return ByteUtils.GetBit(UshortUtils.Lsb(DigitalChannelMask), pinNumber);
            
            if (pinNumber >= 10 && pinNumber <= 12)
                return ByteUtils.GetBit(UshortUtils.Msb(DigitalChannelMask), pinNumber - 8);

            throw new ArgumentOutOfRangeException("Unsupported pin: " + pin);
        }

        /// <summary>
        /// If digital I/O line (DIO0) is enabled: returns true if digital 0 is HIGH (ON); false if it is LOW (OFF).
        /// If digital I/O line is not enabled this method returns false.
        /// </summary>
        /// <remarks>
        /// Important: the pin number corresponds to the logical pin (e.g. D4), not the physical pin number.
        /// Digital I/O pins seem to report high when open circuit (unconnected)
        /// </remarks>
        /// <param name="pin"></param>
        /// <returns></returns>
        public bool IsDigitalOn(Pin pin)
        {
            if (!IsDigitalEnabled(pin))
                return false;

            var pinNumber = (byte)pin;

            if (pinNumber >= 0 && pinNumber <= 7)
                return ByteUtils.GetBit(UshortUtils.Lsb(Digital), pinNumber);
            
            return ByteUtils.GetBit(UshortUtils.Msb(Digital), pinNumber - 8);
        }

        /// <summary>
        /// Returns a 10 bit value of ADC line 0, if enabled.
        /// Returns -1 if ADC line 0 is not enabled.
        /// </summary>
        /// <remarks>
        /// The range of Digi XBee series 2 ADC is 0 - 1.2V and although I couldn't find this 
        /// in the spec a few google searches seems to confirm. When I connected 3.3V to just 
        /// one of the ADC pins, it displayed its displeasure by reporting all ADC pins at 1023.
        /// <para>Analog pins seem to float around 512 when open circuit</para>
        /// The reason this returns null is to prevent bugs in the event that you thought 
        /// you were reading the actual value when the pin is not enabled.
        /// </remarks>
        /// <param name="pin"></param>
        /// <returns></returns>
        public int GetAnalog(Pin pin)
        {
            if (!IsAnalogEnabled(pin))
                return -1;

            // analog pins are 0-3 and Pin.SupplyVoltage is 7
            // we need to adjust the pinNumber to use it as array index
            var pinNumber = pin == Pin.SupplyVoltage ? SupplyVoltageIndex : (byte)pin;

            return Analog[pinNumber];
        }

        public int GetSupplyVoltage()
        {
            return GetAnalog(Pin.SupplyVoltage);
        }

        public override string ToString()
        {
            var result = string.Empty;

            if (ContainsDigital)
            {
                for (var pin = Pin.D0; pin <= Pin.D7; pin++)
                {
                    if (!IsDigitalEnabled(pin)) continue;
                    result += ",digital[" + pin + "]=" + (IsDigitalOn(pin) ? "high" : "low");
                }

                for (var pin = Pin.D10; pin <= Pin.D12; pin++)
                {
                    if (!IsDigitalEnabled(pin)) continue;
                    result += ",digital[" + pin + "]=" + (IsDigitalOn(pin) ? "high" : "low");
                }
            }

            if (ContainsAnalog)
            {
                for (var pin = Pin.A0; pin <= Pin.A3; pin++)
                {
                    if (!IsAnalogEnabled(pin)) continue;
                    result += ",analog[" + pin + "]=" + GetAnalog(pin);
                }

                if (IsAnalogEnabled(Pin.SupplyVoltage))
                    result += ",supplyVoltage=" + GetSupplyVoltage();
            }

            return result;
        }
    }
}