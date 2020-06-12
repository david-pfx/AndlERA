using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Andl.Common;

namespace AndlEra {
  ///===========================================================================
  /// <summary>
  /// Static library of RA functions, param by tuple type
  /// Input is Tuple stream, output is ISet
  /// </summary>
  internal static class RelOpsST {

    // project relation body onto new heading
    internal static ISet<T> Rename<T1, T>(IEnumerable<T1> body1)
    where T : TupBase, new()
    where T1 : TupBase, new() {

      var map = RelValueST<T>.Heading.CreateMapRename(RelValueST<T1>.Heading, true);
      Logger.Assert(map.All(x => x >= 0), "rename heading has missing attribute");
      return new HashSet<T>(body1.Select(t => RelStatic.CreateByMap<T>(t, map)));
    }

    internal static ISet<T> Project<T1, T>(IEnumerable<T1> body1)
    where T : TupBase, new()
    where T1 : TupBase, new() {

      var map = RelValueST<T>.Heading.CreateMap(RelValueST<T1>.Heading);
      Logger.Assert(map.All(x => x >= 0), "project heading has missing attribute");
      return new HashSet<T>(body1.Select(t => RelStatic.CreateByMap<T>(t, map)));
    }

    // extend by one new attribute value
    internal static ISet<T> Extend<T1, T>(IEnumerable<T1> body1, Func<T1, object> func)
    where T : TupBase, new()
    where T1 : TupBase, new() {

      var map = RelValueST<T>.Heading.CreateMap(RelValueST<T1>.Heading);
      return new HashSet<T>(body1.Select(t => RelStatic.CreateByMap<T>(t, map, func(t))));
    }

    internal static ISet<T> Transform<T1, T>(IEnumerable<T1> body, Func<T1, T> func)
    where T : TupBase, new()
    where T1 : TupBase, new() {

      return new HashSet<T>(body.Select(t => func(t)));
    }

    // agregation
    internal static HashSet<T> Aggregate<T1, T, T2>(IEnumerable<T1> body1, Func<T1, T2, T2> func)
      where T : TupBase, new()
      where T1 : TupBase, new()
      where T2 : new() {

      var jhead = RelValueST<T>.Heading.Intersect(RelValueST<T1>.Heading);
      var jmap = jhead.CreateMap(RelValueST<T1>.Heading);
      var map = RelValueST<T>.Heading.CreateMap(jhead);
      var dict = new Dictionary<TupBase, T2>();

      foreach (var t in body1) {
        var tup = RelStatic.CreateByMap<T>(t, jmap);
        dict[tup] = (dict.ContainsKey(tup)) ? func(t, dict[tup]) : dict[tup] = func(t, new T2()); //BUG
      }
      return new HashSet<T>(dict.Select(p => RelStatic.CreateByMap<T>(p.Key, map, p.Value)));
    }

    // create new body as set union of two others
    internal static ISet<T> Union<T>(IEnumerable<T> body1, IEnumerable<T> body2)
    where T : TupBase, new() {

      var output = new HashSet<T>(body1);
      output.UnionWith(body2);
      return output;
    }

    // create new body as set difference of two others
    internal static ISet<T> Minus<T>(IEnumerable<T> body1, IEnumerable<T> body2)
    where T : TupBase, new() {

      var output = new HashSet<T>(body1);
      output.ExceptWith(body2);
      return output;
    }

    // create new body as set intersection of two others
    internal static ISet<T> Intersect<T>(IEnumerable<T> body1, IEnumerable<T> body2)
    where T : TupBase, new() {

      var output = new HashSet<T>(body1);
      output.IntersectWith(body2);
      return output;
    }

    // natural join T = T1 join T2
    internal static ISet<T> Join<T, T1, T2>(IEnumerable<T1> body1, IEnumerable<T2> body2)
    where T : TupBase, new()
    where T1 : TupBase, new()
    where T2 : TupBase, new() {

      var map1 = RelValueST<T>.Heading.CreateMap(RelValueST<T1>.Heading);
      var map2 = RelValueST<T>.Heading.CreateMap(RelValueST<T2>.Heading);
      var jhead = RelValueST<T1>.Heading.Intersect(RelValueST<T2>.Heading);
      var jmap1 = jhead.CreateMap(RelValueST<T1>.Heading);
      var jmap2 = jhead.CreateMap(RelValueST<T2>.Heading);

      if (map2.All(x => x == -1)) return SemiJoin<T, T1, T2>(body1, body2, map1, jmap1, jmap2);

      var index = RelStatic.BuildIndex(body2, jmap2);
      var output = new HashSet<T>();
      foreach (var t1 in body1) {
        var key = RelStatic.CreateByMap<T>(t1, jmap1);
        if (index.ContainsKey(key)) {
          foreach (var t2 in index[key]) {
            var newtuple = RelStatic.CreateByMap<T>(t1, map1, t2, map2);
            output.Add(newtuple);
          }
        }
      }
      return output;
    }

    // natural anti join T = T1 join T2
    internal static ISet<T> AntiJoin<T, T1, T2>(IEnumerable<T1> body1, IEnumerable<T2> body2)
    where T : TupBase, new()
    where T1 : TupBase, new()
    where T2 : TupBase, new() {

      var map1 = RelValueST<T>.Heading.CreateMap(RelValueST<T1>.Heading);
      var map2 = RelValueST<T>.Heading.CreateMap(RelValueST<T2>.Heading);
      Logger.Assert(map2.All(x => x == -1), "antijoin cannot use right side attributes");
      var jhead = RelValueST<T1>.Heading.Intersect(RelValueST<T2>.Heading);
      var jmap1 = jhead.CreateMap(RelValueST<T1>.Heading);
      var jmap2 = jhead.CreateMap(RelValueST<T2>.Heading);

      return SemiJoin<T, T1, T2>(body1, body2, map1, jmap1, jmap2, false);
    }

    // natural semijoin/antijoin (fields from left only)
    internal static HashSet<T> SemiJoin<T, T1, T2>(IEnumerable<T1> body1, IEnumerable<T2> body2,
      int[] map1, int[] jmap1, int[] jmap2, bool issemi = true)
      where T : TupBase, new()
      where T1 : TupBase, new()
      where T2 : TupBase, new() {

      var index = RelStatic.BuildSet(body2, jmap2);
      return new HashSet<T>(body1
        .Where(b => issemi == index.Contains(RelStatic.CreateByMap<T>(b, jmap1)))
        .Select(b => RelStatic.CreateByMap<T>(b, map1)));
    }

    // fixed point recursion
    internal static HashSet<T> While<T, TR>(IEnumerable<T> body, Func<TR, RelValueST<T>> func)
      where T : TupBase, new()
      where TR : RelValueST<T>, new() {

      var stack = new Stack<T>(body);
      var newbody = new HashSet<T>();
      while (stack.Count > 0) {
        var top = stack.Pop();
        if (!newbody.Contains(top)) {
          newbody.Add(top);
          var rel = RelValueST<T>.Create<TR>(Enumerable.Repeat(top, 1));
          foreach (var t in func(rel))
            stack.Push(t);
        }
      }
      return newbody;
    }
  }
}
