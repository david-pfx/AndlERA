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

  }

  ///===========================================================================
  /// <summary>
  /// Implement node that imports tuples from a source
  /// </summary>
  class ImportNode : RelationNode {
    protected RelationStream<TupMin> _source;

    public override IEnumerator<TupMin> GetEnumerator() => _source.GetEnumerator();

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

    public ProjectNode(RelationNode source, string nodeheading) {
      _source = source;
      Heading = _source.Heading.Adapt(nodeheading);
      _map = Heading.CreateMap(_source.Heading);
      if (Heading.Degree >= _source.Heading.Degree) throw Error.Fatal("too many attributes for project");
      if (_map.Any(x => x < 0)) throw Error.Fatal("headings do not match");
    }

    public override IEnumerator<TupMin> GetEnumerator() {
      var hash = new HashSet<TupMin>();
      foreach (var tuple in _source) {
        var newtuple = RelOps.CreateByMap<TupMin>(tuple, _map);
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
    private CommonHeading _nodeheading;
    private int[] _map;

    public RenameNode(RelationNode source, string nodeheading) {
      _source = source;
      _nodeheading = _source.Heading.Adapt(nodeheading);
      _map = _nodeheading.CreateMap(_source.Heading);
      if (!(_map.Length == 2 && _map[0] >= 0 && _map[1] < 0)) throw Error.Fatal("invalid heading");
      Heading = _source.Heading.Rename(_nodeheading);
    }

    public override IEnumerator<TupMin> GetEnumerator() => _source.GetEnumerator();
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

    public override IEnumerator<TupMin> GetEnumerator() {
      return (_op == GroupingOp.Group) ? GetGroupEnumerator()
        : GetWrapEnumerator();
    }

    IEnumerator<TupMin> GetGroupEnumerator() {
      var index = RelOps.BuildIndex(_source, _kmap);
      foreach (var kvp in index) {
        var rva = kvp.Value.Select(t => new CommonRow(_tmap.Select(x => t[x]).ToArray())).ToArray();
        //var rva = kvp.Value.Select(t => RelOps.CreateByMap<TupMin>(t, _tmap)).ToArray();
        yield return RelOps.CreateByMap<TupMin>(kvp.Key, _map, rva);
      }
    }
    IEnumerator<TupMin> GetWrapEnumerator() {
      foreach (var tuple in _source) {
        var tva = new CommonRow(_tmap.Select(x => tuple[x]).ToArray());
        //var tva = RelOps.CreateByMap<TupMin>(tuple, _tmap);
        yield return RelOps.CreateByMap<TupMin>(tuple, _map, tva);
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

    public override IEnumerator<TupMin> GetEnumerator() {
      return (_op == UngroupingOp.Ungroup) ? GetUngroupEnumerator()
        : GetUnwrapEnumerator();
    }

    // enumerate relation ungrouping
    IEnumerator<TupMin> GetUngroupEnumerator() {
      foreach (var tuple in _source) {
        var rva = (CommonRow[])tuple[_nodemap[0]];
        foreach (var row in rva)
          yield return RelOps.CreateByMap<TupMin>(tuple.Values, _map1, row.Values, _map2);
      }
    }

    // enumerate tuple unwrapping
    IEnumerator<TupMin> GetUnwrapEnumerator() {
      foreach (var tuple in _source) {
        var row = (CommonRow)tuple[_nodemap[0]];
        yield return RelOps.CreateByMap<TupMin>(tuple.Values, _map1, row.Values, _map2);
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

    public SelectNode(RelationNode source, string nodeheading, TupSelect tupsel) {
      _source = source;
      Heading = _source.Heading;
      _selfunc = tupsel.Select;
      _selheading = Heading.Adapt(nodeheading);
      _selmap = _selheading.CreateMap(Heading);
      if (_selmap.Any(x => x < 0)) throw Error.Fatal("invalid map, must all match");
    }

    public override IEnumerator<TupMin> GetEnumerator() {
      foreach (var tuple in _source) {
        var newtuple = RelOps.CreateByMap<TupMin>(tuple, _selmap);
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
    CommonHeading _nodeheading;
    CommonHeading _argheading;
    int[] _argmap;

    public ExtendNode(RelationNode source, string nodeheading, TupExtend tupsel) {
      _source = source;
      _nodeheading = _source.Heading.Adapt(nodeheading);
      _extfunc = tupsel.Extend;
      Heading = CommonHeading.Create(_source.Heading.Fields.Union(_nodeheading.Fields));
      _argheading = CommonHeading.Create(_nodeheading.Fields.Take(_nodeheading.Fields.Length - 1));
      _argmap = _argheading.CreateMap(_source.Heading);
      if (_argmap.Any(x => x < 0)) throw Error.Fatal("invalid map, must all match");
    }

    public override IEnumerator<TupMin> GetEnumerator() {
      foreach (var tuple in _source) {
        // pick out the argument values, pass them to the function, get return
        var argtuple = RelOps.CreateByMap<TupMin>(tuple, _argmap);
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
  /// Aggregate function node
  /// Heading has 2 attributes
  /// Function has 2 arguments matching, return matches last
  /// </summary>
  class AggregateNode : RelationNode {
    RelationNode _source;
    Func<object, object, object> _aggfunc;
    CommonHeading _heading;
    private int[] _vmap;
    private CommonHeading _jhead;
    private int[] _jmap1;
    private int[] _jmap2;

    public AggregateNode(RelationNode source, string nodeheading, TupAggregate tupagg) {
      _source = source;
      _heading = _source.Heading.Adapt(nodeheading);
      _aggfunc = tupagg.Aggregate;

      Heading = _source.Heading.Rename(_heading);
      _vmap = _heading.CreateMap(_source.Heading);
      _jhead = _source.Heading.Minus(_heading);
      _jmap1 = _jhead.CreateMap(_source.Heading);
      _jmap2 = Heading.CreateMap(_jhead);
      if (!(_heading.Degree == 2 && _jmap2.Length == Heading.Degree)) throw Error.Fatal("invalid heading");
    }

    public override IEnumerator<TupMin> GetEnumerator() {
      var dict = new Dictionary<TupMin, object>();

      foreach (var tuple in _source) {
        var tupkey = RelOps.CreateByMap<TupMin>(tuple, _jmap1);
        var value = tuple.Values[_vmap[0]];
        dict[tupkey] = (dict.ContainsKey(tupkey)) ? _aggfunc(value, dict[tupkey]) : value;
      }
      foreach (var kv in dict) {
        var newtuple = RelOps.CreateByMap<TupMin>(kv.Key, _jmap2, kv.Value);
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
          var newtuple = RelOps.CreateByMap<TupMin>(tuple, _othermap);
          if (!hash.Contains(newtuple)) yield return newtuple;
        }
        break;
      case SetOp.Minus:
        foreach (var tuple in _right) {
          var newtuple = RelOps.CreateByMap<TupMin>(tuple, _othermap);
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
          var newtuple = RelOps.CreateByMap<TupMin>(tuple, _othermap);
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

    public override IEnumerator<TupMin> GetEnumerator() {
      return (_joinop == JoinOp.Full || _joinop == JoinOp.Compose) ? GetFull()
        : GetSemi(_joinop == JoinOp.Antijoin);
    }

    // enumerator for full join and compose (only the output is different)
    IEnumerator<TupMin> GetFull() {
      var index = RelOps.BuildIndex(_rightarg, _jmapright);
      var hash = new HashSet<TupMin>();
      foreach (var tuple in _leftarg) {
        var key = RelOps.CreateByMap<TupMin>(tuple, _jmapleft);
        if (index.ContainsKey(key)) {
          foreach (var tother in index[key]) {
            var newtuple = RelOps.CreateByMap<TupMin>(tuple, _jmapright);
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
        var key = RelOps.CreateByMap<TupMin>(tuple, _jmapleft);
        if (!isanti == set.Contains(key))
          yield return (tuple);
      }
    }
  }
}
