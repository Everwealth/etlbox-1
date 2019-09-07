﻿using System.Data;
using System.Data.Odbc;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;

namespace ALE.ETLBox.ConnectionManager {
    /// <summary>
    /// Sql Connection manager for an ODBC connection based on ADO.NET to Sql Server.
    /// ODBC by default does not support a Bulk Insert - inserting big amounts of data is translated into a
    /// <code>
    /// insert into (...) values (..),(..),(..) statementes.
    /// </code>
    /// Be careful with the batch size - some databases have limitations regarding the length of sql statements.
    /// Reduce the batch if encounter issues here.
    /// </summary>
    /// <example>
    /// <code>
    /// ControlFlow.CurrentDbConnection =
    ///   new OdbcConnectionManager(new ObdcConnectionString(
    ///     "Driver={SQL Server};Server=.;Database=ETLBox;Trusted_Connection=Yes;"));
    /// </code>
    /// </example>
    public class SqlOdbcConnectionManager : OdbcConnectionManager{
        public SqlOdbcConnectionManager() : base() { }

        public SqlOdbcConnectionManager(OdbcConnectionString connectionString) : base(connectionString) { }

        public override IDbConnectionManager Clone()
        {
            SqlOdbcConnectionManager clone = new SqlOdbcConnectionManager((OdbcConnectionString)ConnectionString)
            {
                MaxLoginAttempts = this.MaxLoginAttempts
            };
            return clone;
        }
    }
}
