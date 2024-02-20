using System;
using System.Collections.Generic;
using System.Text;

namespace UtilClasses
{
    public class ComplexStringBuilder
    {
        private int _indent;
        private StringBuilder _sb;
        private StringBuilder _sbBackup;
        private List<string> _items; 
        private bool _inItem;
        private bool _newLine;
        private bool _itemizationStarted;

        public ComplexStringBuilder()
        {
            _sb = new StringBuilder();
            _indent = 0;
            _inItem = false;
            _items = new List<string>();
            _newLine = true;
        }

        private ComplexStringBuilder(StringBuilder sb, int indent) : this()
        {
            _sb = sb;
            _indent = indent;
        }
        public ComplexStringBuilder Append(string s)
        {
            if (_newLine) _sb.Append(' ', _indent*2);
            _newLine = false;
            _sb.Append(s);
            return this;
        }

        public ComplexStringBuilder Append(char c)
        {
            if (_newLine) _sb.Append(' ', _indent * 2);
            _newLine = false;
            _sb.Append(c);
            return this;
        }

        public ComplexStringBuilder AppendLine()
        {
            _newLine = true;
            _sb.AppendLine();
            return this;
        }
        public ComplexStringBuilder AppendLine(string s)
        {
            if (_newLine) _sb.Append(' ', _indent * 2);
            _newLine = true;
            _sb.AppendLine(s);
            return this;
        }

        public ComplexStringBuilder StartItemization()
        {
            if(_itemizationStarted) throw new AccessViolationException();
            _sbBackup = _sb;
            _sb = new StringBuilder();
            _itemizationStarted = true;
            return this;
        }

        public ComplexStringBuilder BeginItem()
        {
            if (!_itemizationStarted) throw new AccessViolationException();
            _inItem = true;
            return this;
        }

        public ComplexStringBuilder EndItem()
        {
            if (!_itemizationStarted) throw new AccessViolationException();
            if (!_inItem) throw new AccessViolationException();
            if (_sb.Length > 0)
            {
                _items.Add(_sb.ToString().TrimEnd());
                _sb.Clear();
            }
            _inItem = false;
            return this;
        }

        public ComplexStringBuilder EndItemization(string separator=",\r\n")
        {
            if (!_itemizationStarted) throw new AccessViolationException();
            if(_inItem) EndItem();
            _sb = _sbBackup;
            _sb.Append(string.Join(separator, _items));
            _items.Clear();
            _itemizationStarted = false;
            return this;
        }

        public ComplexStringBuilder GetIndented()
        {
            return new ComplexStringBuilder(_sb, _indent+1);
        }

        public override string ToString()
        {
            return _sb.ToString();
        }
    }
}
