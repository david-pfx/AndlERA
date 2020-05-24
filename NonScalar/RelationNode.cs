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
  enum JoinOp {
    Full, Compose, Semijoin, Antijoin,
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

    //--------------------------------------------------------------------------
    // RA Nodes

    public RelationNode Project(string newheading) {
      return new ProjectNode(this, newheading);
    }
    public RelationNode Rename(string newheading) {
      return new RenameNode(this, newheading);
    }
    public RelationNode Select(string heading, TupSelect tupsel) {
      return new SelectNode(this, heading, tupsel);
    }
    public RelationNode Extend(string heading, TupExtend tupext) {
      return new ExtendNode(this, heading, tupext);
    }
    public RelationNode Union(RelationNode other) {
      return new SetOpNode(this, SetOp.Union, other);
    }
    public RelationNode Minus(RelationNode other) {
      return new SetOpNode(this, SetOp.Minus, other);
    }
    public RelationNode Intersect(RelationNode other) {
      return new SetOpNode(this, SetOp.Intersect, other);
    }
    public RelationNode Join(RelationNode other) {
      return new JoinOpNode(this, JoinOp.Full, other);
    }
    public RelationNode Compose(RelationNode other) {
      return new JoinOpNode(this, JoinOp.Compose, other);
    }
    public RelationNode Semijoin(RelationNode other) {
      return new JoinOpNode(this, JoinOp.Semijoin, other);
    }
    public RelationNode Antijoin(RelationNode other) {
      return new JoinOpNode(this, JoinOp.Antijoin, other);
    }

  }

  ///===========================================================================
  /// <summary>
  /// Implement node that imports tuples from a source
  /// </summary>
  class ImportNode : RelationNode {
    protected RelationStream<TupMin> _source;

    public override IEnumerator<TupMin> GetEnumerator() => _source.GetEnumerator();

    public ImportNode(SourceKind kind, string connector, string name, string heading = null) {
      _source = new RelationStream<TupMin>(kind, connector, name, heading);
      Heading = _source.Heading;
    }
  }

  ///===========================================================================
  /// <summary>
  /// Implement project node 
  /// Heading set required output
  /// </summary>
  class ProjectNode : RelationNode {
    RelationNode _source;
    int[] _map;

    public ProjectNode(RelationNode source, string newheading) {
      _source = source;
      Heading = _source.Heading.Adapt(newheading);
      _map = Heading.CreateMap(_source.Heading);
      if (Heading.Degree >= _source.Heading.Degree) throw Error.Fatal("too many attributes for project");
      if (_map.Any(x => x < 0)) throw Error.Fatal("headings do not match");
    }

    public override IEnumerator<TupMin> GetEnumerator() {
      var hash = new HashSet<TupMin>();
      foreach (var tuple in _source) {
        var newtuple = TupMin.Create<TupMin>(RelOps.MapValues(tuple.Values, _map));
        if (hash.Contains(newtuple)) continue;
        hash.Add(newtuple);
        yield return newtuple;
      }
    }
  }

  ///===========================================================================
  /// <summary>
  /// Implement rename node 
  /// Heading is name->name
  /// </summary>
  class RenameNode : RelationNode {
    RelationNode _source;

    public RenameNode(RelationNode source, string newheading) {
      _source = source;
      if (newheading.Split(',').Length != 2) throw Error.Fatal("rename requires 2 attributes");
      Heading = _source.Heading.Rename(newheading);
    }

    public override IEnumerator<TupMin> GetEnumerator() {
      return _source.GetEnumerator();
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
        var newtuple = TupMin.Create<TupMin>(RelOps.MapValues(tuple.Values, _selmap));
        if (_selfunc(newtuple)) yield return newtuple;
      }
    }
  }

  ///===========================================================================
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
        var argtuple = TupMin.Create<TupMin>(RelOps.MapValues(tuple.Values, _argmap));
        var result = _extfunc(argtuple);

        // append return to original tuple as final result
        var newvalues = tuple.Values.Concat(new object[] { result }).ToArray();
        var newtuple = TupleBase.Create<TupMin>(newvalues);
        yield return newtuple;
      }
    }
  }

  ///===========================================================================
  /// <summary>
  /// implement set operations
  /// heading is always as per input
  /// </summary>
  class SetOpNode : RelationNode {
    RelationNode _left;
    RelationNode _right;
    SetOp _setop;
    int[] _othermap;

    public SetOpNode(RelationNode left, SetOp setop, RelationNode right) {
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
          var newtuple = TupMin.Create<TupMin>(RelOps.MapValues(tuple.Values, _othermap));
          if (!hash.Contains(newtuple)) yield return newtuple;
        }
        break;
      case SetOp.Minus:
        foreach (var tuple in _right) {
          var newtuple = TupMin.Create<TupMin>(RelOps.MapValues(tuple.Values, _othermap));
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
          var newtuple = TupMin.Create<TupMin>(RelOps.MapValues(tuple.Values, _othermap));
          if (hash.Contains(newtuple)) yield return newtuple;
        }
        break;
      default:
        break;
      }
    }
  }

  ///===========================================================================
  /// <summary>
  /// implement join operations
  /// heading is inferred from arguments
  /// </summary>
  class JoinOpNode : RelationNode {
    RelationNode _leftarg;
    RelationNode _rightarg;
    JoinOp _joinop;
    private int[] _jmapleft;
    private int[] _jmapright;
    private int[] _tmapleft;
    private int[] _tmapright;

    public JoinOpNode(RelationNode leftarg, JoinOp joinop, RelationNode rightarg) {
      _joinop = joinop;
      _leftarg = leftarg;
      _rightarg = rightarg;
      var join = _leftarg.Heading.Intersect(_rightarg.Heading);
      var left = _leftarg.Heading.Minus(join);
      var right = _rightarg.Heading.Minus(join);
      Heading = (joinop == JoinOp.Full) ? _leftarg.Heading.Union(right)
        : (joinop == JoinOp.Compose) ? left.Union(right)
        : _leftarg.Heading;
      _jmapleft = join.CreateMap(_leftarg.Heading);
      _jmapright = join.CreateMap(_rightarg.Heading);
      _tmapleft = Heading.CreateMap(_leftarg.Heading);
      _tmapright = Heading.CreateMap(_rightarg.Heading);
    }

    public override IEnumerator<TupMin> GetEnumerator() {
      return (_joinop == JoinOp.Full || _joinop == JoinOp.Compose) ? GetFull()
        : GetSemi(_joinop == JoinOp.Antijoin);
    }

    // enumerator for full join and compose (only the output is different)
    IEnumerator<TupMin> GetFull() {
      var index = RelOps.BuildIndex(_rightarg, _jmapright);
      var hash = new HashSet<TupMin>();
      foreach (var tuple in _leftarg) {
        var key = TupMin.Create<TupMin>(RelOps.MapValues(tuple.Values, _jmapleft));
        if (index.ContainsKey(key)) {
          foreach (var tother in index[key]) {
            var newtuple = TupMin.Create<TupMin>(RelOps.MapValues(tother.Values, _jmapright));
            if (hash.Contains(newtuple)) continue;
            hash.Add(newtuple);
            yield return (newtuple);
          }
        }
      }
    }

    // enumerator for semijoin and antijoin (only the logic test is different)
    IEnumerator<TupMin> GetSemi(bool isanti) {

      var set = RelOps.BuildSet(_rightarg, _jmapright);
      foreach (var tuple in _leftarg) {
        var key = TupMin.Create<TupMin>(RelOps.MapValues(tuple.Values, _jmapleft));
        if (!isanti == set.Contains(key))
            yield return (tuple);
      }
    }
  }
}
