/// Andl is A New Data Language. See http://andl.org.
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
using System.Linq;
using Microsoft.VisualBasic.FileIO;
using System.IO;
using System.Collections.Generic;
using Andl.Common;

namespace AndlEra {
  // kinds of supported relation stream
  public enum SourceKind {
    Csv, Text, Sql, Odbc, Oledb
  }

  // kinds of supported relation stores
  public enum StoreKind {
    Local, Sqlite, Postgres
  }

  //////////////////////////////////////////////////////////////////////////////
  /// <summary>
  /// Generic source of relational data
  /// </summary>
  internal abstract class DataSourceStream {
    //internal CommonField[] Fields { get; set; }
    internal string Table { get { return _table; } }

    public CommonHeading Heading { get; internal set; }

    // peek the file and get the heading: ordered pairs of label and common type
    internal abstract bool SelectTable(string table);
    // read the file using the heading, return ordered array of objects of correct type
    public abstract IEnumerator<CommonRow> GetEnumerator();

    protected string _connector;
    protected string _ext;
    protected bool _hasid = false;
    protected string _table;

    // Create an input source of given type and location
    // The locator argument is a path or connection string. The actual filename or table name comes later.
    internal static DataSourceStream Create(SourceKind source, string locator) {
      switch (source) {
      case SourceKind.Csv:
        return DataSourceCsv.Create(locator);
      case SourceKind.Text:
        return DataSourceFile.Create(locator);
      case SourceKind.Sql:
        return DataSourceSql.Create(locator);
      case SourceKind.Oledb:
        return DataSourceOleDb.Create(locator);
      case SourceKind.Odbc:
        return DataSourceOdbc.Create(locator);
      default:
        throw Error.Argument($"bad source: {source}");
      }
    }

    // Allow heading to be set as a string "name:type,name:type..."
    internal void SetHeading(string heading) {
      Heading = CommonHeading.Create(heading);
    }

    // service function
    protected string GetPath() {
      if (Path.GetExtension(_table) == String.Empty && _ext != null)
        return Path.Combine(_connector, _table + _ext);
      else return Path.Combine(_connector, _table);
    }
  }

  //////////////////////////////////////////////////////////////////////////////
  /// <summary>
  /// Source that reads CSV files
  /// </summary>
  internal class DataSourceCsv : DataSourceStream {
    internal static DataSourceCsv Create(string locator) {
      return new DataSourceCsv {
        _connector = locator,
        _ext = ".csv",
      };
    }

    internal override bool SelectTable(string table) {
      _table = table;
      var path = GetPath();
      if (!File.Exists(path)) return false;
      using (var rdr = new TextFieldParser(path) {
        TextFieldType = FieldType.Delimited,
        Delimiters = new string[] { "," },
      }) {
        var row = rdr.ReadFields();
        if (_hasid)
          row = (new string[] { "Id:number" })
            .Concat(row).ToArray();
        var fields = row.Select(r => new CommonField { Name = r, CType = CommonType.Text }).ToArray();
        Heading = CommonHeading.Create(fields);
      }
      return true;
    }

    public override IEnumerator<CommonRow> GetEnumerator() {
      var path = GetPath();
      if (!File.Exists(path)) throw Error.Fatal("no such file {path}");
      using (var rdr = new TextFieldParser(path) {
        TextFieldType = FieldType.Delimited,
        Delimiters = new string[] { "," },
      }) {
        for (var id = 0; !rdr.EndOfData; ++id) {
          var row = rdr.ReadFields();
          if (id > 0) {
            if (_hasid)
              row = (new string[] { id.ToString() })
                .Concat(row).ToArray();
            yield return CommonConverter.ToObject(row, Heading.Fields);
          }
        }
      }
    }

  }

  //////////////////////////////////////////////////////////////////////////////
  /// <summary>
  /// Source that is a serial file read by line
  /// </summary>
  internal class DataSourceFile : DataSourceStream {
    internal static DataSourceFile Create(string locator) {
      return new DataSourceFile {
        _connector = locator,
        _ext = ".txt",
      };
    }

    internal override bool SelectTable(string table) {
      _table = table;
      if (!File.Exists(GetPath())) return false;
      SetHeading("Line:Text");
      return true;
    }

    public override IEnumerator<CommonRow> GetEnumerator() {
      var path = GetPath();
      if (!File.Exists(path)) yield break;
      using (var rdr = File.OpenText(path)) {
        for (var line = rdr.ReadLine(); line != null; line = rdr.ReadLine()) {
          yield return CommonConverter.ToObject(new string[] { line }, Heading.Fields);
        }
      }
    }
  }
}
