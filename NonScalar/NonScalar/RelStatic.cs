using System;
using System.Collections.Generic;
using System.Linq;
using Andl.Common;

namespace AndlEra {
  ///=========================================================================
  ///  support utility methods
  ///  
  internal static class RelStatic {
    // create a tuple from a tuple and a map
    static internal T CreateByMap<T>(TupBase tuple, IList<int> map)
    where T : TupBase, new() {

      Logger.Assert(map.Count(x => x < 0) == 0);
      return TupBase.Create<T>(Enumerable
        .Range(0, map.Count)
        .Select(x => tuple.Values[map[x]])
        .ToArray());
    }

    // create a tuple from a tuple and a map and a value to replace the -1
    static internal T CreateByMap<T>(TupBase tuple, IList<int> map, object newvalue)
    where T : TupBase, new() {

      Logger.Assert(map.Count(x => x < 0) == 1);
      return TupBase.Create<T>(Enumerable
        .Range(0, map.Count)
        .Select(x => map[x] == -1 ? newvalue : tuple.Values[map[x]])
        .ToArray());
    }

    // create a tuple from two tuples and two maps, second to fill in gaps
    static internal T CreateByMap<T>(TupBase t1, IList<int> map1, TupBase t2, IList<int> map2)
    where T : TupBase, new() {

      Logger.Assert(map1.Count == map2.Count);
      //Logger.Assert(map1.Count(x => x < 0) >= 1);
      return CreateByMap<T>(t1.Values, map1, t2.Values, map2);
    }

    static internal T CreateByMap<T>(IList<object> values1, IList<int> map1, IList<object> values2, IList<int> map2)
    where T : TupBase, new() {
      Logger.Assert(values2 == null || map1.Count == map2.Count);
      return TupBase.Create<T>(Enumerable
        .Range(0, map1.Count)
        .Select(x => map1[x] >= 0 ? values1[map1[x]] : values2[map2[x]])
        .ToArray());
    }

    // build an index of tuples (because that's where Equals lives)
    static internal Dictionary<TupBase, IList<TupBase>> BuildIndex(IEnumerable<TupBase> values, int[] keymap) {
      var index = new Dictionary<TupBase, IList<TupBase>>();
      foreach (var tuple in values) {
        var newkey = RelStatic.CreateByMap<TupNoneST>(tuple, keymap);
        if (index.ContainsKey(newkey))
          index[newkey].Add(tuple);
        else index[newkey] = new List<TupBase> { tuple };
      }
      return index;
    }

    static internal HashSet<TupBase> BuildSet(IEnumerable<TupBase> values, int[] map) {
      return new HashSet<TupBase>(values.Select(v => RelStatic.CreateByMap<Tup>(v, map)));
    }
  }
}
