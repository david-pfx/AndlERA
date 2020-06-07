using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Andl.Common;

namespace AndlEra {
  ///==========================================================================
  /// <summary>
  /// External stream source for tuples
  /// </summary>
  public class RelationStream<T> : IEnumerable<T>
  where T : TupBase,new() {

    public string TableName { get { return _stream.Table; } }

    public CommonHeading Heading { get { return _stream.Heading; } }

    DataSourceStream _stream;

    //--- ctor
    // Note importance of passing type through to base
    public RelationStream(SourceKind kind, string connector, string name, string heading = null) {
      if (connector == null) throw Error.NullArg("connector");
      _stream = DataSourceStream.Create(kind, connector);
      if (_stream == null) throw Error.Fatal($"cannot connect to '{kind}' with '{connector}'");
      if (!_stream.SelectTable(name)) throw Error.Fatal($"cannot find '{name}' on '{connector}'");
      if (heading != null)
        _stream.SetHeading(heading);
    }

    // implement interface
    public IEnumerator<T> GetEnumerator() {
      foreach (var item in _stream) {
        yield return new T().Init<T>(item.Values);
      }
    }

    // gotta be here, never figured out why
    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }
  }
}
