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
  public class RelPoint : RelationBase<TupPoint> { }

  public class TupPoint : TupleBase {
    public readonly static string[] Heading = { "X", "Y" };

    public int X { get { return (int)Values[0]; } }
    public int Y { get { return (int)Values[1]; } }

    public static TupPoint Create(int X, int Y) {
      return Create<TupPoint>(new object[] { X, Y });
    }
  }
}
