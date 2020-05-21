using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Andl.Common;

namespace AndlEra {
  // tuple type used for all pipeline nodes
  public class TupMin : TupleBase { }

  public class TupSelect : TupleBase {
    public Func<TupleBase, bool> Select { get; set; }
    public TupSelect(Func<TupleBase, bool> select) => Select = select;
  }

  /// <summary>
  /// Pipeline nodes that look like relation values.
  /// 
  /// Nodes carry a heading and a tuple enumerator.
  /// </summary>
  public abstract class RelationNode : IEnumerable<TupMin> {
    public CommonHeading Heading { get; protected set; }

    public abstract IEnumerator<TupMin> GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

    public string Format() {
      return this.Select(t => t.Format(Heading.ToNames())).Join("\n");
    }

    public static RelationNode Import(SourceKind kind, string connector, string name, string heading = null) {
      return new ImportNode(kind, connector, name, heading);
    }

    public RelationNode Project(string newheading) {
      return new UnaryNode(this, newheading, null, null, null);
    }
    public RelationNode Rename(string newheading) {
      return new UnaryNode(this, newheading, null, null, null);
    }
    public RelationNode Select(string heading, TupSelect tupsel) {
      return new SelectNode(this, heading, tupsel);
    }

  }

  /// <summary>
  /// Node that imports tuples from a source
  /// </summary>
  class ImportNode : RelationNode {
    protected RelationStream<TupMin> _source;

    public override IEnumerator<TupMin> GetEnumerator() => _source.GetEnumerator();

    public ImportNode(SourceKind kind, string connector, string name, string heading = null) {
      _source = new RelationStream<TupMin>(kind, connector, name, heading);
      Heading = _source.Heading;
    }
  }

  /// <summary>
  /// Pipeline node that passes tuples through
  /// </summary>
  class UnaryNode : RelationNode {
    RelationNode _source;
    int[] _map;
    bool _dedup;

    public UnaryNode(RelationNode source, string newheading, Func<object, bool>[] extfuncs,
      string selheading, Func<object, bool>[] selfuncs) {
      _source = source;
      Heading = (newheading == null) ? _source.Heading : _source.Heading.Adapt(newheading);
      _map = Heading.CreateMap(_source.Heading);
      _dedup = _map.Length < _source.Heading.Degree || _map.Any(i => i < 0);
    }

    public override IEnumerator<TupMin> GetEnumerator() {
      var hash = new HashSet<TupMin>();
      foreach (var tuple in _source) {
        var newvalues = tuple.MapValues(_map);
        var newtuple = TupleBase.Create<TupMin>(newvalues);
        if (_dedup) {
          if (hash.Contains(newtuple)) continue;
          hash.Add(newtuple);
        }
        yield return newtuple;
      }
    }

    //--- impl
  }

  /// <summary>
  /// Pipeline node that passes tuples through
  /// </summary>
  class SelectNode : RelationNode {
    RelationNode _source;
    Func<TupleBase, bool> _selfunc;
    CommonHeading _selheading;
    int[] _map;

    public SelectNode(RelationNode source, string heading, TupSelect tupsel) {
      _source = source;
      Heading = _source.Heading;
      _selfunc = tupsel.Select;
      _selheading = Heading.Adapt(heading);
      _map = _selheading.CreateMap(Heading);
      if (_map.Any(x => x < 0)) throw Error.Fatal("invalid map, must all match");
    }

    public override IEnumerator<TupMin> GetEnumerator() {
      foreach (var tuple in _source) {
        var newvalues = tuple.MapValues(_map);
        var newtuple = TupleBase.Create<TupMin>(newvalues);
        if (_selfunc(newtuple)) yield return newtuple;
      }
    }
  }
}
