using System;
using Microsoft.SPOT;
using System.IO.Ports;

namespace Gadgeteer.Modules.GHIElectronics
{
	/// <summary>
	/// Represents an HTTP request.
	/// </summary>
    public class HttpRequest
	{
		/// <summary>
		/// Represents an HTTP request type.
		/// </summary>
        public enum HttpRequestType
        {
			/// <summary>
			/// A GET request.
			/// </summary>
            GET,
			/// <summary>
			/// A POST request.
			/// </summary>
            POST
        }

		/// <summary>
		/// The request type.
		/// </summary>
        public HttpRequestType RequestType;

		/// <summary>
		/// The header data.
		/// </summary>
        public HttpHeaderList HeaderData;

		/// <summary>
		/// The query data.
		/// </summary>
        public QueryDataList QueryData;
		
		/// <summary>
		/// The URL.
		/// </summary>
        public string URL = "";

		/// <summary>
		/// The posted data.
		/// </summary>
        public string PostData = "";

		/// <summary>
		/// The length of the posted data.
		/// </summary>
        public int PostLength = 0;

		/// <summary>
		/// Whether or not there was posted data.
		/// </summary>
        public bool ContainsPostData
        {
            get
            {
                return (PostData.Length > 0);
            }
        }

		/// <summary>
		/// Constructs a new request.
		/// </summary>
        public HttpRequest()
        {
            HeaderData = new HttpHeaderList();
            QueryData = new QueryDataList();
        }
    }
}
