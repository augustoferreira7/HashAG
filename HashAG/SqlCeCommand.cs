using System;

namespace HashAG
{
    internal class SqlCeCommand
    {
        private string sql;
        private SqlCeConnection cn;

        public SqlCeCommand(string sql, SqlCeConnection cn)
        {
            this.sql = sql;
            this.cn = cn;
        }

        public object Parameters { get; internal set; }

        internal void ExecuteNonQuery()
        {
            throw new NotImplementedException();
        }
    }
}