using System;
using Microsoft.SPOT;
using System.IO.Ports;

namespace Gadgeteer.Modules.GHIElectronics
{
    public class HttpResponse
    {
        private ResponseStatus _status;

        public enum ResponseStatus
        {
            //Informational Header Responses
            Continue = 100,
            SwitchingProtocols = 101,

            //Successful Header Responses
            OK = 200,
            Created = 201,
            Accepted = 202,
            NonAuthoritativeInformation = 203,
            NoContent = 204,
            ResetContent = 205,

            //Client Errors
            BadRequest = 400,
            Unauthorized = 401,
            /* Reserved for future use: Payment Required = 402, */
            Forbidden = 403,
            NotFound = 404,
            MethodNotAllowed = 405,
            NotAcceptable = 406,
            ProxyAuthenticationRequired = 407,
            RequestTimeout = 408,
            Conflict = 409,
            Gone = 410,
            LengthRequired = 411,
            PreconditionFailed = 412,
            RequestEntityTooLarge = 413,
            RequestUriTooLong = 414,
            UnsupportedMediaType = 415,
            RequestedRangeNotSatisfiable = 416,
            ExpectationFailed = 417,

            //Server Errors
            InternalServerError = 500,
            NotImplemented = 501,
            BadGateway = 502,
            ServiceUnavailable = 503,
            GatewayTimeout = 504,
            HTTPVersionNotSupported = 505
        }

        private Gadgeteer.Interfaces.Serial _stream;
        public HttpHeaderList HeaderData;

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

        public HttpResponse(Gadgeteer.Interfaces.Serial stream)
        {
            if (stream == null)
                throw new ArgumentNullException();

            _stream = stream;

            HeaderData = new HttpHeaderList();
        }

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
