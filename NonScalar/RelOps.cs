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
  public class TupSequence : TupleBase {
    public readonly static string[] Heading = { "N" };
    public int N { get { return (int)_values[0]; } }

    public static TupSequence Create(int N) {
      var tuple = new TupSequence();
      tuple.Init(new object[] { N });
      return tuple;
    }
  }

  public class RelSequence : RelationBase<TupSequence> {
    public RelSequence(int count)
      : base(Enumerable.Range(0, count).Select(n => TupSequence.Create(n))) {
    }

  }

  ///===========================================================================
  /// <summary>
  /// 
  /// </summary>
  public class TupText : TupleBase {
    public readonly static string[] Heading = { "Seq", "Line" };

    public int Seq { get { return (int)_values[0]; } }
    public string Line { get { return (string)_values[1]; } }

    public static TupText Create(int Seq, string Line) {
      var tuple = new TupText();
      tuple.Init(new object[] { Seq, Line });
      return tuple;
    }
  }

  public class RelText : RelationBase<TupText> {
    public RelText(IList<string> text)
      : base(Enumerable.Range(0, text.Count).Select(n => TupText.Create(n, text[n]))) {
    }

  }
}
