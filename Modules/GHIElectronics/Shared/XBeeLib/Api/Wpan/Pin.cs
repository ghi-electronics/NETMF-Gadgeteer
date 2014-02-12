namespace NETMF.OpenSource.XBee.Api.Wpan
{
    /// <summary>
    ///  TODO: Update comments
    ///     
    /// </summary>
    /// <remarks>
    ///     
    /// </remarks>
    public static class Pin
    {

        /// <summary>
        ///  TODO: Update Comments
        ///     
        /// </summary>
        public static int DigitalCount = 9;

        /// <summary>
        ///  TODO: Update Comments
        ///     
        /// </summary>
        public static int AnalogCount = 6;

        public enum Digital : byte
        {
            /// <summary>
            /// AD0 / DIO0. Pin #20.
            /// </summary>
            D0,
            /// <summary>
            /// AD1 / DIO1. Pin #19.
            /// </summary>
            D1,
            /// <summary>
            /// AD2 / DIO2. Pin #18.
            /// </summary>
            D2,
            /// <summary>
            /// AD3 / DIO3 / (COORD_SEL). Pin #17.
            /// </summary>
            D3,
            /// <summary>
            /// AD4 / DIO4. Pin #11.
            /// </summary>
            D4,
            /// <summary>
            /// AD5 / DIO5 / (ASSOCIATE). Pin #15.
            /// </summary>
            D5,
            /// <summary>
            /// DIO6 / (RTS). Pin #16.
            /// </summary>
            D6,
            /// <summary>
            /// DIO6 / (CTS). Pin #12.
            /// </summary>
            D7,
            /// <summary>
            /// DI8 / (DTR) / (Sleep_RQ). Pin #9.
            /// </summary>
            D8
        }

        public enum Analog : byte
        {
            /// <summary>
            /// AD0 / DIO0. Pin #20.
            /// </summary>
            A0,
            /// <summary>
            /// AD1 / DIO1. Pin #19.
            /// </summary>
            A1,
            /// <summary>
            /// AD2 / DIO2. Pin #18.
            /// </summary>
            A2,
            /// <summary>
            /// AD3 / DIO3 / (COORD_SEL). Pin #17.
            /// </summary>
            A3,
            /// <summary>
            /// AD4 / DIO4. Pin #11.
            /// </summary>
            A4,
            /// <summary>
            /// AD5 / DIO5 / (ASSOCIATE). Pin #15.
            /// </summary>
            A5
        }
    }
}