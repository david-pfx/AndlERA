using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Andl.Common;

namespace AndlEra {
  // set operations
  enum SetOp {
    Union, Minus, Intersect,
  }

  // tuple type used for all pipeline nodes
  public class TupMin : TupleBase { }

  public class TupSelect : TupleBase {
    public Func<TupleBase, bool> Select { get; set; }
    public TupSelect(Func<TupleBase, bool> select) => Select = select;
  }

  public class TupExtend : TupleBase {
    public Func<TupleBase, object> Extend { get; set; }
    public TupExtend(Func<TupleBase, object> extend) => Extend = extend;
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
      return new UnaryNode(this, newheading, true);
    }
    public RelationNode Rename(string newheading) {
      return new UnaryNode(this, newheading, false);
    }
    public RelationNode Select(string heading, TupSelect tupsel) {
      return new SelectNode(this, heading, tupsel);
    }
    public RelationNode Extend(string heading, TupExtend tupext) {
      return new ExtendNode(this, heading, tupext);
    }
    public RelationNode Union(RelationNode other) {
      return new SetOperationNode(this, SetOp.Union, other);
    }
    public RelationNode Minus(RelationNode other) {
      return new SetOperationNode(this, SetOp.Minus, other);
    }
    public RelationNode Intersect(RelationNode other) {
      return new SetOperationNode(this, SetOp.Intersect, other);
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
    bool _isproject;

    public UnaryNode(RelationNode source, string newheading, bool isproject) {
      _source = source;
      Heading = _source.Heading.Adapt(newheading);
      _map = Heading.CreateMap(_source.Heading, !isproject);
      if (isproject != (Heading.Degree < _source.Heading.Degree)) throw Error.Fatal("too many attributes for project");
      if (_map.Any(x => x < 0)) throw Error.Fatal("headings do not match");
      _isproject = isproject;
    }

    public override IEnumerator<TupMin> GetEnumerator() {
      var hash = new HashSet<TupMin>();
      foreach (var tuple in _source) {
        var newvalues = tuple.MapValues(_map);
        var newtuple = TupleBase.Create<TupMin>(newvalues);
        if (_isproject) {
          if (hash.Contains(newtuple)) continue;
          hash.Add(newtuple);
        }
        yield return newtuple;
      }
    }
  }

  /// <summary>
  /// Select function node
  /// Heading must be subset of other
  /// Function has arguments matching heading, return bool
  /// </summary>
  class SelectNode : RelationNode {
    RelationNode _source;
    Func<TupleBase, bool> _selfunc;
    CommonHeading _selheading;
    int[] _selmap;

    public SelectNode(RelationNode source, string heading, TupSelect tupsel) {
      _source = source;
      Heading = _source.Heading;
      _selfunc = tupsel.Select;
      _selheading = Heading.Adapt(heading);
      _selmap = _selheading.CreateMap(Heading);
      if (_selmap.Any(x => x < 0)) throw Error.Fatal("invalid map, must all match");
    }

    public override IEnumerator<TupMin> GetEnumerator() {
      foreach (var tuple in _source) {
        var newvalues = tuple.MapValues(_selmap);
        var newtuple = TupleBase.Create<TupMin>(newvalues);
        if (_selfunc(newtuple)) yield return newtuple;
      }
    }
  }

  /// <summary>
  /// Extend function node
  /// Heading excluding last must be subset of other
  /// Function has arguments matching heading excluding last, return matches last
  /// </summary>
  class ExtendNode : RelationNode {
    RelationNode _source;
    Func<TupleBase, object> _extfunc;
    CommonHeading _heading;
    CommonHeading _argheading;
    int[] _argmap;

    public ExtendNode(RelationNode source, string heading, TupExtend tupsel) {
      _source = source;
      _heading = _source.Heading.Adapt(heading);
      _extfunc = tupsel.Extend;
      Heading = CommonHeading.Create(_source.Heading.Fields.Union(_heading.Fields));
      _argheading = CommonHeading.Create(_heading.Fields.Take(_heading.Fields.Length - 1));
      _argmap = _argheading.CreateMap(_source.Heading);
      if (_argmap.Any(x => x < 0)) throw Error.Fatal("invalid map, must all match");
    }

    public override IEnumerator<TupMin> GetEnumerator() {
      foreach (var tuple in _source) {
        // pick out the argument values, pass them to the function, get return
        var argvalues = tuple.MapValues(_argmap);
        var argtuple = TupleBase.Create<TupMin>(argvalues);
        var result = _extfunc(argtuple);

        // append return to original tuple as final result
        var newvalues = tuple.Values.Concat(new object[] { result }).ToArray();
        var newtuple = TupleBase.Create<TupMin>(newvalues);
        yield return newtuple;
      }
    }
  }

  /// <summary>
  /// implement set operations
  /// </summary>
  class SetOperationNode : RelationNode {
    RelationNode _left;
    RelationNode _right;
    SetOp _setop;
    int[] _othermap;

    public SetOperationNode(RelationNode left, SetOp setop, RelationNode right) {
      _setop = setop;
      _left = left;
      _right = right;
      Heading = left.Heading;
      _othermap = Heading.CreateMap(right.Heading);
      if (_othermap.Length != Heading.Degree
        || _othermap.Any(x => x < 0)) throw Error.Fatal("invalid heading, must be the same");
    }

    public override IEnumerator<TupMin> GetEnumerator() {
      var hash = new HashSet<TupMin>();
      switch (_setop) {
      case SetOp.Union:
        foreach (var tuple in _left) {
          hash.Add(tuple);
          yield return tuple;
        }
        foreach (var tuple in _right) {
          var newtuple = TupleBase.Create<TupMin>(tuple.MapValues(_othermap));
          if (!hash.Contains(newtuple)) yield return newtuple;
        }
        break;
      case SetOp.Minus:
        foreach (var tuple in _right) {
          var newtuple = TupleBase.Create<TupMin>(tuple.MapValues(_othermap));
          hash.Add(newtuple);
        }
        foreach (var tuple in _left) {
          if (!hash.Contains(tuple)) yield return tuple;
        }
        break;
      case SetOp.Intersect:
        foreach (var tuple in _left) {
          hash.Add(tuple);
        }
        foreach (var tuple in _right) {
          var newtuple = TupleBase.Create<TupMin>(tuple.MapValues(_othermap));
          if (hash.Contains(newtuple)) yield return newtuple;
        }
        break;
      default:
        break;
      }
    }
  }
}
