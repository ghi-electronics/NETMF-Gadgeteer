namespace NETMF.OpenSource.XBee
{
    /// <summary>
    /// Represents a configurable XBee pin and associated name, pin number, AT command, 
    /// default capability and list of supported capabilities.
    /// </summary>
    public class XBeePin
    {
        #region Capability enum

        /// <summary>
        /// Contains all possible pin configurations and the associated AT command value
        /// </summary>
        public enum Capability
        {
            None = -1,
            Disabled = 0,
            RtsFlowCtrl = 1,
            CtsFlowCtrl = 1,
            RssiPwm = 1,
            AssocLed = 1,
            AnalogInput = 2,
            PwmOutput = 2,
            DigitalInput = 3,
            DigitalOutputLow = 4,
            DigitalOutputHigh = 5,

            /// <summary>
            /// only zigbee
            /// </summary>
            UnmonitoredInput = 0,

            /// <summary>
            /// only zigbee
            /// </summary>
            NodeIdEnabled = 1,

            /// <summary>
            /// only zigbee
            /// </summary>
            Rs485TxLow = 6,

            /// <summary>
            /// only zigbee
            /// </summary>
            Rs485TxHigh = 7
        }

        #endregion

        //TODO add pin direction
        //TODO methods to filter list by Capability

        public string Name { get; set; }
        public int Pin { get; set; }
        public string AtCommand { get; set; }
        public int AtPin { get; set; }

        // TODO add logical pin e.g. getDigital(pin)

        public string Description { get; set; }
        public Capability DefaultCapability { get; set; }
        public Capability[] Capabilities { get; set; }

        //private static XBeePin[] _wpanPins;
        private static XBeePin[] _zigBeePins;

        //public static XBeePin[] WpanPins
        //{
        //    get
        //    {
        //        if (_wpanPins == null)
        //            CreateWpanPins();

        //        return _wpanPins;
        //    }
        //}

        public static XBeePin[] ZigBeePins
        {
            get
            {
                if (_zigBeePins == null)
                    CreateZigBeePins();

                return _zigBeePins;
            }
        }

        private static void CreateZigBeePins()
        {
            // notes: DIO13/DIO8/DIO9 not supported
            // TODO P0/P1 is missing pwm output option?? could it be 0x2?

            _zigBeePins = new[]
            {
                new XBeePin("PWM0/RSSI/DIO10", 6, "P0", 10, Capability.RssiPwm, "PWM Output 0 / RX Signal Strength Indicator / Digital IO", new[]
                {
                    Capability.Disabled, 
                    Capability.RssiPwm,
                    Capability.DigitalInput, 
                    Capability.DigitalOutputLow,
                    Capability.DigitalOutputHigh
                }),

                new XBeePin("PWM/DIO11", 7, "P1", 1, Capability.UnmonitoredInput, "Digital I/O 11", new [] 
                {
                    Capability.UnmonitoredInput,
                    Capability.DigitalInput,
                    Capability.DigitalOutputLow, 
                    Capability.DigitalOutputHigh
                }),

                new XBeePin("DIO12", 4, "P2", 2, Capability.UnmonitoredInput, "Digital I/O 12", new[] 
                {
                    Capability.UnmonitoredInput, 
				    Capability.DigitalInput, 
                    Capability.DigitalOutputLow, 
                    Capability.DigitalOutputHigh
                }),

                new XBeePin("AD0/DIO0/Commissioning Button", 20, "D0", 0, Capability.NodeIdEnabled, "Analog Input 0, Digital IO 0, or Commissioning Button", new[] 
                {
                    Capability.Disabled, 
                    Capability.NodeIdEnabled, 
				    Capability.AnalogInput, 
                    Capability.DigitalInput, 
                    Capability.DigitalOutputLow, 
                    Capability.DigitalOutputHigh
                }),

                new XBeePin("AD1/DIO1", 19, "D1", 1, Capability.Disabled, "Analog Input 1 or Digital I/O 1", new[] 
                {
                    Capability.Disabled, 
                    Capability.AnalogInput, 
                    Capability.DigitalInput, 
                    Capability.DigitalOutputLow, 
                    Capability.DigitalOutputHigh
                }),

                new XBeePin("AD2/DIO2", 18, "D2", 2, Capability.Disabled, "Analog Input 2 or Digital I/O 2", new[] 
                {
                    Capability.Disabled,
                    Capability.AnalogInput,
                    Capability.DigitalInput, 
                    Capability.DigitalOutputLow, 
                    Capability.DigitalOutputHigh
                }),

                new XBeePin("AD3/DIO3", 17, "D3", 3, Capability.Disabled, "Analog Input 3 or Digital I/O 3", new[] 
                {
                    Capability.Disabled, 
				    Capability.AnalogInput,
                    Capability.DigitalInput, 
                    Capability.DigitalOutputLow, 
                    Capability.DigitalOutputHigh
                }),

                new XBeePin("DIO4", 11, "D4", 4, Capability.Disabled, "Digital I/O 4", new[] 
                {
                    Capability.Disabled, 
                    Capability.DigitalInput, 
                    Capability.DigitalOutputLow,
                    Capability.DigitalOutputHigh
                }),

                new XBeePin("Associate/DIO5", 15, "D5", 5, Capability.AssocLed, "Associated Indicator, Digital I/O 5", new[] 
                {
				    Capability.Disabled,
                    Capability.AssocLed,
                    Capability.DigitalInput, 
                    Capability.DigitalOutputLow, 
				    Capability.DigitalOutputHigh
                }),

                new XBeePin("CTS/DIO7", 12, "D7", 7, Capability.CtsFlowCtrl, "Clear-to-Send Flow Control or Digital I/O 7", new[] 
                {
				    Capability.Disabled, 
                    Capability.CtsFlowCtrl,
                    Capability.DigitalInput,
                    Capability.DigitalOutputLow, 
				    Capability.DigitalOutputHigh, 
                    Capability.Rs485TxLow, 
                    Capability.Rs485TxHigh
                }),

                // TODO manual lists only RTS and disabled but x-ctu lists all digital capabilities

                new XBeePin("RTS/DIO6", 16, "D6", 6, Capability.Disabled, "Request-to-Send Flow Control, Digital I/O 6", new[] 
				{
                    Capability.Disabled,
                    Capability.RtsFlowCtrl,
                    Capability.DigitalInput, 
                    Capability.DigitalOutputLow, 
				    Capability.DigitalOutputHigh
                }),

                // other pins
                new XBeePin("VCC", 1, "", -1, Capability.None, "Power Supply", null),
                new XBeePin("DOUT", 2, "", -1, Capability.None, "UART Data Out", null),
                new XBeePin("DIN", 3, "", -1, Capability.None, "UART Data In", null),
                new XBeePin("RESET", 5, "", -1, Capability.None, "Module Reset (reset pulse must be at least 200 ns)", null),
                new XBeePin("[reserved]", 8, "", -1, Capability.None, "Do not connect", null),
                // DIO8 not supported according to manual
                new XBeePin("DTR/SLEEP_RQ", 9, "", -1, Capability.None, "Pin Sleep Control Line", null),
                new XBeePin("GND", 10, "", -1, Capability.None, "Ground", null),
                // DIO9 not supported according to manual
                new XBeePin("ON/SLEEP", 13, "", -1, Capability.None, "Module Status Indicator", null),
                new XBeePin("VREF", 14, "", -1, Capability.None, "Not used on this module. For compatibility with other XBee modules, we recommend connecting this pin to a voltage reference if Analog sampling is desired. Otherwise, connect to GND", null)
            };
        }

        private static void CreateWpanPins()
        {
            throw new System.NotImplementedException();
        }

        public XBeePin(string name, int pin, string atCommand, int atPin, Capability defaultCapability, string description, Capability[] capabilities)
        {
            Name = name;
            Pin = pin;
            AtCommand = atCommand;
            AtPin = atPin;
            Description = description;
            DefaultCapability = defaultCapability;
            Capabilities = capabilities;
        }
    }
}