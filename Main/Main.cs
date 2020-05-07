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
      Console.WriteLine(new RelSequence(5));
      Console.WriteLine(Supplier.S);
      Console.WriteLine(Supplier.S.Select(t => t.Status < 20));
      //Console.WriteLine(Supplier.S.Rename<RelSX>());
      //foreach (var t in new RelSequence(5))
      //  Console.WriteLine(t);
      //foreach (var t in Supplier.S)
      //  Console.WriteLine(t);
    }
  }

  public class TupSX : TupleBase {
    public readonly static string[] Heading = { "SNo", "Supplier Name", "Status", "City" };
  }
  public class RelSX : RelationBase<TupSX> {
    public RelSX(IList<TupSX> tuples) : base(tuples) {
    }
  }

}
