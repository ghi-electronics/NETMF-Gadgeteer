using System;
using Microsoft.SPOT;
using System.IO.Ports;

namespace Gadgeteer.Modules.GHIElectronics
{
    public class HttpRequest
    {
        public enum HttpRequestType
        {
            GET,
            POST
        }

        public HttpRequestType RequestType;

        public HttpHeaderList HeaderData;

        public QueryDataList QueryData;

        public string URL = "";

        public string PostData = "";

        public int PostLength = 0;

        public bool ContainsPostData
        {
            get
            {
                return (PostData.Length > 0);
            }
        }

        public HttpRequest()
        {
            HeaderData = new HttpHeaderList();
        }
    }
}
