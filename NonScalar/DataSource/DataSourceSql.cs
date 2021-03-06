﻿/// Andl is A New Data Language. See http://andl.org.
///
/// Copyright © David M. Bennett 2015-20 as an unpublished work. All rights reserved.
/// 
/// This work is licensed under the Creative Commons Attribution-NonCommercial 4.0 International License. 
/// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc/4.0/.
/// 
/// In summary, you are free to share and adapt this software freely for any non-commercial purpose provided 
/// you give due attribution and do not impose additional restrictions.
/// 
/// This software is provided in the hope that it will be useful, but with 
/// absolutely no warranties. You assume all responsibility for its use.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Data.Odbc;
using System.Data.Common;
using Andl.Common;

namespace AndlEra {
  //////////////////////////////////////////////////////////////////////////////
  /// <summary>
  /// Base class for SQL readers providing common code
  /// </summary>
  internal abstract class DataSourceSqlBase : DataSourceStream, IDisposable {
    protected Dictionary<string, CommonType> _convdict;

    // open a table and return a reader
    abstract protected DbDataReader Open(string table);
    // close the table
    abstract protected void Close();
    // get schema info
    abstract protected DataTable GetSchema();

    // Generic input
    internal override bool SelectTable(string table) {
      //Logger.WriteLine(2, "Sql Peek '{0}'", table);
      if (table == null) return SelectSchema();
      using (var reader = Open(table)) {
        if (reader == null) return false;
        _table = table;
        //Logger.WriteLine(3, "Table {0} fields {1}", table, String.Join(",", s));
        var fields = Enumerable.Range(0, reader.FieldCount)
          .Where(x => _convdict.ContainsKey(reader.GetDataTypeName(x)))
          .Select(x => new CommonField {
            Name = reader.GetName(x), CType = _convdict[reader.GetDataTypeName(x)]
          }).ToArray();
        Heading = CommonHeading.Create(fields);
      }
      Close();
      return true;
    }

    // select the schema as the current table
    bool SelectSchema() {
      _table = null;
      var schema = GetSchema();
      var scols = schema.Columns;
      var fields = Enumerable.Range(0, scols.Count)
        .Select(x => new CommonField {
          Name = scols[x].ColumnName, CType = CommonConverter.TypeToCommon(scols[x].DataType)
        }).ToArray();
      Heading = CommonHeading.Create(fields);
      return true;
    }

    // choose which enumerator
    // note: cannot mix function return and yield return in one method
    public override IEnumerator<CommonRow> GetEnumerator() {
      return (_table == null) ? GetSchemaEnumerator() : GetDataEnumerator();
    }

    // enumerate over data from table
    internal IEnumerator<CommonRow> GetDataEnumerator() {
      //Logger.WriteLine(2, "Sql Read '{0}'", table);
      using (var reader = Open(_table)) {
        while (reader.Read()) {
          var values = Heading.Fields.Select(c => GetReaderValue(reader, c.Name, c.CType)).ToArray();
          yield return new CommonRow(values);
        }
      }
      Close();
    }

    // enumerate over schema rows to match heading
    internal IEnumerator<CommonRow> GetSchemaEnumerator() {
      var schema = GetSchema();
      foreach (DataRow row in schema.Rows) {
        var values = Heading.Fields.Select(c => GetDatarowValue(row, c.Name, c.CType)).ToArray();
        yield return new CommonRow(values);
      }
    }

    // extract reader value in common type
    object GetReaderValue(DbDataReader rdr, string fieldname, CommonType ctype) {
      var field = rdr.GetOrdinal(fieldname);
      if (rdr.IsDBNull(field))
        return CommonConverter.GetDefault(ctype);
      else return rdr.GetValue(field);
    }

    // extract value from row in common type
    object GetDatarowValue(DataRow row, string fieldname, CommonType ctype) {
      try {
        var value = row[fieldname];
        return (value == null || value == DBNull.Value) ? CommonConverter.GetDefault(ctype) : value;
      } catch(Exception) {
        throw Error.Fatal($"bad field {fieldname}");
      }
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing) {
      if (!disposedValue) {
        if (disposing) {
          Close();
        }
        disposedValue = true;
      }
    }

    // This code added to correctly implement the disposable pattern.
    public void Dispose() {
      Dispose(true);
    }
    #endregion

  }

  //////////////////////////////////////////////////////////////////////////////
  /// <summary>
  /// Subclass for Sql source type
  /// </summary>
  internal class DataSourceSql : DataSourceSqlBase {
    SqlConnection _connection;

