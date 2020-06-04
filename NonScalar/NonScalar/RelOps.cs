using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Andl.Common;

namespace AndlEra {
  ///===========================================================================
  /// <summary>
  /// The real algorithms. 
  /// All statics but making heavy use of generics for type safety.
  /// </summary>
  internal static class RelOps {

    // project relation body onto new heading
    internal static ISet<T> Rename<T1, T>(IEnumerable<T1> body1)
    where T : TupleBase, new()
    where T1 : TupleBase, new() {

      var map = RelationBase<T>.Heading.CreateMapRename(RelationBase<T1>.Heading, true);
      Logger.Assert(map.All(x => x >= 0), "rename heading has missing attribute");
      return new HashSet<T>(body1.Select(t => RelOps.CreateByMap<T>(t, map)));
    }

    internal static ISet<T> Project<T1, T>(IEnumerable<T1> body1)
    where T : TupleBase, new()
    where T1 : TupleBase, new() {

      var map = RelationBase<T>.Heading.CreateMap(RelationBase<T1>.Heading);
      Logger.Assert(map.All(x => x >= 0), "project heading has missing attribute");
      return new HashSet<T>(body1.Select(t => RelOps.CreateByMap<T>(t, map)));
    }

    // extend by one new attribute value
    internal static ISet<T> Extend<T1, T>(IEnumerable<T1> body1, Func<T1,object> func)
    where T : TupleBase, new()
    where T1 : TupleBase, new() {

      var map = RelationBase<T>.Heading.CreateMap(RelationBase<T1>.Heading);
      return new HashSet<T>(body1.Select(t => RelOps.CreateByMap<T>(t, map, func(t))));
    }

    internal static ISet<T> Transform<T1, T>(IEnumerable<T1> body, Func<T1, T> func) 
    where T : TupleBase, new()
    where T1 : TupleBase, new() {

      return new HashSet<T>(body.Select(t => func(t)));
    }

    // agregation
    internal static HashSet<T> Aggregate<T1, T, T2>(IEnumerable<T1> body1, Func<T1, T2, T2> func)
      where T : TupleBase, new()
      where T1 : TupleBase, new()
      where T2 : new() {

      var jhead = RelationBase<T>.Heading.Intersect(RelationBase<T1>.Heading);
      var jmap = jhead.CreateMap(RelationBase<T1>.Heading);
      var map = RelationBase<T>.Heading.CreateMap(jhead);
      var dict = new Dictionary<TupleBase, T2>();

      foreach (var t in body1) {
        var tup = RelOps.CreateByMap<T>(t, jmap);
        dict[tup] = (dict.ContainsKey(tup)) ? func(t, dict[tup]) : dict[tup] = func(t, new T2()); //BUG
      }
      return new HashSet<T>(dict.Select(p => RelOps.CreateByMap<T>(p.Key, map, p.Value)));
    }

    // create new body as set union of two others
    internal static ISet<T> Union<T>(IEnumerable<T> body1, IEnumerable<T> body2)
    where T : TupleBase, new() {

      var output = new HashSet<T>(body1);
      output.UnionWith(body2);
      return output;
    }

    // create new body as set difference of two others
    internal static ISet<T> Minus<T>(IEnumerable<T> body1, IEnumerable<T> body2)
    where T : TupleBase, new() {

      var output = new HashSet<T>(body1);
      output.ExceptWith(body2);
      return output;
    }

    // create new body as set intersection of two others
    internal static ISet<T> Intersect<T>(IEnumerable<T> body1, IEnumerable<T> body2)
    where T : TupleBase, new() {

      var output = new HashSet<T>(body1);
      output.IntersectWith(body2);
      return output;
    }

    // natural join T = T1 join T2
    internal static ISet<T> Join<T,T1,T2>(IEnumerable<T1> body1, IEnumerable<T2> body2)
    where T:TupleBase,new()
    where T1:TupleBase,new()
    where T2:TupleBase,new() {

      var map1 = RelationBase<T>.Heading.CreateMap(RelationBase<T1>.Heading);
      var map2 = RelationBase<T>.Heading.CreateMap(RelationBase<T2>.Heading);
      var jhead = RelationBase<T1>.Heading.Intersect(RelationBase<T2>.Heading);
      var jmap1 = jhead.CreateMap(RelationBase<T1>.Heading);
      var jmap2 = jhead.CreateMap(RelationBase<T2>.Heading);

      if (map2.All(x => x == -1)) return SemiJoin<T,T1,T2>(body1, body2, map1, jmap1, jmap2);

      var index = BuildIndex(body2, jmap2);
      var output = new HashSet<T>();
      foreach (var t1 in body1) {
        var key = RelOps.CreateByMap<T>(t1, jmap1);
        if (index.ContainsKey(key)) {
          foreach (var t2 in index[key]) {
            var newtuple = RelOps.CreateByMap<T>(t1, map1, t2, map2);
            output.Add(newtuple);
          }
        }
      }
      return output;
    }

