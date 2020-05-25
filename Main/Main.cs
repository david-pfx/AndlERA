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
      //SampleBasic();
      SampleSource();
    }

    // basic samples, operators return relations
    static void SampleSource() {

      var si = RelationNode.Import(SourceKind.Csv, ".", "S", "SNo:text,SName:text,Status:integer,City:text");
      var s8i = RelationNode.Import(SourceKind.Csv, ".", "S8", "SNo:text,SName:text,Status:integer,City:text");
      var pi = RelationNode.Import(SourceKind.Csv, ".", "P", "PNo:text,PName:text,Color:text,Weight:number,City:text");
      var spi = RelationNode.Import(SourceKind.Csv, ".", "SP", "SNo:text,PNo:text,Qty:integer");

      var pagg = pi
        .Project("Color,Weight")
        .Aggregate("Weight,TotWeight", new TupAggregate((v, a) => (decimal)v + (decimal)a));
      WriteLine("Agg\n" + pagg.Format());

      //var sj = si.Join(spi);
      //var sj = si.Compose(spi);
      //var sj = si.Semijoin(spi);
      var sj = si.Antijoin(spi);
      WriteLine("Join\n" + sj.Format());

      var se = pi.Extend("Weight,WeightKg", new TupExtend(t => (decimal)t[0] * 0.454m));
      WriteLine("Extend\n" + se.Format());

      var sr = si.Rename("City,SCITY");
      WriteLine("Rename\n" + sr.Format());

      //var su = sn.Union(sn8);
      //var su = sn.Minus(sn8);
      var su = si.Intersect(s8i);
      WriteLine("Union\n" + su.Format());

      var ss = si.Select("City", new TupSelect(t => (string)t[0] == "Paris"));
      WriteLine("Select\n" + ss.Format());

      var sp = si.Project("City");
      WriteLine("Project\n" + sp.Format());
    }

    // basic samples, operators return relations
    static void SampleBasic() {
      WriteLine(RelSequence.Create(5));
      WriteLine(Supplier.SP);

      var v1 = RelVar<TupS>.Create(Supplier.S);
      WriteLine(v1);
      var v2 = RelVar<Tup1>.Create(v1.Value
        .Select(t => t.Status == 30)
        .Rename<TupSX>()    // "SNo", "SName", "Status", "Supplier City"
        .Project<Tup1>());    // "Supplier City"
      WriteLine(v2.Value.Format());

      WriteLine(Supplier.S
        .Extend<TupSX>(t => "XXX")    // "SNo", "SName", "Status", "Supplier City"
        .Format());

      WriteLine("Join");
      WriteLine(Supplier.P
        .Rename<TupPcolour>()                 //  "PNo", "PName", "Colour", "Weight", "City"
        .Select(t => t.Colour == "Red")
        .Project<TupPPno>()                   // "PNo", "PName"
        .Join<TupSP, TupPjoin>(Supplier.SP)   // "PNo", "PName", "SNo", "Qty"
        .Format());

      WriteLine("Aggregation");
      WriteLine(Supplier.SP
        .Aggregate<TupAgg,int>((t,a) => a + t.Qty)  //  "PNo", "TotQty"
        .Format());

      WriteLine("While");
      var seed = MMQData.MMQ.Extend<TupMMQA>(t => t.Qty);    // "Major", "Minor", "Qty", "AggQty"
      var zmq = MMQData.MMQ.Rename<TupzMQ>();    // "zmatch", "Minor", "Qty"
      var exp = seed.While(t => t
        .Rename<TupMzA>()             // "Major", "zmatch", "AggQty"
        .Join<TupzMQ, TupMMQA>(zmq)
        .Transform<TupMMQA>(tt => TupMMQA.Create(tt.Major, tt.Minor, 0, tt.AggQty * tt.Qty)));
      WriteLine(exp.Format());
      WriteLine("P1 -> P5");
      WriteLine(exp.Select(t => t.Major == "P1" && t.Minor == "P5")
        .Aggregate<TupMMT,int>((t,a) => a + t.AggQty)
        .Format());

      WriteLine("Insert P6 P7");
      v1.Insert(
        RelS.Create<RelS>(
          new List<TupS> {
            TupS.Create( "S6", "White", 25, "Paris" ),
            TupS.Create( "S7", "Black", 15, "London" ),
          })
        );
      WriteLine(v1.Format());

      WriteLine("Move to Sydney");
      v1.Update(t => t.City == "Paris", t => TupS.Create(t.SNo, t.SName, t.Status, "Sydney"));
      WriteLine(v1.Format());

      WriteLine("Delete Sydneysiders");
      v1.Delete(t => t.City == "Sydney");
      WriteLine(v1.Format());
    }
  }


  public class TupSX : TupleBase {
    public readonly static string[] Heading = { "SNo", "SName", "Status", "Supplier City" };
  }
  public class Tup1 : TupleBase {
    public readonly static string[] Heading = { "Supplier City" };
  }
  public class TupSSP : TupleBase {
    public readonly static string[] Heading = { "SNo", "SName", "Status", "City", "PNo", "Qty" };
  }
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
  public class TupAgg : TupleBase {
    public readonly static string[] Heading = { "PNo", "TotQty" };
  }

  public class RelMMQA : RelationBase<TupMMQ> { }
  public class TupMMQA : TupMMQ {
    new public readonly static string[] Heading = { "Major", "Minor", "Qty", "AggQty" };
    public int AggQty { get { return (int)Values[3]; } }
    public static TupMMQA Create(string major, string minor, int qty, int aggqty) {
      return Create<TupMMQA>(new object[] { major, minor, qty, aggqty });
    }
  }
  public class TupMzA : TupleBase {
    public readonly static string[] Heading = { "Major", "zmatch", "AggQty" };
  }
  public class TupzMQ : TupleBase {
    public readonly static string[] Heading = { "zmatch", "Minor", "Qty" };
  }
  public class TupMMT : TupleBase {
    public readonly static string[] Heading = { "Major", "Minor", "TotQty" };
  }


}
