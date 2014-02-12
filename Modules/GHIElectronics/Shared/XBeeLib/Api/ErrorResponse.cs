using System;

namespace NETMF.OpenSource.XBee.Api
{
    /// <summary>
    /// Represents a Java error during packet parsing.
    /// This is the only class that extends XBeeResponse and does not map
    /// to a XBee API ID
    /// </summary>
    public class ErrorResponse : XBeeResponse
    {
        public string ErrorMsg { get; set; }
        public Exception Exception { get; set; }

        public ErrorResponse()
        {
            ApiId = ApiId.ErrorResponse;
            Error = true;
        }

        public override void Parse(IPacketParser parser)
        {
            // do nothing
        }

        public override string ToString()
        {
            return base.ToString()
                   + ",errorMsg=" + ErrorMsg
                   + ",exception=" + Exception;
        }
    }
}