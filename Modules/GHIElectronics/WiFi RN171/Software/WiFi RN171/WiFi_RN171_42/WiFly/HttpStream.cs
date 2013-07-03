using System;
using System.IO;

using Microsoft.SPOT.IO;
using Microsoft.SPOT;
using System.IO.Ports;

namespace Gadgeteer.Modules.GHIElectronics
{
    public class HttpStream
    {
        public HttpStream(HttpRequest request, Gadgeteer.Interfaces.Serial stream)
        {
            _request = request;
            _response = new HttpResponse(stream);
        }

        private HttpRequest _request;
        private HttpResponse _response;

        public HttpResponse Response
        {
            get
            {
                return _response;
            }
        }

        public HttpRequest Request
        {
            get
            {
                return _request;
            }
        }
    }
}
