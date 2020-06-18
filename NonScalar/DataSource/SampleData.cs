/// Andl is A New Data Language. See http://andl.org.
///
/// Copyright © David M. Bennett 2015-20 as an unpublished work. All rights reserved.
/// 
/// This work is licensed under the Creative Commons Attribution-NonCommercial 4.0 International License. 
/// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc/4.0/.
/// 
/// In summary, you are free to share and adapt this software freely for any non-commercial purpose provided 
/// you give due attribution and do not impose additional restrictions.
/// 
/// This software is provided in the hope that it will be useful, but with 
/// absolutely no warranties. You assume all responsibility for its use.

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
  public class RelPoint : RelValueST<TupPoint> { }

  public class TupPoint : TupBase {
    public readonly static string Heading = "X,Y";

    public int X { get { return (int)Values[0]; } }
    public int Y { get { return (int)Values[1]; } }

    public static TupPoint Create(int X, int Y) {
      return Create<TupPoint>(new object[] { X, Y });
    }
  }

  public class RelMMQ : RelValueST<TupMMQ> { }
  public class TupMMQ :TupBase {
    public readonly static string Heading = "Major,Minor,Qty";
    public string Major { get { return (string)Values[0]; } }
    public string Minor { get { return (string)Values[1]; } }
    public int Qty { get { return (int)Values[2]; } }
    public static TupMMQ Create(string major, string minor, int qty) {
      return Create<TupMMQ>(new object[] { major, minor, qty });
    }

  }

  public static class MMQData {
    public static RelMMQ MMQ = RelMMQ.Create<RelMMQ>(
      new List<TupMMQ> {
        TupMMQ.Create("P1", "P2", 5),
        TupMMQ.Create("P1", "P3", 3),
        TupMMQ.Create("P2", "P3", 2),
        TupMMQ.Create("P2", "P4", 7),
        TupMMQ.Create("P3", "P5", 4),
        TupMMQ.Create("P4", "P6", 8),
      });
  }



}