    // natural anti join T = T1 join T2
    internal static ISet<T> AntiJoin<T, T1, T2>(IEnumerable<T1> body1, IEnumerable<T2> body2)
    where T : TupleBase, new()
    where T1 : TupleBase, new()
    where T2 : TupleBase, new() {

      var map1 = RelationBase<T>.Heading.CreateMap(RelationBase<T1>.Heading);
      var map2 = RelationBase<T>.Heading.CreateMap(RelationBase<T2>.Heading);
      Logger.Assert(map2.All(x => x == -1), "antijoin cannot use right side attributes");
      var jhead = RelationBase<T1>.Heading.Intersect(RelationBase<T2>.Heading);
      var jmap1 = jhead.CreateMap(RelationBase<T1>.Heading);
      var jmap2 = jhead.CreateMap(RelationBase<T2>.Heading);

      return SemiJoin<T, T1, T2>(body1, body2, map1, jmap1, jmap2, false);
    }

    // natural semijoin/antijoin (fields from left only)
    internal static HashSet<T> SemiJoin<T, T1, T2>(IEnumerable<T1> body1, IEnumerable<T2> body2, 
      int[] map1, int[] jmap1, int[] jmap2, bool issemi = true)
      where T : TupleBase, new()
      where T1 : TupleBase, new()
      where T2 : TupleBase, new() {

      var index = BuildSet(body2, jmap2);
      return new HashSet<T>(body1
        .Where(b => issemi == index.Contains(RelOps.CreateByMap<T>(b, jmap1)))
        .Select(b => RelOps.CreateByMap<T>(b, map1)));
    }

    // fixed point recursion
    internal static HashSet<T> While<T,Trel>(IEnumerable<T> body, Func<Trel, RelationBase<T>> func)
      where T : TupleBase, new()
      where Trel : RelationBase<T>, new() {

      var stack = new Stack<T>(body);
      var newbody = new HashSet<T>();
      while (stack.Count > 0) {
          var top = stack.Pop();
          if (!newbody.Contains(top)) {
            newbody.Add(top);
            var rel = RelationBase<T>.Create<Trel>(Enumerable.Repeat(top, 1));
            foreach (var t in func(rel))
              stack.Push(t);
          }
      }
      return newbody;
    }

    ///=========================================================================
    ///  support utility methods
    ///  

    // create a tuple from a tuple and a map
    static internal T CreateByMap<T>(TupleBase tuple, IList<int> map)
    where T : TupleBase, new() {

      Logger.Assert(map.Count(x => x < 0) == 0);
      return TupleBase.Create<T>(Enumerable
        .Range(0, map.Count)
        .Select(x => tuple.Values[map[x]])
        .ToArray());
    }

    // create a tuple from a tuple and a map and a value to replace the -1
    static internal T CreateByMap<T>(TupleBase tuple, IList<int> map, object newvalue)
    where T : TupleBase, new() {

      Logger.Assert(map.Count(x => x < 0) == 1);
      return TupleBase.Create<T>(Enumerable
        .Range(0, map.Count)
        .Select(x => map[x] == -1 ? newvalue : tuple.Values[map[x]])
        .ToArray());
    }

    // create a tuple from two tuples and two maps, second to fill in gaps
    static internal T CreateByMap<T>(TupleBase t1, IList<int> map1, TupleBase t2, IList<int> map2)
    where T : TupleBase, new() {

      Logger.Assert(map1.Count == map2.Count);
      Logger.Assert(map1.Count(x => x < 0) >= 1);
      return CreateByMap<T>(t1.Values, map1, t2.Values, map2);
    }

    static internal T CreateByMap<T>(IList<object> values1, IList<int> map1, IList<object> values2, IList<int> map2)
    where T : TupleBase, new() {
      Logger.Assert(values2 == null || map1.Count == map2.Count);
      return TupleBase.Create<T>(Enumerable
        .Range(0, map1.Count)
        .Select(x => map1[x] >= 0 ? values1[map1[x]] : values2[map2[x]])
        .ToArray());
    }

    // build an index of tuples (because that's where Equals lives)
    static internal Dictionary<TupleBase, IList<TupleBase>> BuildIndex(IEnumerable<TupleBase> values, int[] keymap) {
      var index = new Dictionary<TupleBase, IList<TupleBase>>();
      foreach (var tuple in values) {
        var newkey = RelOps.CreateByMap<TupNone>(tuple, keymap);
        if (index.ContainsKey(newkey))
          index[newkey].Add(tuple);
        else index[newkey] = new List<TupleBase> { tuple };
      }
      return index;
    }

    static internal HashSet<TupleBase> BuildSet(IEnumerable<TupleBase> values, int[] map) {
      return new HashSet<TupleBase>(values.Select(v => RelOps.CreateByMap<Tup>(v, map)));
    }
  }
}
