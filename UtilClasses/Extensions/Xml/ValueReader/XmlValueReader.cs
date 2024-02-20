using System.Collections.Generic;
using System.Xml;

namespace UtilClasses.Extensions.Xml.ValueReader
{
    public abstract class XmlValueReader
    {
        protected List<XmlValueReader> _subReaders;

        public delegate void StringFound(string val);

        public event StringFound? OnStringFound;
        private readonly string _key;
        protected bool _inTag;
        protected bool _inParent;
        protected string _parent;
        protected string? _val;

        protected XmlValueReader(string key, string parent, IEnumerable<XmlValueReader>? subReaders)
        {
            _key = key;
            _val = null;
            _inTag = false;
            _inParent = false;
            _parent = parent;
            _subReaders = new List<XmlValueReader>();
            if (null == subReaders)
                return;
            foreach (var sRdr in subReaders)
            {
                _subReaders.Add(sRdr);
                sRdr.OnStringFound += RaiseStringFound;
            }
        }

        protected void RaiseStringFound()
        {
            OnStringFound?.Invoke(_val);
        }

        protected void RaiseStringFound(string val)
        {
            OnStringFound?.Invoke(val);
        }

        public void Update(XmlReader rdr)
        {
            if (!string.IsNullOrWhiteSpace(_parent))
            {
                if (rdr.IsStartElement(_parent)) _inParent = true;
                if (rdr.IsEndElement(_parent)) _inParent = false;
            }

            if (string.IsNullOrWhiteSpace(_parent) || _inParent)
            {
                if (rdr.IsStartElement(_key))
                    _inTag = true;
                if (rdr.IsEndElement(_key))
                    _inTag = false;
            }

            if (!_inTag) return;
            if (_subReaders.Count > 0)
            {
                foreach (var srdr in _subReaders)
                {
                    srdr.Update(rdr);
                }
            }
            else
            {
                _Update(rdr);
            }

            if (_inTag && rdr.IsEmptyElement)
                _inTag = false;
        }

        protected abstract void _Update(XmlReader rdr);

        public string Value => _val;

        public bool Valid => !string.IsNullOrEmpty(_val);
    }
}
