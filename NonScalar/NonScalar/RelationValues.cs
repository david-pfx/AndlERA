﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndlEra {
  ///===========================================================================
  /// <summary>
  /// The two empty relations DUM and DEE
  /// </summary>
  public static class NoneData {
    // Empty relation empty header is DUM
    public static RelationBase<TupNone> Zero = RelNone.Create(new List<TupNone> { });
    // Full relation empty header is DEE
    public static RelationBase<TupNone> One = RelNone.Create(new List<TupNone> { new TupNone() });
  }

  /// <summary>
  /// Tuple with degree of zero
  /// </summary>
  public class RelNone : RelationBase<TupNone> { }

  public class TupNone : TupleBase {
    public readonly static string Heading = "";

    public static TupNone Create() {
      return Create<TupNone>(new object[] { });
    }
  }
  ///===========================================================================
  /// <summary>
  /// A relation that is a sequence of numbers
  /// </summary>
  public class TupSequence : TupleBase {
    public readonly static string Heading = "N";
    public int N { get { return (int)Values[0]; } }

    public static TupSequence Create(int N) {
      return Create<TupSequence>(new object[] { N });
    }
  }

  public class RelSequence : RelationBase<TupSequence> {
    public static RelSequence Create(int count) {
      return Create(Enumerable.Range(0, count).Select(n => TupSequence.Create(n)));
    }
  }

  ///===========================================================================
  /// <summary>
  /// A relation that is an array of text strings
  /// </summary>
  public class TupText : TupleBase {
    public readonly static string Heading = "Seq,Line";

    public int Seq { get { return (int)Values[0]; } }
    public string Line { get { return (string)Values[1]; } }

    public static TupText Create(int Seq, string Line) {
      return Create<TupText>(new object[] { Seq, Line });
    }
  }

  public class RelText : RelationBase<TupText> {
    public static RelText Create(IList<string> text) {
      return Create(Enumerable.Range(0, text.Count)
        .Select(n => TupText
        .Create(n, text[n])));
    }
  }
}
