using System;
using Microsoft.SPOT;
using System.IO.Ports;

namespace Gadgeteer.Modules.GHIElectronics
{
	/// <summary>
	/// An HTTP response.
	/// </summary>
    public class HttpResponse
    {
        private ResponseStatus _status;

		/// <summary>
		/// The response status.
		/// </summary>
        public enum ResponseStatus
		{
			//Informational Header Responses

			/// <summary>
			/// The Continue status
			/// </summary>
			Continue = 100,
			/// <summary>
			/// The SwitchingProtocols status
			/// </summary>
            SwitchingProtocols = 101,

			//Successful Header Responses
			/// <summary>
			/// The OK status
			/// </summary>
			OK = 200,
			/// <summary>
			/// The Created status
			/// </summary>
			Created = 201,
			/// <summary>
			/// The Accepted status
			/// </summary>
			Accepted = 202,
			/// <summary>
			/// The NonAuthoritativeInformation status
			/// </summary>
			NonAuthoritativeInformation = 203,
			/// <summary>
			/// The NoContent status
			/// </summary>
			NoContent = 204,
			/// <summary>
			/// The ResetContent status
			/// </summary>
            ResetContent = 205,

			//Client Errors
			/// <summary>
			/// The BadRequest status
			/// </summary>
			BadRequest = 400,
			/// <summary>
			/// The Unauthorized status
			/// </summary>
			Unauthorized = 401,
			/* Reserved for future use: Payment Required = 402, */
			/// <summary>
			/// The Forbidden status
			/// </summary>
			Forbidden = 403,
			/// <summary>
			/// The NotFound status
			/// </summary>
			NotFound = 404,
			/// <summary>
			/// The MethodNotAllowed status
			/// </summary>
			MethodNotAllowed = 405,
			/// <summary>
			/// The NotAcceptable status
			/// </summary>
			NotAcceptable = 406,
			/// <summary>
			/// The ProxyAuthenticationRequired status
			/// </summary>
			ProxyAuthenticationRequired = 407,
			/// <summary>
			/// The RequestTimeout status
			/// </summary>
			RequestTimeout = 408,
			/// <summary>
			/// The Conflict status
			/// </summary>
			Conflict = 409,
			/// <summary>
			/// The Gone status
			/// </summary>
			Gone = 410,
			/// <summary>
			/// The LengthRequired status
			/// </summary>
			LengthRequired = 411,
			/// <summary>
			/// The PreconditionFailed status
			/// </summary>
			PreconditionFailed = 412,
			/// <summary>
			/// The RequestEntityTooLarge status
			/// </summary>
			RequestEntityTooLarge = 413,
			/// <summary>
			/// The RequestUriTooLong status
			/// </summary>
			RequestUriTooLong = 414,
			/// <summary>
			/// The UnsupportedMediaType status
			/// </summary>
			UnsupportedMediaType = 415,
			/// <summary>
			/// The RequestedRangeNotSatisfiable status
			/// </summary>
			RequestedRangeNotSatisfiable = 416,
			/// <summary>
			/// The ExpectationFailed status
			/// </summary>
            ExpectationFailed = 417,

			//Server Errors
			/// <summary>
			/// The InternalServerError status
			/// </summary>
			InternalServerError = 500,
			/// <summary>
			/// The NotImplemented status
			/// </summary>
			NotImplemented = 501,
			/// <summary>
			/// The BadGateway status
			/// </summary>
			BadGateway = 502,
			/// <summary>
			/// The ServiceUnavailable status
			/// </summary>
			ServiceUnavailable = 503,
			/// <summary>
			/// The GatewayTimeout status
			/// </summary>
			GatewayTimeout = 504,
			/// <summary>
			/// The HTTPVersionNotSupported status
			/// </summary>
            HTTPVersionNotSupported = 505
        }

        private Gadgeteer.Interfaces.Serial _stream;
		/// <summary>
		/// The header data of the response.
		/// </summary>
        public HttpHeaderList HeaderData;

