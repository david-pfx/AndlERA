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
      SampleWithTypes();
      SampleWithHeading();
    }

    static void Show(string msg, object value) {
      WriteLine(msg);
      WriteLine(value.ToString());
      WriteLine("----------");
    }

    // basic samples, operators return relations
    static void SampleWithHeading() {

      var si = RelationNode.Import(SourceKind.Csv, ".", "S", "SNo:text,SName:text,Status:integer,City:text");
      var s8i = RelationNode.Import(SourceKind.Csv, ".", "S8", "SNo:text,SName:text,Status:integer,City:text");
      var pi = RelationNode.Import(SourceKind.Csv, ".", "P", "PNo:text,PName:text,Color:text,Weight:number,City:text");
      var spi = RelationNode.Import(SourceKind.Csv, ".", "SP", "SNo:text,PNo:text,Qty:integer");
      var mmqi = RelationNode.Import(SourceKind.Csv, ".", "MMQ", "MajorPNo:text,MinorPNo:text,Qty:number");

      Show("Extend", 
        pi.Extend("Weight,WeightKg", new TupExtend(t => (decimal)t[0] * 0.454m))
          .Format());

      Show("Replace",
        pi.Extend("Weight,Weight", new TupExtend(t => (decimal)t[0] * 0.454m))
        .Format());

      var mmexp = mmqi
        .Rename("Qty,ExpQty")
        .While(new TupWhile(tw => tw
          .Rename("MinorPNo,zmatch")
          .Compose(mmqi.Rename("MajorPNo,zmatch"))
          .Extend("Qty,ExpQty,ExpQty", new TupExtend(t => (decimal)t[0] * (decimal)t[1]))
          .Remove("Qty")
      ));
      Show("While", mmexp.Format());
      Show("Aggregated", mmexp
        .Aggregate("ExpQty,TotQty", new TupAggregate((v, a) => (decimal)v + (decimal)a))
        .Format());

      var pgrp = spi
        .Group("PNo,Qty,PQ");
        //.Wrap("PNo,Qty,PQ");
      Show("Group", pgrp 
        .Format());

      Show("Ungroup", pgrp
        .Ungroup("PQ")
        //.Unwrap("PQ")
        .Format());

      Show("Agg", pi
        .Project("Color,Weight")
        .Aggregate("Weight,TotWeight", new TupAggregate((v, a) => (decimal)v + (decimal)a))
        .Format());

      Show("Join", si
        //Join(spi);
        //Compose(spi);
        //Semijoin(spi);
        .Antijoin(spi)
        .Format());

      Show("Rename", si
        .Rename("City,SCITY")
        .Format());

      Show("Union", si
        //.Union(sn8);
        //.Minus(sn8);
        .Intersect(s8i)
        .Format());

      Show("Select", si
        .Select("City", new TupSelect(t => (string)t[0] == "Paris"))
        .Format());

      Show("Project", si
        .Project("City")
        .Format());
    }

    // basic samples, operators return relations
    static void SampleWithTypes() {
      Show("Seq", RelSequence.Create(5));
      Show("SP", Supplier.SP);

      var v1 = new RelVar<TupS>(Supplier.S);
      var v2 = new RelVar<Tup1>(v1.Value
        .Select(t => t.Status == 30)
        .Rename<TupSX>()    // "SNo", "SName", "Status", "Supplier City"
        .Project<Tup1>());    // "Supplier City"
      Show("v2", v2.Value.Format());

      Show("Extend", Supplier.S
        .Extend<TupSX>(t => "XXX")    // "SNo", "SName", "Status", "Supplier City"
        .Format());

      Show("Join", Supplier.P
        .Rename<TupPcolour>()                 //  "PNo", "PName", "Colour", "Weight", "City"
        .Select(t => t.Colour == "Red")
        .Project<TupPPno>()                   // "PNo", "PName"
        .Join<TupSP, TupPjoin>(Supplier.SP)   // "PNo", "PName", "SNo", "Qty"
        .Format());

      Show("Aggregation", Supplier.SP
        .Aggregate<TupAgg,int>((t,a) => a + t.Qty)  //  "PNo", "TotQty"
        .Format());

      var seed = MMQData.MMQ.Extend<TupMMQA>(t => t.Qty);    // "Major", "Minor", "Qty", "AggQty"
      var zmq = MMQData.MMQ.Rename<TupzMQ>();    // "zmatch", "Minor", "Qty"
      var exp = seed.While(t => t
        .Rename<TupMzA>()             // "Major", "zmatch", "ExpQty"
        .Join<TupzMQ, TupMMQA>(zmq)
        .Transform<TupMMQA>(tt => TupMMQA.Create(tt.Major, tt.Minor, 0, tt.AggQty * tt.Qty)));
      Show("While", exp.Format());
      Show("P1 -> P5", exp
        .Select(t => t.Major == "P1" && t.Minor == "P5")
        .Aggregate<TupMMT,int>((t,a) => a + t.AggQty)
        .Format());

      v1.Insert(
        RelS.Create<RelS>(
          new List<TupS> {
            TupS.Create( "S6", "White", 25, "Paris" ),
            TupS.Create( "S7", "Black", 15, "London" ),
          })
        );
      Show("Insert P6 P7", v1.Format());

      v1.Update(t => t.City == "Paris", t => TupS.Create(t.SNo, t.SName, t.Status, "Sydney"));
      Show("Move to Sydney", v1.Format());

      v1.Delete(t => t.City == "Sydney");
      Show("Delete Sydneysiders", v1.Format());
    }
  }


  public class TupSX : TupleBase {
    public readonly static string Heading = "SNo,SName,Status,Supplier City";
  }
  public class Tup1 : TupleBase {
    public readonly static string Heading = "Supplier City";
  }
  public class TupSSP : TupleBase {
    public readonly static string Heading = "SNo,SName,Status,City,PNo,Qty";
  }
  public class TupPcolour : TupleBase {
    public readonly static string Heading = "PNo,PName,Colour,Weight,City";
    public string Colour { get { return (string)Values[2]; } }
  }

  public class TupPPno : TupleBase {
    public readonly static string Heading = "PNo,PName";
  }

  public class TupPjoin : TupleBase {
    public readonly static string Heading = "PNo,PName,SNo,Qty";
  }
  public class TupAgg : TupleBase {
    public readonly static string Heading = "PNo,TotQty";
  }

  public class RelMMQA : RelationBase<TupMMQ> { }
  public class TupMMQA : TupMMQ {
    new public readonly static string Heading = "Major,Minor,Qty,AggQty";
    public int AggQty { get { return (int)Values[3]; } }
    public static TupMMQA Create(string major, string minor, int qty, int aggqty) {
      return Create<TupMMQA>(new object[] { major, minor, qty, aggqty });
    }
  }
  public class TupMzA : TupleBase {
    public readonly static string Heading = "Major,zmatch,AggQty";
  }
  public class TupzMQ : TupleBase {
    public readonly static string Heading = "zmatch,Minor,Qty";
  }
  public class TupMMT : TupleBase {
    public readonly static string Heading = "Major,Minor,TotQty";
  }


}
