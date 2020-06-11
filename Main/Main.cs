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
      WriteLine(SampleWithHeading() && SampleWithTypes());
    }

    static void Show(string msg, params RelNode[] node) {
      Show(msg, node.Select(n => n.Format()));
    }

    static void Show(string msg, params RelVar[] rvar) {
      Show(msg, rvar.Select(n => n.Format()));
    }

    static void Show(string msg, params object[] value) {
      Show(msg, value.Select(n => n.ToString()));
    }
    static void Show(string msg, IEnumerable<string> values) {
      WriteLine(msg);
      foreach (var value in values) {
        WriteLine(value);
        WriteLine("----------");
      }
    }

    // basic samples, operators return relations
    static bool SampleWithHeading() {

      var si = RelNode.Import(SourceKind.Csv, ".", "S", "SNo:text,SName:text,Status:integer,City:text");
      var s8i = RelNode.Import(SourceKind.Csv, ".", "S8", "SNo:text,SName:text,Status:integer,City:text");
      var pi = RelNode.Import(SourceKind.Csv, ".", "P", "PNo:text,PName:text,Color:text,Weight:number,City:text");
      var spi = RelNode.Import(SourceKind.Csv, ".", "SP", "SNo:text,PNo:text,Qty:integer");

      Show("Restrict", si
        .Restrict("City", RelCon.Func<string,bool>(v => v == "Paris")));
        //.Restrict("City", new FuncValue<string,bool>(v => v == "Paris")));

      Show("Project", si
        .Project("City"));

      Show("Rename", si
        .Rename("SNo,S#","SName,NAME","City,SCITY"));

      Show("Extend",
        pi.Extend("Weight,WeightKg", RelCon.Func<decimal>(a => a * 0.454m)));

      Show("Replace",
        pi.Extend("Weight,Weight", RelCon.Func<decimal>(a => a * 0.454m)));

      Show("Joins", 
        si.Join(spi),
        si.Compose(spi),
        si.Semijoin(spi),
        si.Antijoin(spi));

      Show("Union", 
        si.Union(s8i),
        si.Minus(s8i),
        si.Intersect(s8i)
        );

      Show("Group",
        spi.Group("PNo,Qty,PQ"),
        spi.Group("PNo,Qty,PQ").Ungroup("PQ"));

      Show("Wrap", 
        spi.Wrap("PNo,Qty,PQ"),
        spi.Wrap("PNo,Qty,PQ").Unwrap("PQ"));

      Show("Agg", pi
        .Project("Color,Weight")
        .Aggregate("Weight,TotWeight", RelCon.Agg<decimal>((v, a) => v + a))
        );

      //var orgi = RelNode.Import(SourceKind.Csv, ".", "orgchart");
      //Show("TClose", orgi
      //  .TranClose());
      var mmi = RelNode.Import(SourceKind.Csv, ".", "MMQ")
        .Remove("QTY");
      Show("TClose", mmi
        .TranClose());

      var mmqi = RelNode.Import(SourceKind.Csv, ".", "MMQ", "MajorPNo:text,MinorPNo:text,Qty:number");

      var mmexp = mmqi
        .Rename("Qty,ExpQty")
        .While(RelCon.While(tw => tw
          .Rename("MinorPNo,zmatch")
          .Compose(mmqi.Rename("MajorPNo,zmatch"))
          .Extend("Qty,ExpQty,ExpQty", RelCon.Func<decimal>((v,w) => v * w))
          .Remove("Qty")));
      Show("While", mmexp);
      Show("Aggregated", mmexp
        .Aggregate("ExpQty,TotQty", RelCon.Agg<decimal>((v, a) => v + a))
        );

      var v1 = new RelVar(si);
      Show("RelVar", v1);
      v1.Insert(RelNode.Data("SNo:text,SName:text,Status:integer,City:text",
        Tup.Data("S6", "White", 25, "Paris"),
        Tup.Data("S7", "Black", 15, "London")));

      Show("Insert P6 P7", v1);

      v1.Update("Status,City", v => (int)v[0] >= 25, "Sydney");
      Show("Move to Sydney", v1);

      v1.Delete("City", v => (string)v[0] == "Sydney");
      Show("Delete Sydneysiders", v1);

      return true;

    }

    // basic samples, operators return relations
    static bool SampleWithTypes() {
      Show("Seq", RelSequence.Create(5));
      Show("SP", Supplier.SP);

      var v1 = new RelVar<TupS>(Supplier.S);
      var v2 = new RelVar<Tup1>(v1.Value
        .Restrict(t => t.Status == 30)
        .Rename<TupSX>()    // "SNo", "SName", "Status", "Supplier City"
        .Project<Tup1>());    // "Supplier City"
      Show("v2", v2.Value);

      Show("Extend", Supplier.S
        .Extend<TupSX>(t => "XXX")    // "SNo", "SName", "Status", "Supplier City"
        );

      Show("Join", Supplier.P
        .Rename<TupPcolour>()                 //  "PNo", "PName", "Colour", "Weight", "City"
        .Restrict(t => t.Colour == "Red")
        .Project<TupPPno>()                   // "PNo", "PName"
        .Join<TupSP, TupPjoin>(Supplier.SP)   // "PNo", "PName", "SNo", "Qty"
        );

      Show("Aggregation", Supplier.SP
        .Aggregate<TupAgg,int>((t,a) => a + t.Qty)  //  "PNo", "TotQty"
        );

      var seed = MMQData.MMQ.Extend<TupMMQA>(t => t.Qty);    // "Major", "Minor", "Qty", "AggQty"
      var zmq = MMQData.MMQ.Rename<TupzMQ>();    // "zmatch", "Minor", "Qty"
      var exp = seed.While<RelMMQA>(t => t
        .Rename<TupMzA>()             // "Major", "zmatch", "ExpQty"
        .Join<TupzMQ, TupMMQA>(zmq)
        .Transform<TupMMQA>(tt => TupMMQA.Create(tt.Major, tt.Minor, 0, tt.AggQty * tt.Qty)));
      Show("While", exp);
      Show("P1 -> P5", exp
        .Restrict(t => t.Major == "P1" && t.Minor == "P5")
        .Aggregate<TupMMT, int>((t, a) => a + t.AggQty));

      v1.Insert(
        RelS.Create<RelS>(
          new List<TupS> {
            TupS.Create( "S6", "White", 25, "Paris" ),
            TupS.Create( "S7", "Black", 15, "London" ),
          })
        );
      Show("Insert P6 P7", v1);

      v1.Update(t => t.City == "Paris", t => TupS.Create(t.SNo, t.SName, t.Status, "Sydney"));
      Show("Move to Sydney", v1);

      v1.Delete(t => t.City == "Sydney");
      Show("Delete Sydneysiders", v1);
      return true;
    }
  }


  public class TupSX : TupBase {
    public readonly static string Heading = "SNo,SName,Status,Supplier City";
  }
  public class Tup1 : TupBase {
    public readonly static string Heading = "Supplier City";
  }
  public class TupSSP : TupBase {
    public readonly static string Heading = "SNo,SName,Status,City,PNo,Qty";
  }
  public class TupPcolour : TupBase {
    public readonly static string Heading = "PNo,PName,Colour,Weight,City";
    public string Colour { get { return (string)Values[2]; } }
  }

  public class TupPPno : TupBase {
    public readonly static string Heading = "PNo,PName";
  }

  public class TupPjoin : TupBase {
    public readonly static string Heading = "PNo,PName,SNo,Qty";
  }
  public class TupAgg : TupBase {
    public readonly static string Heading = "PNo,TotQty";
  }

  public class RelMMQA : RelValue<TupMMQA> { }
  public class TupMMQA : TupMMQ {
    new public readonly static string Heading = "Major,Minor,Qty,AggQty";
    public int AggQty { get { return (int)Values[3]; } }
    public static TupMMQA Create(string major, string minor, int qty, int aggqty) {
      return Create<TupMMQA>(new object[] { major, minor, qty, aggqty });
    }
  }
  public class TupMzA : TupBase {
    public readonly static string Heading = "Major,zmatch,AggQty";
  }
  public class TupzMQ : TupBase {
    public readonly static string Heading = "zmatch,Minor,Qty";
  }
  public class TupMMT : TupBase {
    public readonly static string Heading = "Major,Minor,TotQty";
  }


}
