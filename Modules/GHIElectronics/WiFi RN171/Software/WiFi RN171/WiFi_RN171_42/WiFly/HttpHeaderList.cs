using System;
using Microsoft.SPOT;
using System.Collections;

namespace Gadgeteer.Modules.GHIElectronics
{
    public class HttpHeaderList
    {
        private ArrayList _list;

        public HttpHeaderList()
        {
            _list = new ArrayList();
        }

        public override string ToString()
        {
            string header = "";

            foreach (DictionaryEntry entry in _list)
            {
                if ((string)entry.Key != "Request" && (string)entry.Key != "Status")
                    header += (string)entry.Key + ": " + (string)entry.Value + "\r\n";
            }

            if(this["Request"] != "")
                header = this["Request"] + header + "\r\n";
            else if (this["Status"] != "")
                header = this["Status"] + header + "\r\n";

            return header;
        }

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
