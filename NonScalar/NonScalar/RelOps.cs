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

      var map = MakeRenameMap(RelationBase<T>.Heading, RelationBase<T1>.Heading);
      Logger.Assert(map.All(x => x >= 0), "rename heading has missing attribute");
      return new HashSet<T>(body1.Select(t => TupleBase.Create<T>(MapValues(t.Values, map))));
    }

    internal static ISet<T> Project<T1, T>(IEnumerable<T1> body1)
    where T : TupleBase, new()
    where T1 : TupleBase, new() {

      var map = MakeMap(RelationBase<T>.Heading, RelationBase<T1>.Heading);
      Logger.Assert(map.All(x => x >= 0), "project heading has missing attribute");
      return new HashSet<T>(body1.Select(t => TupleBase.Create<T>(MapValues(t.Values, map))));
    }

    // extend by one new attribute value
    internal static ISet<T> Extend<T1, T>(IEnumerable<T1> body1, Func<T1,object> func)
    where T : TupleBase, new()
    where T1 : TupleBase, new() {

      var map = MakeMap(RelationBase<T>.Heading, RelationBase<T1>.Heading);
      return new HashSet<T>(body1.Select(t => TupleBase.Create<T>(MapValues(t.Values, map, func(t)))));
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

      var jhead = MakeJoinHeading(RelationBase<T>.Heading, RelationBase<T1>.Heading);
      var jmap = MakeMap(jhead, RelationBase<T1>.Heading);
      var map = MakeMap(RelationBase<T>.Heading, jhead);
      var dict = new Dictionary<TupleBase, T2>();

      foreach (var t in body1) {
        var tup = TupleBase.Create<T>(MapValues(t.Values, jmap));
        dict[tup] = (dict.ContainsKey(tup)) ? func(t, dict[tup]) : dict[tup] = func(t, new T2());
      }
      return new HashSet<T>(dict.Select(p => TupleBase.Create<T>(MapValues(p.Key.Values, map, p.Value))));
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

      var map1 = MakeMap(RelationBase<T>.Heading, RelationBase<T1>.Heading);
      var map2 = MakeMap(RelationBase<T>.Heading, RelationBase<T2>.Heading);
      var jhead = MakeJoinHeading(RelationBase<T1>.Heading, RelationBase<T2>.Heading);
      var jmap1 = MakeMap(jhead, RelationBase<T1>.Heading);
      var jmap2 = MakeMap(jhead, RelationBase<T2>.Heading);

      if (map2.All(x => x == -1)) return SemiJoin<T,T1,T2>(body1, body2, map1, jmap1, jmap2);

      var index = BuildIndex(body2, jmap2);
      var output = new HashSet<T>();
      foreach (var t1 in body1) {
        var key = TupleBase.Create<T>(MapValues(t1.Values, jmap1));
        if (index.ContainsKey(key)) {
          foreach (var t2 in index[key]) {
            var newtuple = MapValues(t1, map1, t2, map2);
            output.Add(TupleBase.Create<T>(newtuple));
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

      var map1 = MakeMap(RelationBase<T>.Heading, RelationBase<T1>.Heading);
      var map2 = MakeMap(RelationBase<T>.Heading, RelationBase<T2>.Heading);
      Logger.Assert(map2.All(x => x == -1), "antijoin cannot use right side attributes");
      var jhead = MakeJoinHeading(RelationBase<T1>.Heading, RelationBase<T2>.Heading);
      var jmap1 = MakeMap(jhead, RelationBase<T1>.Heading);
      var jmap2 = MakeMap(jhead, RelationBase<T2>.Heading);

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
        .Where(b => issemi == index.Contains(TupleBase.Create<TupNone>(MapValues(b.Values, jmap1))))
        .Select(b => TupleBase.Create<T>(MapValues(b.Values, map1))));
    }

    // fixed point recursion
    internal static HashSet<T> While<T>(IEnumerable<T> body, Func<RelationBase<T>, RelationBase<T>> func)
      where T : TupleBase, new() {

      var stack = new Stack<T>(body);
      var newbody = new HashSet<T>();
      while (stack.Count > 0) {
          var top = stack.Pop();
          if (!newbody.Contains(top)) {
            newbody.Add(top);
            var rel = RelationBase<T>.Create<RelationBase<T>>(Enumerable.Repeat(top, 1));
            foreach (var t in func(rel))
              stack.Push(t);
          }
      }
      return newbody;
    }

    ///=========================================================================
    ///  implementation
    ///  

    // build an index of tuples (because that's where Equals lives)
    static internal Dictionary<TupleBase, IList<TupleBase>> BuildIndex(IEnumerable<TupleBase> values, int[] map) {
      var index = new Dictionary<TupleBase, IList<TupleBase>>();
      foreach (var tuple in values) {
        var newkey = TupleBase.Create<TupNone>(MapValues(tuple.Values, map));
        if (index.ContainsKey(newkey))
          index[newkey].Add(tuple);
        else index[newkey] = new List<TupleBase> { tuple };
      }
      return index;
    }

    static internal HashSet<TupleBase> BuildSet(IEnumerable<TupleBase> values, int[] map) {
      return new HashSet<TupleBase>(values.Select(v => TupleBase.Create<TupNone>(MapValues(v.Values, map))));
    }

    static int[] MakeRenameMap(Dictionary<string,int> head, Dictionary<string, int> ohead) {
      var pnew = ohead.FirstOrDefault(p => !head.ContainsKey(p.Key));
      Logger.Assert(pnew.Key != null, "nothing to rename");
      return head.Select(p => ohead.SafeLookup(p.Key, pnew.Value)).ToArray();
    }

    static int[] MakeRenameMap(string[] head, string[] ohead) {
      var map = new int[head.Length];
      var odd = -1;
      for (int hx = 0; hx < head.Length; ++hx) {
        map[hx] = Array.FindIndex(ohead, s => s == head[hx]);  // equals?
        if (map[hx] == -1) {
          Logger.Assert(odd == -1);
          odd = hx;
        }
      }
      for (int ox = 0; ox < ohead.Length; ++ox) {
        if (Array.FindIndex(map, i => i == ox) == -1) {
          map[odd] = ox;
          break;
        }
      }
      return map;
    }

    // Create a map from head1 to head2 for names that match
    static int[] MakeMap(string[] head1, string[] head2) {
      var map = new int[head1.Length];
      for (int hx = 0; hx < head1.Length; ++hx) {
        map[hx] = Array.FindIndex(head2, s => s == head1[hx]);  // equals?
      }
      return map;
    }

    // Find the set of names in common
    static string[] MakeJoinHeading(string[] head1, string[] head2) {
      return head1.Where(s1 => head2.Contains(s1)).ToArray();
    }


    static internal object[] MapValues(IList<object>values, IList<int> map) {
      return Enumerable.Range(0, map.Count)
        .Select(x => values[map[x]])
        .ToArray();
    }

    static internal object[] MapValues(IList<object> values, IList<int> map, object newvalue) {
      return Enumerable.Range(0, map.Count)
        .Select(x => map[x] == -1 ? newvalue : values[map[x]])
        .ToArray();
    }

    static internal object[] MapValues(TupleBase t1, IList<int> map1, TupleBase t2, IList<int> map2) {
      Logger.Assert(map1.Count == map2.Count);
      return Enumerable.Range(0, map1.Count)
        .Select(x => map1[x] >= 0 ? t1.Values[map1[x]] : t2.Values[map2[x]])
        .ToArray();
    }

  }

  ///===========================================================================
  /// <summary>
  /// The two empty relations DUM and DEE
  /// </summary>
  public static class NoneData {
    // Empty relation empty header is DUM
    public static RelNone Zero = RelNone.Create<RelNone>(new List<TupNone> { });
    // Full relation empty header is DEE
    public static RelNone One = RelNone.Create<RelNone>(new List<TupNone> { new TupNone() });
  }

  /// <summary>
  /// Tuple with degree of zero
  /// </summary>
  public class RelNone : RelationBase<TupNone> { }

  public class TupNone : TupleBase {
    public readonly static string[] Heading = { };

    public static TupNone Create() {
      return Create<TupNone>(new object[] { });
    }
  }
  ///===========================================================================
  /// <summary>
  /// A relation that is a sequence of numbers
  /// </summary>
  public class TupSequence : TupleBase {
    public readonly static string[] Heading = { "N" };
    public int N { get { return (int)Values[0]; } }

    public static TupSequence Create(int N) {
      return Create<TupSequence>(new object[] { N });
    }
  }

  public class RelSequence : RelationBase<TupSequence> {
    public static RelSequence Create(int count) {
      return Create<RelSequence>(Enumerable.Range(0, count).Select(n => TupSequence.Create(n)));
    }
  }

  ///===========================================================================
  /// <summary>
  /// A relation that is an array of text strings
  /// </summary>
  public class TupText : TupleBase {
    public readonly static string[] Heading = { "Seq", "Line" };

    public int Seq { get { return (int)Values[0]; } }
    public string Line { get { return (string)Values[1]; } }

    public static TupText Create(int Seq, string Line) {
      return Create<TupText>(new object[] { Seq, Line });
    }
  }

  public class RelText : RelationBase<TupText> {
    public static RelText Create(IList<string> text) {
      return Create<RelText>(Enumerable.Range(0, text.Count)
        .Select(n => TupText
        .Create(n, text[n])));
    }
  }
}
