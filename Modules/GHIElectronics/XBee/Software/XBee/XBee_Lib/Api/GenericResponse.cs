namespace NETMF.OpenSource.XBee.Api
{
    /// <summary>
    /// Container for unknown response
    /// </summary>
    public class GenericResponse : XBeeResponse
    {
        public byte GenericApiId { get; set; }

        public override void Parse(IPacketParser parser)
        {
            //eat packet bytes -- they will be save to bytearray and stored in response
            parser.ReadRemainingBytes();
            // TODO gotta save it because it isn't know to the enum apiId won't
            GenericApiId = (byte)parser.ApiId;	
        }
    }
}