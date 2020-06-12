using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndlEra {
  ///===========================================================================
  /// <summary>
  /// The two empty relations DUM and DEE
  /// </summary>
  public static class NoneDataST {
    // Empty relation empty header is DUM
    public static RelNoneST Zero = RelNoneST.Create<RelNoneST>(new List<TupNoneST> { });
    // Full relation empty header is DEE
    public static RelNoneST One = RelNoneST.Create<RelNoneST>(new List<TupNoneST> { new TupNoneST() });
  }

  /// <summary>
  /// Tuple with degree of zero
  /// </summary>
  public class RelNoneST : RelValueST<TupNoneST> { }

  public class TupNoneST : TupBase {
    public readonly static string Heading = "";

    public static TupNoneST Create() {
      return Create<TupNoneST>(new object[] { });
    }
  }
  ///===========================================================================
  /// <summary>
  /// A relation that is a sequence of numbers
  /// </summary>
  public class TupSequenceST : TupBase {
    public readonly static string Heading = "N";
    public int N { get { return (int)Values[0]; } }

    public static TupSequenceST Create(int N) {
      return Create<TupSequenceST>(new object[] { N });
    }
  }

  public class RelSequenceST : RelValueST<TupSequenceST> {
    public static RelSequenceST Create(int count) {
      return RelSequenceST.Create<RelSequenceST>(Enumerable.Range(0, count).Select(n => TupSequenceST.Create(n)));
    }
  }

  ///===========================================================================
  /// <summary>
  /// A relation that is an array of text strings
  /// </summary>
  public class TupTextST : TupBase {
    public readonly static string Heading = "Seq,Line";

    public int Seq { get { return (int)Values[0]; } }
    public string Line { get { return (string)Values[1]; } }

    public static TupTextST Create(int Seq, string Line) {
      return Create<TupTextST>(new object[] { Seq, Line });
    }
  }

  public class RelTextST : RelValueST<TupTextST> {
    public static RelTextST Create(IList<string> text) {
      return RelTextST.Create<RelTextST>(Enumerable.Range(0, text.Count)
        .Select(n => TupTextST
        .Create(n, text[n])));
    }
  }
}
