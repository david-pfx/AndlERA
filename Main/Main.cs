using System;
using System.Collections.Generic;
using System.Linq;

using Andl.Common;
using SupplierData;

namespace AndlEra {
  class Program {
    static void Main(string[] args) {
      Console.WriteLine("Andl Era");
      Exec();
    }

    private static void Exec() {
      Console.WriteLine(RelSequence.Create(5));
      Console.WriteLine(Supplier.S);
      //Console.WriteLine(Supplier.S.Select(t => t.Status < 20));
      //Console.WriteLine(RelSX.Rename(Supplier.S));
      //Console.WriteLine(Rel1.Project(Supplier.S));

      Console.WriteLine(Rel1.Project(RelSX.Rename(Supplier.S.Select(t => t.Status == 30))));
      //Console.WriteLine(RelSSP.Join(Supplier.S, Supplier.SP));
      Console.WriteLine(RelSSP.Join<TupSSP,TupS, TupSP>(Supplier.S, Supplier.SP).Format());

      //Console.WriteLine(Supplier.S.Rename<RelSX>());
      p(TupSX.Heading);
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



}
