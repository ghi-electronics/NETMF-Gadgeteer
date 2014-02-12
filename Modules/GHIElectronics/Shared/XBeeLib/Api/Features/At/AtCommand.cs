using NETMF.OpenSource.XBee.Util;

namespace NETMF.OpenSource.XBee.Api
{
    /// <summary>
    /// API technique to set/query commands
    /// </summary>
    /// <remarks>
    /// WARNING: Any changes made will not survive a power cycle unless written to memory with WR command
    /// According to the manual, the WR command can only be written so many times.. however many that is.
    /// <para>
    /// API ID: 0x8
    /// </para>
    /// Determining radio type with HV:
    /// <list type="table">
    ///     <listheader>
    ///         <term>Byte 1</term>
    ///         <description>Part Number</description>
    ///     </listheader>  
    ///     <item>
    ///         <term>x17</term>
    ///         <description>XB24 (series 1)</description>
    ///     </item>
    ///     <item>
    ///         <term>x18</term>
    ///         <description>XBP24 (series 1)</description>
    ///     </item>
    ///     <item>
    ///         <term>x19</term>
    ///         <description>XB24-B (series 2</description>
    ///     </item>
    ///     <item>
    ///         <term>x1A</term>
    ///         <description>XBP24-B (series 2)</description>
    ///     </item>
    /// </list>
    /// XB24-ZB
    /// XBP24-ZB
    /// </remarks>
    public class AtCommand : XBeeRequest
    {
        public ushort Command { get; set; }
        public byte[] Value { get; set; }

        public AtCommand(string command, params byte[] value)
            :this(UshortUtils.FromAscii(command), value)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="command"></param>
        /// <param name="value"></param>
        public AtCommand(ushort command, params byte[] value)
        {
            Command = command;
            Value = value;
        }

        public override ApiId ApiId
        {
            get { return ApiId.AtCommand; }
        }

        public override byte[] GetFrameData()
        {
            var frameData = new OutputStream();

            frameData.Write((byte) ApiId);
            frameData.Write(FrameId);
            frameData.Write(Command);

            if (Value != null)
                frameData.Write(Value);

            return frameData.ToArray();
        }

        public override string ToString()
        {
            return base.ToString()
                   + ",command=" + UshortUtils.ToAscii(Command)
                   + ",value=" + (Value == null ? "null" : ByteUtils.ToBase16(Value));
        }
    }
}