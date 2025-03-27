using System;
using System.Data;

namespace HashAG
{
    internal class SqlCeConnection
    {
        private string connectString;

        public SqlCeConnection(string connectString)
        {
            this.connectString = connectString;
        }

        public ConnectionState State { get; internal set; }

        internal void Close()
        {
            throw new NotImplementedException();
        }

        internal void Open()
        {
            throw new NotImplementedException();
        }
    }
}