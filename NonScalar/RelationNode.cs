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

  enum GroupingOp {
    Group, Wrap,
  }
  enum UngroupingOp {
    Ungroup, Unwrap,
  }

  // tuple type used for all pipeline nodes
  public class TupMin : TupleBase { }

  public class TupSelect : TupleBase {
    public Func<TupleBase, bool> Select { get; set; }
    public TupSelect(Func<TupleBase, bool> select) => Select = select;
  }

  public class TupExtend : TupleBase {
    public Func<TupleBase, object> Extend { get; set; }
    public TupExtend(Func<TupleBase, object> extfunc) => Extend = extfunc;
  }

  public class TupAggregate : TupleBase {
    public Func<object, object, object> Aggregate { get; set; }
    public TupAggregate(Func<object, object, object> aggfunc) => Aggregate = aggfunc;
  }

  public class TupWhile : TupleBase {
    public Func<RelationNode, RelationNode> While { get; set; }
    public TupWhile(Func<RelationNode, RelationNode> whilefunc) => While = whilefunc;
  }

  /// <summary>
  /// Pipeline nodes that look like relation values.
  /// 
  /// Nodes carry a heading and a tuple enumerator.
  /// </summary>
  public abstract class RelationNode : IEnumerable<TupleBase> {
    public CommonHeading Heading { get; protected set; }
    public int Degree { get { return Heading.Degree; } }
    public int Cardinality { get { return this.Count(); } }

    public abstract IEnumerator<TupleBase> GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

    public override string ToString() {
      return this.Join(";");
    }

    public string Format() {
      return this.Select(t => t.Format(Heading)).Join("\n");
    }

    public static RelationNode Import(SourceKind kind, string connector, string name, string nodeheading = null) {
      return new ImportNode(kind, connector, name, nodeheading);
    }

    //--------------------------------------------------------------------------
    // RA Nodes

    public RelationNode Project(string nodeheading) {
      return new ProjectNode(this, nodeheading);
    }
    public RelationNode Remove(string nodeheading) {
      return new ProjectNode(this, nodeheading, true);
    }
    public RelationNode Rename(string nodeheading) {
      return new RenameNode(this, nodeheading);
    }
    public RelationNode Group(string nodeheading) {
      return new GroupNode(this, GroupingOp.Group, nodeheading);
    }
    public RelationNode Wrap(string nodeheading) {
      return new GroupNode(this, GroupingOp.Wrap, nodeheading);
    }
    public RelationNode Ungroup(string nodeheading) {
      return new UngroupNode(this, UngroupingOp.Ungroup, nodeheading);
    }
    public RelationNode Unwrap(string nodeheading) {
      return new UngroupNode(this, UngroupingOp.Unwrap, nodeheading);
    }
    public RelationNode Select(string nodeheading, TupSelect tupsel) {
      return new SelectNode(this, nodeheading, tupsel);
    }
    public RelationNode Extend(string nodeheading, TupExtend tupext) {
      return new ExtendNode(this, nodeheading, tupext);
    }
    public RelationNode Aggregate(string nodeheading, TupAggregate tupagg) {
      return new AggregateNode(this, nodeheading, tupagg);
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
    public RelationNode While(TupWhile tupwhile) {
      return new WhileNode(this, tupwhile);
    }

  }

  ///===========================================================================
  /// <summary>
  /// Implement a wrapper around a heading and enumerable
  /// </summary>
  class WrapperNode : RelationNode {
    protected IEnumerable<TupleBase> _source;

    public override IEnumerator<TupleBase> GetEnumerator() => _source.GetEnumerator();

    public WrapperNode(CommonHeading heading, IEnumerable<TupleBase> source) {
      _source = source;
      Heading = heading;
    }
  }


  ///===========================================================================
  /// <summary>
  /// Implement node that imports tuples from a source
  /// </summary>
  class ImportNode : RelationNode {
    protected RelationStream<TupMin> _source;

    public override IEnumerator<TupleBase> GetEnumerator() => _source.GetEnumerator();

    public ImportNode(SourceKind kind, string connector, string name, string nodeheading = null) {
      _source = new RelationStream<TupMin>(kind, connector, name, nodeheading);
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

    public ProjectNode(RelationNode source, string nodeheading, bool isremove = false) {
      _source = source;
      var _nodeheading = _source.Heading.Adapt(nodeheading);

      Heading = (isremove) ? _source.Heading.Minus(_nodeheading) : _nodeheading;
      _map = Heading.CreateMap(_source.Heading);
      if (Heading.Degree >= _source.Heading.Degree) throw Error.Fatal("too many attributes for project");
      if (_map.Any(x => x < 0)) throw Error.Fatal("headings do not match");
    }

    public override IEnumerator<TupleBase> GetEnumerator() {
      var hash = new HashSet<TupleBase>();
      foreach (var tuple in _source) {
        var newtuple = RelOps.CreateByMap<TupMin>(tuple, _map);
        if (hash.Contains(newtuple)) continue;
        hash.Add(newtuple);
        Logger.Assert(newtuple.Degree == Heading.Degree);
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
    private CommonHeading _nodeheading;
    private int[] _map;

    public RenameNode(RelationNode source, string nodeheading) {
      _source = source;
      _nodeheading = _source.Heading.Adapt(nodeheading);
      _map = _nodeheading.CreateMap(_source.Heading);
      if (!(_map.Length == 2 && _map[0] >= 0 && _map[1] < 0)) throw Error.Fatal("invalid heading");
      Heading = _source.Heading.Rename(_nodeheading[0], _nodeheading[1]);
    }

    public override IEnumerator<TupleBase> GetEnumerator() => _source.GetEnumerator();
  }

  ///===========================================================================
  /// <summary>
  /// Implement wrap/group node 
  /// Heading is names...->name
  /// </summary>
  class GroupNode : RelationNode {
    RelationNode _source;
    private GroupingOp _op;
    private CommonHeading _nodeheading;
    private int[] _tmap, _map, _kmap;

    public GroupNode(RelationNode source, GroupingOp op, string nodeheading) {
      _source = source;
      _op = op;
      _nodeheading = _source.Heading.Adapt(nodeheading);
      // inner tuple(s) for TVA or RVA
      var tupheading = _nodeheading.Remove(_nodeheading.Fields.Last());
      _tmap = tupheading.CreateMap(_source.Heading);
      // outer fields used as key for index
      var keyheading = _source.Heading.Minus(tupheading);
      _kmap = keyheading.CreateMap(_source.Heading);
      // final result with new field appended
      Heading = keyheading.Append(new CommonField(_nodeheading.Fields.Last().Name,
        (_op == GroupingOp.Group) ? CommonType.Table : CommonType.Row, tupheading.Fields));
      _map = Heading.CreateMap(keyheading);
      if (!(_tmap.All(x => x >= 0) && _kmap.All(x => x >= 0) && _map.Last() < 0)) throw Error.Fatal("invalid heading");
    }

    public override IEnumerator<TupleBase> GetEnumerator() {
      return (_op == GroupingOp.Group) ? GetGroupEnumerator()
        : GetWrapEnumerator();
    }

    IEnumerator<TupMin> GetGroupEnumerator() {
      var index = RelOps.BuildIndex(_source, _kmap);
      foreach (var kvp in index) {
        var rva = kvp.Value.Select(t => new CommonRow(_tmap.Select(x => t[x]))).ToArray();
        var newtuple = RelOps.CreateByMap<TupMin>(kvp.Key, _map, rva);
        Logger.Assert(newtuple.Degree == Heading.Degree);
        yield return newtuple;
      }
    }
    IEnumerator<TupMin> GetWrapEnumerator() {
      foreach (var tuple in _source) {
        var tva = new CommonRow(_tmap.Select(x => tuple[x]));
        var newtuple = RelOps.CreateByMap<TupMin>(tuple, _map, tva);
        Logger.Assert(newtuple.Degree == Heading.Degree);
        yield return newtuple;
      }
    }
  }

  ///===========================================================================
  /// <summary>
  /// Implement unwrap/ungroup node 
  /// Heading is name of TVA/RVA
  /// </summary>
  class UngroupNode : RelationNode {
    RelationNode _source;
    private UngroupingOp _op;
    private CommonHeading _nodeheading;
    private int[] _nodemap, _map1, _map2;

    public UngroupNode(RelationNode source, UngroupingOp op, string nodeheading) {
      _source = source;
      _op = op;
      _nodeheading = _source.Heading.Adapt(nodeheading);
      _nodemap = _nodeheading.CreateMap(_source.Heading);
      if (!(_nodemap.Length == 1 && _nodemap[0] >= 0)) throw Error.Fatal("invalid heading");
      // on output replace one field by the TVA/RVA
      Heading = _source.Heading.Remove(_nodeheading[0]).Append(_nodeheading[0].Fields);
      // 
      var tupheading = CommonHeading.Create(_nodeheading[0].Fields);
      _map1 = Heading.CreateMap(_source.Heading);
      _map2 = Heading.CreateMap(tupheading);
    }

    public override IEnumerator<TupleBase> GetEnumerator() {
      return (_op == UngroupingOp.Ungroup) ? GetUngroupEnumerator()
        : GetUnwrapEnumerator();
    }

    // enumerate relation ungrouping
    IEnumerator<TupMin> GetUngroupEnumerator() {
      foreach (var tuple in _source) {
        var rva = (CommonRow[])tuple[_nodemap[0]];
        foreach (var row in rva) {
          var newtuple = RelOps.CreateByMap<TupMin>(tuple.Values, _map1, row.Values, _map2);
          Logger.Assert(newtuple.Degree == Heading.Degree);
          yield return newtuple;
        }
      }
    }

    // enumerate tuple unwrapping
    IEnumerator<TupMin> GetUnwrapEnumerator() {
      foreach (var tuple in _source) {
        var row = (CommonRow)tuple[_nodemap[0]];
        var newtuple = RelOps.CreateByMap<TupMin>(tuple.Values, _map1, row.Values, _map2);
        Logger.Assert(newtuple.Degree == Heading.Degree);
        yield return newtuple;
      }
    }

  }

  ///===========================================================================
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

    public SelectNode(RelationNode source, string nodeheading, TupSelect tupsel) {
      _source = source;
      _selheading = _source.Heading.Adapt(nodeheading);
      _selfunc = tupsel.Select;

      Heading = _source.Heading;
      _selmap = _selheading.CreateMap(Heading);
      if (_selmap.Any(x => x < 0)) throw Error.Fatal("invalid map, must all match");
    }

    public override IEnumerator<TupleBase> GetEnumerator() {
      foreach (var tuple in _source) {
        var newtuple = RelOps.CreateByMap<TupMin>(tuple, _selmap);
        Logger.Assert(tuple.Degree == Heading.Degree);
        if (_selfunc(newtuple)) yield return tuple;
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
    CommonHeading _nodeheading;
    private int[] _outmap;
    CommonHeading _argheading;
    int[] _argmap;

    public ExtendNode(RelationNode source, string nodeheading, TupExtend tupsel) {
      _source = source;
      _nodeheading = _source.Heading.Adapt(nodeheading);
      _extfunc = tupsel.Extend;

      Heading = CommonHeading.Create(_source.Heading.Fields.Union(_nodeheading.Fields));
      // get the argument fields
      _argheading = CommonHeading.Create(_nodeheading.Fields.Take(_nodeheading.Fields.Length - 1));
      _argmap = _argheading.CreateMap(_source.Heading);
      // last field can extend or replace, remove name but retain place
      var outh = Heading.Rename(_nodeheading.Fields.Last(), CommonField.Empty);
      _outmap = Heading.CreateMap(outh);
      if (_argmap.Any(x => x < 0)) throw Error.Fatal("invalid heading, field mismatch");
      if (_outmap.Count(x => x < 0) != 1) throw Error.Fatal("invalid heading, return field mismatch"); // any use?
    }

    public override IEnumerator<TupleBase> GetEnumerator() {
      foreach (var tuple in _source) {
        // pick out the argument values, pass them to the function, get return
        var argtuple = RelOps.CreateByMap<TupMin>(tuple, _argmap);
        var result = _extfunc(argtuple);
        var newtuple = RelOps.CreateByMap<TupMin>(tuple, _outmap, result);
        Logger.Assert(newtuple.Degree == Heading.Degree);
        yield return newtuple;
      }
    }
  }

  ///===========================================================================
  /// <summary>
  /// Aggregate function node
  /// Heading has 2 attributes
  /// Function has 2 arguments (value and accumulator) of matching type, return matches last
  /// Optional initial value, if null first time uses first value
  /// </summary>
  class AggregateNode : RelationNode {
    RelationNode _source;
    CommonHeading _heading;
    Func<object, object, object> _aggfunc;
    private object _initial;
    private CommonHeading _jhead;
    private int[] _vmap, _jmap1, _jmap2;

    public AggregateNode(RelationNode source, string nodeheading, TupAggregate tupagg, object initial = null) {
      _source = source;
      _heading = _source.Heading.Adapt(nodeheading);
      _aggfunc = tupagg.Aggregate;
      _initial = initial;

      Heading = _source.Heading.Rename(_heading[0], _heading[1]);
      _vmap = _heading.CreateMap(_source.Heading);
      _jhead = _source.Heading.Minus(_heading);
      _jmap1 = _jhead.CreateMap(_source.Heading);
      _jmap2 = Heading.CreateMap(_jhead);
      if (!(_heading.Degree == 2 && _jmap2.Length == Heading.Degree)) throw Error.Fatal("invalid heading");
    }

    public override IEnumerator<TupleBase> GetEnumerator() {
      var dict = new Dictionary<TupMin, object>();

      foreach (var tuple in _source) {
        var tupkey = RelOps.CreateByMap<TupMin>(tuple, _jmap1);
        var value = tuple.Values[_vmap[0]];
        dict[tupkey] = (dict.ContainsKey(tupkey)) ? _aggfunc(value, dict[tupkey]) 
          : (_initial != null) ? _aggfunc(value, _initial) : value;
      }
      foreach (var kv in dict) {
        var newtuple = RelOps.CreateByMap<TupMin>(kv.Key, _jmap2, kv.Value);
        Logger.Assert(newtuple.Degree == Heading.Degree);
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

    public override IEnumerator<TupleBase> GetEnumerator() {
      var hash = new HashSet<TupleBase>();
      switch (_setop) {
      case SetOp.Union:
        foreach (var tuple in _left) {
          hash.Add(tuple);
          Logger.Assert(tuple.Degree == Heading.Degree);
          yield return tuple;
        }
        foreach (var tuple in _right) {
          var newtuple = RelOps.CreateByMap<TupMin>(tuple, _othermap);
          Logger.Assert(tuple.Degree == Heading.Degree);
          if (!hash.Contains(newtuple)) yield return newtuple;
        }
        break;
      case SetOp.Minus:
        foreach (var tuple in _right) {
          var newtuple = RelOps.CreateByMap<TupMin>(tuple, _othermap);
          hash.Add(newtuple);
        }
        foreach (var tuple in _left) {
          Logger.Assert(tuple.Degree == Heading.Degree);
          if (!hash.Contains(tuple)) yield return tuple;
        }
        break;
      case SetOp.Intersect:
        foreach (var tuple in _left) {
          hash.Add(tuple);
        }
        foreach (var tuple in _right) {
          var newtuple = RelOps.CreateByMap<TupMin>(tuple, _othermap);
          Logger.Assert(tuple.Degree == Heading.Degree);
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
    int[] _jmapleft, _jmapright, _tmapleft, _tmapright;

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

    public override IEnumerator<TupleBase> GetEnumerator() {
      return (_joinop == JoinOp.Full || _joinop == JoinOp.Compose) ? GetFull()
        : GetSemi(_joinop == JoinOp.Antijoin);
    }

    // enumerator for full join and compose (only the output is different)
    IEnumerator<TupleBase> GetFull() {
      var index = RelOps.BuildIndex(_rightarg, _jmapright);
      var hash = new HashSet<TupleBase>();
      foreach (var tuple in _leftarg) {
        var key = RelOps.CreateByMap<TupMin>(tuple, _jmapleft);
        if (index.ContainsKey(key)) {
          foreach (var tother in index[key]) {
            var newtuple = RelOps.CreateByMap<TupMin>(tuple, _tmapleft, tother, _tmapright);
            if (!hash.Contains(newtuple)) {
              hash.Add(newtuple);
              Logger.Assert(newtuple.Degree == Heading.Degree);
              yield return (newtuple);
            }
          }
        }
      }
    }

    // enumerator for semijoin and antijoin (only the logic test is different)
    IEnumerator<TupleBase> GetSemi(bool isanti) {

      var set = RelOps.BuildSet(_rightarg, _jmapright);
      foreach (var tuple in _leftarg) {
        var key = RelOps.CreateByMap<TupMin>(tuple, _jmapleft);
        if (!isanti == set.Contains(key)) {
          Logger.Assert(tuple.Degree == Heading.Degree);
          yield return (tuple);
        }
      }
    }

    // possible alternative???
    IEnumerable<T> GetSemi<T>(bool isanti, IEnumerable<T> leftarg, IEnumerable<T> rightarg, int[] jmapleft, int[] jmapright)
    where T : TupleBase,new() {

      var set = RelOps.BuildSet(rightarg, jmapright);
      foreach (var tuple in leftarg) {
        var key = RelOps.CreateByMap<T>(tuple, jmapleft);
        if (!isanti == set.Contains(key)) {
          Logger.Assert(tuple.Degree == Heading.Degree);
          yield return (tuple);
        }
      }
    }

  }

  ///===========================================================================
  /// <summary>
  /// implement while fixed point recursion
  /// heading is inferred from argument
  /// </summary>
  class WhileNode : RelationNode {
    RelationNode _source;
    Func<RelationNode, RelationNode> _whifunc;

    public WhileNode(RelationNode source, TupWhile tupwhi) {
      _source = source;
      _whifunc = tupwhi.While;
      Heading = _source.Heading;
    }

    public override IEnumerator<TupleBase> GetEnumerator() {
      var wrapper = new WrapperNode(Heading, While(Heading, _source, _whifunc));
      return wrapper.GetEnumerator();
    }

    static IEnumerable<TupleBase> While(CommonHeading heading, IEnumerable<TupleBase> body, Func<RelationNode, RelationNode> func) {

      var stack = new Stack<TupleBase>(body);
      var hash = new HashSet<TupleBase>();
      while (stack.Count > 0) {
        var top = stack.Pop();
        if (!hash.Contains(top)) {
          hash.Add(top);
          var result = func(new WrapperNode(heading, Enumerable.Repeat(top, 1)));
          var eq = result.Heading.IsEqual(heading);
          var map = heading.CreateMap(result.Heading);
          if (result.Heading.Degree != heading.Degree 
            || map.Any(x => x < 0)) throw Error.Fatal($"heading mismatch: {result.Heading} {heading}");
          // convert compatible if needed
          foreach (var t in result) {
            stack.Push(eq ? t : RelOps.CreateByMap<TupMin>(t, map));
          }
        }
      }
      return hash;
    }
  }
}