		/// <summary>
		/// The status code of the reponse.
		/// </summary>
        public ResponseStatus StatusCode
        {
            get
            {
                return _status;
            }
            set
            {
                _status = ResponseStatus.OK;
                HeaderData["Status"] = "HTTP/1.1 " + GetResponseText(_status);
            }
        }

		/// <summary>
		/// Constructs a new response from a stream.
		/// </summary>
		/// <param name="stream">The stream to use.</param>
        public HttpResponse(Gadgeteer.Interfaces.Serial stream)
        {
            if (stream == null)
                throw new ArgumentNullException();

            _stream = stream;

            HeaderData = new HttpHeaderList();
        }

		/// <summary>
		/// Sends a response.
		/// </summary>
		/// <param name="document">The body of the response.</param>
        public void Send(byte[] document)
        {
            byte[] header = System.Text.Encoding.UTF8.GetBytes(this.HeaderData.ToString());

            _stream.Write(header, 0, header.Length);

            _stream.Write(document, 0, document.Length);
        }

        private string GetResponseText(ResponseStatus status)
        {
            string text = "";

            switch (status)
            {
                case ResponseStatus.Accepted:
                    text = "202 Accepted";
                    break;

                case ResponseStatus.BadGateway:
                    text = "502 Bad Gateway";
                    break;

                case ResponseStatus.BadRequest:
                    text = "400 Bad Gateway";
                    break;

                case ResponseStatus.Conflict:
                    text = "409 Conflict";
                    break;

                case ResponseStatus.Continue:
                    text = "100 Continue";
                    break;

                case ResponseStatus.Created:
                    text = "201 Created";
                    break;

                case ResponseStatus.ExpectationFailed:
                    text = "417 Expectation Fail";
                    break;

                case ResponseStatus.Forbidden:
                    text = "403 Forbidden";
                    break;

                case ResponseStatus.GatewayTimeout:
                    text = "504 Gateway Timeout";
                    break;

                case ResponseStatus.Gone:
                    text = "410 Gone";
                    break;

                case ResponseStatus.HTTPVersionNotSupported:
                    text = "505 HTTP Version Not Supported";
                    break;

                case ResponseStatus.InternalServerError:
                    text = "500 Internal Server Error";
                    break;

                case ResponseStatus.LengthRequired:
                    text = "411 Length Required";
                    break;

                case ResponseStatus.MethodNotAllowed:
                    text = "405 Method Not Allowed";
                    break;

                case ResponseStatus.NoContent:
                    text = "204 No Content";
                    break;

                case ResponseStatus.NonAuthoritativeInformation:
                    text = "203 Non-Authoritative Information";
                    break;

                case ResponseStatus.NotAcceptable:
                    text = "406 Not Acceptable";
                    break;

                case ResponseStatus.NotFound:
                    text = "404 Not Found";
                    break;

                case ResponseStatus.NotImplemented:
                    text = "501 Not Implemented";
                    break;

                case ResponseStatus.OK:
                    text = "200 OK";
                    break;

                case ResponseStatus.PreconditionFailed:
                    text = "412 Precondition Failed";
                    break;

                case ResponseStatus.ProxyAuthenticationRequired:
                    text = "407 Proxy Authentication Required";
                    break;

                case ResponseStatus.RequestedRangeNotSatisfiable:
                    text = "416 Requested Range Not Satisfiable";
                    break;

                case ResponseStatus.RequestEntityTooLarge:
                    text = "413 Request Entity Too Large";
                    break;

                case ResponseStatus.RequestTimeout:
                    text = "408 Request Timeout";
                    break;

                case ResponseStatus.RequestUriTooLong:
                    text = "413 Request Entity Too Large";
                    break;

                case ResponseStatus.ResetContent:
                    text = "205 Reset Content";
                    break;

                case ResponseStatus.ServiceUnavailable:
                    text = "503 Service Unavailable";
                    break;

                case ResponseStatus.SwitchingProtocols:
                    text = "101 Switching Protocols";
                    break;

                case ResponseStatus.Unauthorized:
                    text = "401 Unauthorized";
                    break;

                case ResponseStatus.UnsupportedMediaType:
                    text = "415 Unsupported Media Type";
                    break;
            }

            return text;
        }
    }
}
