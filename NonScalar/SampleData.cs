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
      var tuple = new TupPoint();
      tuple.Init(new object[] { X, Y });
      return tuple;
    }
  }

  /// <summary>
  /// 
  /// </summary>
  public class RelPoint : RelationBase<TupPoint> {
    public RelPoint(IList<TupPoint> tuples) : base(tuples) {
    }
  }

  ///===========================================================================
  /// <summary>
  /// 
  /// </summary>
  public static class NoneData {
    // Empty relation empty header is DUM
    public static RelNone Zero = new RelNone(
      new List<TupNone> { } );
    // Full relation empty header is DEE
    public static RelNone One = new RelNone(
      new List<TupNone> { new TupNone() });
  }

  /// <summary>
  /// Tuple with degree of zero
  /// </summary>
  public class TupNone : TupleBase {
    public readonly static string[] Heading = { };

    public static TupNone Create() {
      var tuple = new TupNone();
      tuple.Init(new object[] { });
      return tuple;
    }
  }

  /// <summary>
  /// Relation with degree of zero
  /// </summary>
  public class RelNone : RelationBase<TupNone> {
    public RelNone(IList<TupNone> tuples) : base(tuples) {
    }
  }

}
