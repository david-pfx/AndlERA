using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Andl.Common;

namespace AndlEra {
  ///===========================================================================
  /// <summary>
  /// 
  /// </summary>
  public static class RelOps {

    // natural join T = T1 join T2
    public static ISet<T> Join<T,T1,T2>(ISet<T1> body1, ISet<T2> body2)
    where T:TupleBase,new()
    where T1:TupleBase,new()
    where T2:TupleBase,new() {

      var map1 = MakeMap(RelationBase<T>.Heading, RelationBase<T1>.Heading);
      var map2 = MakeMap(RelationBase<T>.Heading, RelationBase<T2>.Heading);
      var jhead = FindJoin(RelationBase<T1>.Heading, RelationBase<T2>.Heading);
      var jmap1 = MakeMap(jhead, RelationBase<T1>.Heading);
      var jmap2 = MakeMap(jhead, RelationBase<T2>.Heading);

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

    internal static int[] MapRename(string[] head, string[] ohead) {
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

    internal static int[] MapProject(string[] head, string[] ohead) {
      var map = new int[head.Length];
      for (int hx = 0; hx < head.Length; ++hx) {
        map[hx] = Array.FindIndex(ohead, s => s == head[hx]);  // equals?
        Logger.Assert(map[hx] != -1);
      }
      return map;
    }

    // Create a map from head1 to head2 for names that match
    internal static int[] MakeMap(string[] head1, string[] head2) {
      var map = new int[head1.Length];
      for (int hx = 0; hx < head1.Length; ++hx) {
        map[hx] = Array.FindIndex(head2, s => s == head1[hx]);  // equals?
      }
      return map;
    }

    // Find the set of names in common
    internal static string[] FindJoin(string[] head1, string[] head2) {
      return head1.Where(s1 => head2.Contains(s1)).ToArray();
    }


    internal static object[] MapValues(IList<object>values, IList<int> map) {
      return Enumerable.Range(0, map.Count)
        .Select(x => values[map[x]])
        .ToArray();
    }

    internal static object[] MapValues(TupleBase t1, IList<int> map1, TupleBase t2, IList<int> map2) {
      Logger.Assert(map1.Count == map2.Count);
      return Enumerable.Range(0, map1.Count)
        .Select(x => map1[x] >= 0 ? t1.Values[map1[x]] : t2.Values[map2[x]])
        .ToArray();
    }

    // build an index of tuples (because that's where Equals lives)
    static Dictionary<TupleBase, IList<TupleBase>> BuildIndex(IEnumerable<TupleBase> values, int[] map) {
      var index = new Dictionary<TupleBase, IList<TupleBase>>();
      foreach (var tuple in values) {
        var newkey = TupleBase.Create<TupNone>(MapValues(tuple.Values, map));
        if (index.ContainsKey(newkey))
          index[newkey].Add(tuple);
        else index[newkey] = new List<TupleBase> { tuple };
      }
      return index;
    }

  }

  ///===========================================================================
  /// <summary>
  /// 
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
  /// 
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
