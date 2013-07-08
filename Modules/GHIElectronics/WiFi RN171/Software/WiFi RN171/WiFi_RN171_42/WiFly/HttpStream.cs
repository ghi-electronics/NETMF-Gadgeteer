using System;
using System.IO;

using Microsoft.SPOT.IO;
using Microsoft.SPOT;
using System.IO.Ports;

namespace Gadgeteer.Modules.GHIElectronics
{
	/// <summary>
	/// Represents an HTTP stream.
	/// </summary>
    public class HttpStream
    {
		/// <summary>
		/// Constructs a new HTTPStream.
		/// </summary>
		/// <param name="request">The http request the stream will represent.</param>
		/// <param name="stream">The stream of serial data.</param>
        public HttpStream(HttpRequest request, Gadgeteer.Interfaces.Serial stream)
        {
            _request = request;
            _response = new HttpResponse(stream);
        }

        private HttpRequest _request;
        private HttpResponse _response;

		/// <summary>
		/// The response.
		/// </summary>
        public HttpResponse Response
        {
            get
            {
                return _response;
            }
        }

		/// <summary>
		/// The request.
		/// </summary>
        public HttpRequest Request
        {
            get
            {
                return _request;
            }
        }
    }
}
