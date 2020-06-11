using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Andl.Common;
using SupplierData;

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

  // Minimal tuple type with null heading
  // Used by pipeline nodes to create tuples where 
  // the heading is not provided but is inferred from the source
  public class Tup : TupBase { 
    public static Tup Data(params object[] values) {
      return Create<Tup>(values);
    }
    public override string ToString() {
      return CommonConverter.ValueToString(Values);
    }
  }

  ///===========================================================================
  /// <summary>
  /// Classes defining function values
  /// </summary>
  public static class RelCon {
    public static FuncValue<T1> Func<T1>(Func<T1> func) 
      => new FuncValue<T1>(func);
    public static FuncValue<T1, T1> Func<T1>(Func<T1, T1> func)
      => new FuncValue<T1, T1>(func);
    public static FuncValue<T1, T1, T1> Func<T1>(Func<T1, T1, T1> func)
      => new FuncValue<T1, T1, T1>(func);


    public static FuncValue<T1,T2> Func<T1, T2>(Func<T1, T2> func) 
      => new FuncValue<T1, T2>(func);

    public static FuncValue<T1,T2,T3> Func<T1, T2, T3>(Func<T1, T2, T3> func) 
      => new FuncValue<T1, T2, T3>(func);
    public static FuncValue<T1, T2, T3,T4> Func<T1, T2, T3, T4>(Func<T1, T2, T3, T4> func) 
      => new FuncValue<T1, T2, T3, T4>(func);
    public static FuncValue<T1,T2,T3,T4,T5> Func<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5> func) 
      => new FuncValue<T1, T2, T3, T4, T5>(func);
    public static FuncValue<T1,T1,T1> Agg<T1>(Func<T1,T1,T1> func)
      => new FuncValue<T1, T1, T1>(func);
    public static FuncWhile While(Func<RelNode, RelNode> func) 
      => new FuncWhile(func);
  }

  public abstract class RelFuncBase {
    internal abstract object Call(object[] args);
  }
  public class FuncValue<T1> : RelFuncBase {
    Func<T1> _func { get; set; }
    internal FuncValue(Func<T1> func) => _func = func;
    internal override object Call(object[] args) => _func();
  }
  public class FuncValue<T1, T2> : RelFuncBase {
    Func<T1, T2> _func { get; set; }
    internal override object Call(object[] args) => _func((T1)args[0]);
    internal FuncValue(Func<T1, T2> func) => _func = func;
  }
  public class FuncValue<T1, T2, T3> : RelFuncBase {
    Func<T1, T2, T3> _func { get; set; }
    internal override object Call(object[] args) => _func((T1)args[0], (T2)args[1]);
    internal FuncValue(Func<T1, T2, T3> func) => _func = func;
  }
  public class FuncValue<T1, T2, T3, T4> : RelFuncBase {
    Func<T1, T2, T3, T4> _func { get; set; }
    internal FuncValue(Func<T1, T2, T3, T4> func) => _func = func;
    internal override object Call(object[] args) => _func((T1)args[0], (T2)args[1], (T3)args[2]);
  }
  public class FuncValue<T1, T2, T3, T4, T5> : RelFuncBase {
    Func<T1, T2, T3, T4, T5> _func { get; set; }
    internal FuncValue(Func<T1, T2, T3, T4, T5> func) => _func = func;
    internal override object Call(object[] args) => _func((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3]);
  }

  public class FuncWhile {
    Func<RelNode, RelNode> _func { get; set; }
    internal FuncWhile(Func<RelNode, RelNode> func) => _func = func;
    internal RelNode Call(RelNode arg) => _func(arg);
  }

  ///===========================================================================
  /// <summary>
  /// Pipeline nodes that look like relation values.
  /// 
  /// Nodes carry a heading and a tuple enumerator.
  /// Note: does not implement Equals. Use ToRelVar().Equals() instead.
  /// </summary>
  public abstract class RelNode : IEnumerable<Tup> {
    public CommonHeading Heading { get; protected set; }
    public int Degree { get { return Heading.Degree; } }
    public int Cardinality { get { return this.Count(); } }

    public abstract IEnumerator<Tup> GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

    public override string ToString() {
      return this.Join(";");
    }

    public string Format() {
      return this.Select(t => t.Format(Heading)).Join("\n");
    }

    public RelVar ToRelVar() {
      return new RelVar(this);
    }

    public static RelNode Import(SourceKind kind, string connector, string name, string nodeheading = null) {
      return new ImportNode(kind, connector, name, nodeheading);
    }

    // Construct a node from literal data
    public static RelNode Data(string heading, params Tup[] source) {

      return new WrapperNode(CommonHeading.Create(heading), source);
    }

    // Construct a node from a stream
    public static RelNode Create(string heading, IEnumerable<Tup> source) {

      return new WrapperNode(CommonHeading.Create(heading), source);
    }

    //--------------------------------------------------------------------------
    // RA Nodes

    public RelNode Project(string nodeheading) {
      return new ProjectNode(this, this.Heading.Adapt(nodeheading));
    }
    public RelNode Remove(string nodeheading) {
      return new ProjectNode(this, this.Heading.Adapt(nodeheading), true);
    }
    public RelNode Rename(params string[] nodeheadings) {
      RelNode r = new RenameNode(this, this.Heading.Adapt(nodeheadings[0]));
      for (var i = 1; i < nodeheadings.Length; ++i)
        r = new RenameNode(r, r.Heading.Adapt(nodeheadings[i]));
      return r;
    }
    public RelNode Group(string nodeheading) {
      return new GroupNode(this, GroupingOp.Group, this.Heading.Adapt(nodeheading));
    }
    public RelNode Wrap(string nodeheading) {
      return new GroupNode(this, GroupingOp.Wrap, this.Heading.Adapt(nodeheading));
    }
    public RelNode Ungroup(string nodeheading) {
      return new UngroupNode(this, UngroupingOp.Ungroup, this.Heading.Adapt(nodeheading));
    }
    public RelNode Unwrap(string nodeheading) {
      return new UngroupNode(this, UngroupingOp.Unwrap, this.Heading.Adapt(nodeheading));
    }
    public RelNode Restrict(string nodeheading, RelFuncBase func) {
      return new RestrictNode(this, this.Heading.Adapt(nodeheading), func);
    }
    public RelNode Extend(string nodeheading, RelFuncBase func) {
      return new ExtendNode(this, this.Heading.Adapt(nodeheading), func);
    }
    public RelNode Aggregate(string nodeheading, RelFuncBase func) {
      return new AggregateNode(this, this.Heading.Adapt(nodeheading), func);
    }
    public RelNode Union(RelNode other) {
      return new SetOpNode(this, SetOp.Union, other);
    }
    public RelNode Minus(RelNode other) {
      return new SetOpNode(this, SetOp.Minus, other);
    }
    public RelNode Intersect(RelNode other) {
      return new SetOpNode(this, SetOp.Intersect, other);
    }
    public RelNode Join(RelNode other) {
      return new JoinOpNode(this, JoinOp.Full, other);
    }
    public RelNode Compose(RelNode other) {
      return new JoinOpNode(this, JoinOp.Compose, other);
    }
    public RelNode Semijoin(RelNode other) {
      return new JoinOpNode(this, JoinOp.Semijoin, other);
    }
    public RelNode Antijoin(RelNode other) {
      return new JoinOpNode(this, JoinOp.Antijoin, other);
    }
    public RelNode TranClose() {
      return new TranCloseNode(this);
    }
    public RelNode While(FuncWhile func) {
      return new WhileNode(this, func);
    }
  }

  ///===========================================================================
  /// <summary>
  /// Implement a wrapper around a heading and enumerable
  /// </summary>
  class WrapperNode : RelNode {
    protected IEnumerable<TupBase> _source;

    public override IEnumerator<Tup> GetEnumerator() => _source.Cast<Tup>().GetEnumerator();

    public WrapperNode(CommonHeading heading, IEnumerable<TupBase> source) {
      _source = source;
      Heading = heading;
    }
  }


  ///===========================================================================
  /// <summary>
  /// Implement node that imports tuples from a source
  /// </summary>
  class ImportNode : RelNode {
    protected RelationStream<Tup> _source;

    public override IEnumerator<Tup> GetEnumerator() => _source.GetEnumerator();

    public ImportNode(SourceKind kind, string connector, string name, string nodeheading = null) {
      _source = new RelationStream<Tup>(kind, connector, name, nodeheading);
      Heading = _source.Heading;
    }
  }

  ///===========================================================================
  /// <summary>
  /// Implement project node 
  /// Heading set required output
  /// </summary>
  class ProjectNode : RelNode {
    RelNode _source;
    int[] _map;

    public ProjectNode(RelNode source, CommonHeading nodeheading, bool isremove = false) {
      _source = source;
      var _nodeheading = nodeheading;

      Heading = (isremove) ? _source.Heading.Minus(_nodeheading) : _nodeheading;
      _map = Heading.CreateMap(_source.Heading);
      if (Heading.Degree >= _source.Heading.Degree) throw Error.Fatal("too many attributes for project");
      if (_map.Any(x => x < 0)) throw Error.Fatal("headings do not match");
    }

    public override IEnumerator<Tup> GetEnumerator() {
      var hash = new HashSet<TupBase>();
      foreach (var tuple in _source) {
        var newtuple = RelOps.CreateByMap<Tup>(tuple, _map);
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
  class RenameNode : RelNode {
    RelNode _source;
    private CommonHeading _nodeheading;
    private int[] _map;

    public RenameNode(RelNode source, CommonHeading nodeheading) {
      _source = source;
      _nodeheading = nodeheading;
      _map = _nodeheading.CreateMap(_source.Heading);
      if (!(_map.Length == 2 && _map[0] >= 0 && _map[1] < 0)) throw Error.Fatal("invalid heading");
      Heading = _source.Heading.Rename(_nodeheading[0], _nodeheading[1]);
    }

    public override IEnumerator<Tup> GetEnumerator() => _source.GetEnumerator();
  }

  ///===========================================================================
  /// <summary>
  /// Implement wrap/group node 
  /// Heading is names...->name
  /// </summary>
  class GroupNode : RelNode {
    RelNode _source;
    private GroupingOp _op;
    private CommonHeading _nodeheading;
    private int[] _tmap, _map, _kmap;

    public GroupNode(RelNode source, GroupingOp op, CommonHeading nodeheading) {
      _source = source;
      _op = op;
      _nodeheading = nodeheading;
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

    public override IEnumerator<Tup> GetEnumerator() {
      return (_op == GroupingOp.Group) ? GetGroupEnumerator()
        : GetWrapEnumerator();
    }

    IEnumerator<Tup> GetGroupEnumerator() {
      var index = RelOps.BuildIndex(_source, _kmap);
      foreach (var kvp in index) {
        var rva = kvp.Value.Select(t => new CommonRow(_tmap.Select(x => t[x]))).ToArray();
        var newtuple = RelOps.CreateByMap<Tup>(kvp.Key, _map, rva);
        Logger.Assert(newtuple.Degree == Heading.Degree);
        yield return newtuple;
      }
    }
    IEnumerator<Tup> GetWrapEnumerator() {
      foreach (var tuple in _source) {
        var tva = new CommonRow(_tmap.Select(x => tuple[x]));
        var newtuple = RelOps.CreateByMap<Tup>(tuple, _map, tva);
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
  class UngroupNode : RelNode {
    RelNode _source;
    private UngroupingOp _op;
    private CommonHeading _nodeheading;
    private int[] _nodemap, _map1, _map2;

    public UngroupNode(RelNode source, UngroupingOp op, CommonHeading nodeheading) {
      _source = source;
      _op = op;
      _nodeheading = nodeheading;
      _nodemap = _nodeheading.CreateMap(_source.Heading);
      if (!(_nodemap.Length == 1 && _nodemap[0] >= 0)) throw Error.Fatal("invalid heading");
      // on output replace one field by the TVA/RVA
      Heading = _source.Heading.Remove(_nodeheading[0]).Append(_nodeheading[0].Fields);
      // 
      var tupheading = CommonHeading.Create(_nodeheading[0].Fields);
      _map1 = Heading.CreateMap(_source.Heading);
      _map2 = Heading.CreateMap(tupheading);
    }

    public override IEnumerator<Tup> GetEnumerator() {
      return (_op == UngroupingOp.Ungroup) ? GetUngroupEnumerator()
        : GetUnwrapEnumerator();
    }

    // enumerate relation ungrouping
    IEnumerator<Tup> GetUngroupEnumerator() {
      foreach (var tuple in _source) {
        var rva = (CommonRow[])tuple[_nodemap[0]];
        foreach (var row in rva) {
          var newtuple = RelOps.CreateByMap<Tup>(tuple.Values, _map1, row.Values, _map2);
          Logger.Assert(newtuple.Degree == Heading.Degree);
          yield return newtuple;
        }
      }
    }

    // enumerate tuple unwrapping
    IEnumerator<Tup> GetUnwrapEnumerator() {
      foreach (var tuple in _source) {
        var row = (CommonRow)tuple[_nodemap[0]];
        var newtuple = RelOps.CreateByMap<Tup>(tuple.Values, _map1, row.Values, _map2);
        Logger.Assert(newtuple.Degree == Heading.Degree);
        yield return newtuple;
      }
    }

  }

  ///===========================================================================
  /// <summary>
  /// Restrict function node
  /// Heading must be same as other
  /// Function has arguments matching heading, return bool
  /// </summary>
  class RestrictNode : RelNode {
    RelNode _source;
    RelFuncBase _restfunc;
    CommonHeading _restheading;
    int[] _restmap;

    public RestrictNode(RelNode source, CommonHeading nodeheading, RelFuncBase func) {
      _source = source;
      _restheading = nodeheading;
      _restfunc = func;

      Heading = _source.Heading;
      _restmap = _restheading.CreateMap(Heading);
      if (_restmap.Any(x => x < 0)) throw Error.Fatal("invalid heading, must all match");
    }

    public override IEnumerator<Tup> GetEnumerator() {
      foreach (var tuple in _source) {
        var newtuple = RelOps.CreateByMap<Tup>(tuple, _restmap);
        Logger.Assert(tuple.Degree == Heading.Degree);
        if ((bool)_restfunc.Call(newtuple.Values)) yield return tuple;
        //if (_restfunc(newtuple)) yield return tuple;
      }
    }
  }

  ///===========================================================================
  /// <summary>
  /// Extend function node
  /// Heading excluding last must be subset of other
  /// Function has arguments matching heading excluding last, return matches last
  /// </summary>
  class ExtendNode : RelNode {
    RelNode _source;
    RelFuncBase _func;
    //Func<TupBase, object> _func;
    CommonHeading _nodeheading;
    private int[] _outmap;
    CommonHeading _argheading;
    int[] _argmap;

    public ExtendNode(RelNode source, CommonHeading nodeheading, RelFuncBase func) {
      _source = source;
      _nodeheading = nodeheading;
      _func = func;

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

    public override IEnumerator<Tup> GetEnumerator() {
      foreach (var tuple in _source) {
        // pick out the argument values, pass them to the function, get return
        var argtuple = RelOps.CreateByMap<Tup>(tuple, _argmap);
        var result = _func.Call(argtuple.Values);
        var newtuple = RelOps.CreateByMap<Tup>(tuple, _outmap, result);
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
  class AggregateNode : RelNode {
    RelNode _source;
    CommonHeading _heading;
    RelFuncBase _func;
    private object _initial;
    private CommonHeading _jhead;
    private int[] _vmap, _jmap1, _jmap2;

    public AggregateNode(RelNode source, CommonHeading nodeheading, RelFuncBase func, object initial = null) {
      _source = source;
      _heading = nodeheading;
      _func = func;
      _initial = initial;

      Heading = _source.Heading.Rename(_heading[0], _heading[1]);
      _vmap = _heading.CreateMap(_source.Heading);
      _jhead = _source.Heading.Minus(_heading);
      _jmap1 = _jhead.CreateMap(_source.Heading);
      _jmap2 = Heading.CreateMap(_jhead);
      if (!(_heading.Degree == 2 && _jmap2.Length == Heading.Degree)) throw Error.Fatal("invalid heading");
    }

    public override IEnumerator<Tup> GetEnumerator() {
      var dict = new Dictionary<TupBase, object>();

      foreach (var tuple in _source) {
        var tupkey = RelOps.CreateByMap<Tup>(tuple, _jmap1);
        var value = tuple.Values[_vmap[0]];
        dict[tupkey] = (dict.ContainsKey(tupkey)) ? _func.Call(new object[] { value, dict[tupkey] })
             : (_initial != null) ? _func.Call(new object[] { value, dict[tupkey], _initial })
             : value;
      }
      foreach (var kv in dict) {
        var newtuple = RelOps.CreateByMap<Tup>(kv.Key, _jmap2, kv.Value);
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
  class SetOpNode : RelNode {
    RelNode _left;
    RelNode _right;
    SetOp _setop;
    int[] _othermap;

    public SetOpNode(RelNode left, SetOp setop, RelNode right) {
      _setop = setop;
      _left = left;
      _right = right;
      Heading = left.Heading;
      _othermap = Heading.CreateMap(right.Heading);
      if (_othermap.Length != Heading.Degree
        || _othermap.Any(x => x < 0)) throw Error.Fatal("invalid heading, must be the same");
    }

    public override IEnumerator<Tup> GetEnumerator() {
      var hash = new HashSet<TupBase>();
      switch (_setop) {
      case SetOp.Union:
        foreach (var tuple in _left) {
          hash.Add(tuple);
          Logger.Assert(tuple.Degree == Heading.Degree);
          yield return tuple;
        }
        foreach (var tuple in _right) {
          var newtuple = RelOps.CreateByMap<Tup>(tuple, _othermap);
          Logger.Assert(tuple.Degree == Heading.Degree);
          if (!hash.Contains(newtuple)) yield return newtuple;
        }
        break;
      case SetOp.Minus:
        foreach (var tuple in _right) {
          var newtuple = RelOps.CreateByMap<Tup>(tuple, _othermap);
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
          var newtuple = RelOps.CreateByMap<Tup>(tuple, _othermap);
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
  class JoinOpNode : RelNode {
    RelNode _leftarg;
    RelNode _rightarg;
    JoinOp _joinop;
    int[] _jmapleft, _jmapright, _tmapleft, _tmapright;

    public JoinOpNode(RelNode leftarg, JoinOp joinop, RelNode rightarg) {
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

    public override IEnumerator<Tup> GetEnumerator() {
      return (_joinop == JoinOp.Full || _joinop == JoinOp.Compose) ? GetFull()
        : GetSemi(_joinop == JoinOp.Antijoin);
    }

    // enumerator for full join and compose (only the output is different)
    IEnumerator<Tup> GetFull() {
      var index = RelOps.BuildIndex(_rightarg, _jmapright);
      var hash = new HashSet<TupBase>();
      foreach (var tuple in _leftarg) {
        var key = RelOps.CreateByMap<Tup>(tuple, _jmapleft);
        if (index.ContainsKey(key)) {
          foreach (var tother in index[key]) {
            var newtuple = RelOps.CreateByMap<Tup>(tuple, _tmapleft, tother, _tmapright);
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
    IEnumerator<Tup> GetSemi(bool isanti) {

      var set = RelOps.BuildSet(_rightarg, _jmapright);
      foreach (var tuple in _leftarg) {
        var key = RelOps.CreateByMap<Tup>(tuple, _jmapleft);
        if (!isanti == set.Contains(key)) {
          Logger.Assert(tuple.Degree == Heading.Degree);
          yield return (tuple);
        }
      }
    }

    // possible alternative???
    IEnumerable<T> GetSemi<T>(bool isanti, IEnumerable<T> leftarg, IEnumerable<T> rightarg, 
      int[] jmapleft, int[] jmapright)
    where T : TupBase,new() {

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
  /// implement transitive closure
  /// heading is inferred, must have exactly two attributes
  /// </summary>
  class TranCloseNode : RelNode {
    RelNode _source;

    public TranCloseNode(RelNode source) {
      _source = source;
      Heading = _source.Heading;
      if (!(source.Degree == 2)) throw Error.Fatal("transitive closure requires argument of degree 2");
    }

    public override IEnumerator<Tup> GetEnumerator() {
      //var ret = RelValue<Tup>.Create(_source);
      var ret = new RelVar(_source);
      var h = _source.Heading.ToNames();
      var joiner = "," + h[0] + "-" + h[1];  // unique
      for (; ; ) {
        var ttt = ret.ToRelNode().Rename(h[1] + joiner).Compose(ret.ToRelNode().Rename(h[0] + joiner));
        var n = ret.Value.Count;
        ret.Insert(ttt);
        if (n == ret.Value.Count) break;
      }
      return ret.GetEnumerator();
    }
  }

  ///===========================================================================
  /// <summary>
  /// implement while fixed point recursion
  /// heading is inferred from argument
  /// </summary>
  class WhileNode : RelNode {
    RelNode _source;
    FuncWhile _whifunc;

    public WhileNode(RelNode source, FuncWhile func) {
      _source = source;
      _whifunc = func;
      Heading = _source.Heading;
    }

    public override IEnumerator<Tup> GetEnumerator() {
      var wrapper = new WrapperNode(Heading, While(Heading, _source, _whifunc.Call));
      return wrapper.GetEnumerator();
    }

    static IEnumerable<TupBase> While(CommonHeading heading, IEnumerable<TupBase> body, Func<RelNode, RelNode> func) {

      var stack = new Stack<TupBase>(body);
      var hash = new HashSet<TupBase>();
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
            stack.Push(eq ? t : RelOps.CreateByMap<Tup>(t, map));
          }
        }
      }
      return hash;
    }
  }
}
