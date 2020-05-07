using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndlEra {
  ///===========================================================================
  /// <summary>
  /// 
  /// </summary>
  public class TupPoint : TupleBase {
    public readonly static string[] Heading = { "X", "Y" };

    public int X { get { return (int)_values[0]; } }
    public int Y { get { return (int)_values[1]; } }

    public static TupPoint Create(int X, int Y) {
      return Create<TupPoint>(new object[] { X, Y });
    }
  }

  /// <summary>
  /// 
  /// </summary>
  public class RelPoint : RelationBase<TupPoint> {
    public static RelPoint Create(IList<TupPoint> tuples) {
      return Create<RelPoint>(tuples);
    }
  }

  ///===========================================================================
  /// <summary>
  /// 
  /// </summary>
  public static class NoneData {
    // Empty relation empty header is DUM
    public static RelNone Zero = RelNone.Create(
      new List<TupNone> { } );
    // Full relation empty header is DEE
    public static RelNone One = RelNone.Create(
      new List<TupNone> { new TupNone() });
  }

  /// <summary>
  /// Tuple with degree of zero
  /// </summary>
  public class TupNone : TupleBase {
    public readonly static string[] Heading = { };

    public static TupNone Create() {
      return Create<TupNone>(new object[] { });
    }
  }

  /// <summary>
  /// Relation with degree of zero
  /// </summary>
  public class RelNone : RelationBase<TupNone> {
    public static RelNone Create(IList<TupNone> tuples) {
      return Create<RelNone>(tuples);
    }
  }

}
