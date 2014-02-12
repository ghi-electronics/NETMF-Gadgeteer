using NETMF.OpenSource.XBee.Util;

namespace NETMF.OpenSource.XBee.Api.Wpan
{
    /// <summary>
    /// Series 1 XBee. Represents an I/O sample
    /// </summary>
    public class IoSampleResponse : RxResponseBase, INoRequestResponse
    {
        public static int AnalogMask = 0x7E00; //0111111000000000
	    public static int DigitalMask = 0x1FF; //0000000111111111

        public byte SampleCount { get; protected set; }

        public IoSample[] Samples { get; protected set; }

        /// <summary>
        /// Defines which inputs are active.
        /// </summary>
        public ushort ChannelIndicator { get; protected set; }

        /// <summary>
        /// Returns true if this packet contains at least one digital sample
        /// </summary>
        public bool ContainsDigital { get; protected set; }

        /// <summary>
        /// Return true if this packet contains at least one analog sample
        /// </summary>
        public bool ContainsAnalog { get; protected set; }

        public override void Parse(IPacketParser parser)
        {
            Source = parser.ApiId == ApiId.Rx16IoResponse
             ? (XBeeAddress)parser.ParseAddress16()
             : parser.ParseAddress64();

            base.Parse(parser);

            SampleCount = parser.Read();
            ChannelIndicator = UshortUtils.ToUshort(parser.Read(), parser.Read());
            ContainsDigital = (ChannelIndicator & DigitalMask) > 0;
            ContainsAnalog = (ChannelIndicator & AnalogMask) > 0;
            Samples = new IoSample[SampleCount];

            for (var i = 0; i < SampleCount; i++)
            {
                Logger.LowDebug("Parsing I/O sample nr " + (i + 1));
                Samples[i] = ParseIoSample(parser);
            }
        }

        private IoSample ParseIoSample(IPacketParser parser)
        {
            ushort digital = 0;
            var analog = new double[Pin.AnalogCount];

            if (ContainsDigital)
            {
                Logger.LowDebug("Sample contains digital inputs");
                digital = UshortUtils.ToUshort(parser.Read(), parser.Read());
            }

            if (ContainsAnalog)
            {
                var analogCount = 0;

                for (var i = 0; i < analog.Length; i++)
                {
                    if (!IsEnabled((Pin.Analog) i))
                        continue;

                    var reading = UshortUtils.Parse10BitAnalog(parser.Read(), parser.Read());
                    analog[i] = AdcHelper.ToMilliVolts(reading);
                    analogCount++;
                }

                Logger.LowDebug("Sample contains " + analogCount +" analog inputs");
            }

            return new IoSample(analog, digital);
        }

        public bool IsEnabled(Pin.Digital input)
        {
            return UshortUtils.GetBit(ChannelIndicator, (byte)input);
        }

        public bool IsEnabled(Pin.Analog input)
        {
            var bitNumber = (byte) ((byte)input + Pin.DigitalCount);
            return UshortUtils.GetBit(ChannelIndicator, bitNumber);
        }

        public override string ToString()
        {
            if (SampleCount == 0 || (!ContainsAnalog && !ContainsDigital))
                return base.ToString() + ", no I/O data";
        
            var result = string.Empty;

            if (ContainsDigital)
                for (var pin = Pin.Digital.D0; pin <= Pin.Digital.D8; pin++)
                {
                    if (!IsEnabled(pin)) continue;
                    result += ", D" + pin + "=" + (Samples[0].GetValue(pin) ? "1" : "0");
                }

            if (ContainsAnalog)
                for (var pin = Pin.Analog.A0; pin <= Pin.Analog.A5; pin++)
                {
                    if (!IsEnabled(pin)) continue;
                    result += ", A" + pin + "=" + Samples[0].GetValue(pin).ToString("F2");
                }

            return "SampleCount=" + SampleCount
                + result;
        }
    }
}