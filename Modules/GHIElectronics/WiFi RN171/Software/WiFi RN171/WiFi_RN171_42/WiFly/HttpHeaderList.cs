using System;
using Microsoft.SPOT;
using System.Collections;

namespace Gadgeteer.Modules.GHIElectronics
{
	/// <summary>
	/// Represents the HTTP headers.
	/// </summary>
    public class HttpHeaderList
    {
        private ArrayList _list;

		/// <summary>
		/// Constructs a new instance.
		/// </summary>
        public HttpHeaderList()
        {
            _list = new ArrayList();
        }

		/// <summary>
		/// Returns a string representing this instance.
		/// </summary>
		/// <returns>A string representing this instance.</returns>
        public override string ToString()
        {
            string header = "";

            foreach (DictionaryEntry entry in _list)
            {
                if ((string)entry.Key != "Request" && (string)entry.Key != "Status")
                    header += (string)entry.Key + ": " + (string)entry.Value + "\r\n";
            }

            if(this["Request"] != "")
                header = this["Request"] + "\r\n" + header + "\r\n";
            else if (this["Status"] != "")
                header = this["Status"] + "\r\n" + header + "\r\n";

            return header;
        }

		/// <summary>
		/// Gets or sets one of the headers.
		/// </summary>
		/// <param name="i">The index of the header.</param>
		/// <returns>The header value at the index.</returns>
        public string this[string i]
        {
            get
            {
                foreach (DictionaryEntry entry in _list)
                {
                    if ((string)entry.Key == i)
                        return (string)entry.Value;
                }

                return "";
            }

            set
            {
                bool found = false;

                foreach (DictionaryEntry entry in _list)
                {
                    if ((string)entry.Key == i)
                    {
                        entry.Value = value;
                        found = true;
                    }
                }

                if (!found)
                {
                    _list.Add(new DictionaryEntry(i, value));
                }
            }
        }

		/// <summary>
		/// Gets or sets one of the headers.
		/// </summary>
		/// <param name="i">The index of the header.</param>
		/// <returns>The header value at the index.</returns>
        public string this[int i]
        {
            get
            {
                return (string)_list[i];
            }

            set
            {
                _list[i] = value;
            }
        }
    }
}
