using System;
using System.Collections.Generic;
using System.Linq;

using Andl.Common;
using SupplierData;
using static System.Console;

namespace AndlEra {
  class Program {
    static void Main(string[] args) {
      WriteLine("Andl Era");
      Exec();
    }

    private static void Exec() {
      WriteLine(RelSequence.Create(5));
      WriteLine(Supplier.SP);

      var v1 = RelVar<TupS>.Create(Supplier.S);
      WriteLine(v1);
      var v2 = RelVar<Tup1>.Create(v1.Value
        .Select(t => t.Status == 30)
        .Rename<TupSX>()    // "SNo", "SName", "Status", "Supplier City"
        .Project<Tup1>());    // "Supplier City"
      WriteLine(v2.Value.Format());

      //WriteLine(Supplier.S.Select(t => t.Status == 30)
      //  .Rename<TupSX>()    // "SNo", "SName", "Status", "Supplier City"
      //  .Project<Tup1>()    // "Supplier City"
      //  .Format());

      WriteLine(Supplier.P
        .Rename<TupPcolour>()                 //  "PNo", "PName", "Colour", "Weight", "City"
        .Select(t => t.Colour == "Red")
        .Project<TupPPno>()                   // "PNo", "PName"
        .Join<TupSP, TupPjoin>(Supplier.SP)   // "PNo", "PName", "SNo", "Qty"
        .Format());
    }
  }


  public class TupSX : TupleBase {
    public readonly static string[] Heading = { "SNo", "SName", "Status", "Supplier City" };
  }
  public class RelSX : RelationBase<TupSX> { }
  public class Tup1 : TupleBase {
    public readonly static string[] Heading = { "Supplier City" };
  }
  public class Rel1 : RelationBase<Tup1> { }

  public class TupSSP : TupleBase {
    public readonly static string[] Heading = { "SNo", "SName", "Status", "City", "PNo", "Qty" };
  }
  public class RelSSP : RelationBase<TupSSP> { }

  public class TupPcolour : TupleBase {
    public readonly static string[] Heading = { "PNo", "PName", "Colour", "Weight", "City" };
    public string Colour { get { return (string)Values[2]; } }
  }

  public class TupPPno : TupleBase {
    public readonly static string[] Heading = { "PNo", "PName" };
  }

  public class TupPjoin : TupleBase {
    public readonly static string[] Heading = { "PNo", "PName", "SNo", "Qty" };
  }


}