    internal static DataSourceSql Create(string locator) {
      var ds = new DataSourceSql {
        _connector = locator,
      };
      try {
        ds._connection = new SqlConnection(locator);
      } catch(Exception ex) {
        throw Error.Fatal($"Sql: {ex.Message}");
      }
      ds._convdict = new Dictionary<string, CommonType> {
        { "char", CommonType.Text },
        { "varchar", CommonType.Text },
        { "nchar", CommonType.Text },
        { "nvarchar", CommonType.Text },
        { "text", CommonType.Text },
        { "bit", CommonType.Bool },
        { "int", CommonType.Integer },
        { "bigint", CommonType.Integer },
        { "smallint", CommonType.Integer },
        { "tinyint", CommonType.Integer },
        { "numeric", CommonType.Number },
        { "decimal", CommonType.Number },
        { "money", CommonType.Number },
        { "smallmoney", CommonType.Number },
        { "date", CommonType.Time },
        { "datetime", CommonType.Time },
        { "time", CommonType.Time },
        { "datetime2", CommonType.Time },
        { "smalldatetime", CommonType.Time },
        { "datetimeoffset", CommonType.Time },
      };

      return ds;
    }

    protected override DbDataReader Open(string table) {
      var cmd = new SqlCommand(String.Format("select * from [{0}]", table), _connection);
      try {
        _connection.Open();
      } catch (Exception ex) {
        throw Error.Fatal($"Sql: {ex.Message}");
      }
      return cmd.ExecuteReader();
    }

    protected override void Close() {
      _connection.Close();
    }

    protected override DataTable GetSchema() {
      _connection.Open();
      var ret = _connection.GetSchema("tables");
      _connection.Close();
      return ret;
    }
  }

  //////////////////////////////////////////////////////////////////////////////
  /// <summary>
  /// Subclass for ODBC source type. 
  /// </summary>
  internal class DataSourceOdbc : DataSourceSqlBase {
    OdbcConnection _connection;

    // Factory method
    internal static DataSourceOdbc Create(string locator) {
      var ds = new DataSourceOdbc {
        _connector = locator,
      };
      try {
        ds._connection = new OdbcConnection(locator);
      } catch (Exception ex) {
        throw Error.Fatal($"Sql: {ex.Message}");
      }
      ds._convdict = new Dictionary<string, CommonType> {
        { "CHAR", CommonType.Text },
        { "VARCHAR", CommonType.Text },
        { "NCHAR", CommonType.Text },
        { "NVARCHAR", CommonType.Text },
        { "INTEGER", CommonType.Integer },
      };
      return ds;
    }

    //
    protected override DbDataReader Open(string table) {
      var cmd = new OdbcCommand(String.Format("select * from {0}", table), _connection);
      try {
        _connection.Open();
      } catch (Exception ex) {
        throw Error.Fatal($"Source Odbc {ex.Message}");
      }
      return cmd.ExecuteReader();
    }

    protected override void Close() {
      _connection.Close();
    }

    protected override System.Data.DataTable GetSchema() {
      _connection.Open();
      var ret = _connection.GetSchema("tables");
      _connection.Close();
      return ret;
    }
  }

  //////////////////////////////////////////////////////////////////////////////
  /// <summary>
  /// Subclass for Oledb source type. Can read Access database.
  /// </summary>
  internal class DataSourceOleDb : DataSourceSqlBase {
    OleDbConnection _connection;

    // Factory method
    internal static DataSourceOleDb Create(string locator) {
      var ds = new DataSourceOleDb {
        _connector = locator,
      };
      try {
        ds._connection = new OleDbConnection(locator);
      } catch (Exception ex) {
        throw Error.Fatal($"Source OleDb: {ex.Message}");
      }
      ds._convdict = new Dictionary<string, CommonType> {
        { "DBTYPE_BOOL", CommonType.Bool },
        { "DBTYPE_I4", CommonType.Integer },
        { "DBTYPE_DATE", CommonType.Time },
        { "DBTYPE_WVARCHAR", CommonType.Text },
        { "DBTYPE_WVARLONGCHAR", CommonType.Text },
      };
      return ds;
    }

    //
    protected override DbDataReader Open(string table) {
      var cmd = new OleDbCommand(String.Format("select * from {0}", table), _connection);
      try {
        _connection.Open();
      } catch (Exception ex) {
        throw Error.Fatal($"Source OleDb: {ex.Message}");
      }
      return cmd.ExecuteReader();
    }

    protected override void Close() {
      _connection.Close();
    }

    protected override DataTable GetSchema() {
      _connection.Open();
      var ret = _connection.GetSchema("tables");
      _connection.Close();
      return ret;
    }
  }

}
