using System;
using System.Collections.Generic;
using System.Linq;

namespace MemcachedSharp
{
    struct ResponseLine
    {
        static readonly HashSet<string> _errorResponses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ERROR",
            "CLIENT_ERROR",
            "SERVER_ERROR",
        };
        static readonly char[] _splitChars = new[] { ' ' };

        private readonly string _line;
        private readonly string[] _parts;

        public ResponseLine(string line)
        {
            _line = line;
            _parts = line.Split(_splitChars);
        }

        public string Line
        {
            get { return _line; }
        }

        public string[] Parts
        {
            get { return _parts; }
        }

        public void Validate()
        {
            if (_parts.Length == 0 || _errorResponses.Contains(_parts[0]))
            {
                throw new MemcachedException(_line);
            }
        }

        public override string ToString()
        {
            return _line;
        }
    }
}
