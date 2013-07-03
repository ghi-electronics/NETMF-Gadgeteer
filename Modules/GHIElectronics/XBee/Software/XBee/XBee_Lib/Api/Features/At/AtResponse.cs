using NETMF.OpenSource.XBee.Util;

namespace NETMF.OpenSource.XBee.Api
{
    public class AtResponse : XBeeFrameIdResponse
    {
        public ushort Command { get; protected set; }

        /// <summary>
        /// Returns the command data byte array.
        /// A zero length array will be returned if the command data is not specified.
        /// This is the case if the at command set a value, or executed a command that does
        /// not have a value (like FR)
        /// </summary>
        public AtResponseStatus Status { get; protected set; }

        // response value msb to lsb
        public byte[] Value { get; protected set; }

        public bool IsOk
        {
            get { return Status == AtResponseStatus.Ok; }
        }

        public override void Parse(IPacketParser parser)
        {
            base.Parse(parser);

            Command = UshortUtils.ToUshort(
                parser.Read("AT Response Char 1"), 
                parser.Read("AT Response Char 2"));

            Status = (AtResponseStatus) parser.Read("AT Response Status");
            Value = parser.ReadRemainingBytes();
        }

        public override string ToString()
        {
            return "command=" + UshortUtils.ToAscii(Command)
                   + ",status=" + Status
                   + ",value=" + (Value == null ? "null" : ByteUtils.ToBase16(Value))
                   + "," + base.ToString();
        }
    }
}