using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using AndlEra;
using Andl.Common;
using System.Linq;

namespace TestSuite {
  [TestClass]
  public class TestSet1 {
    const string Testdata = @".\Data";

    [TestMethod]
    public void Import() {
      var si = RelationNode.Import(SourceKind.Csv, Testdata, "S");
      Assert.AreEqual("S#,SNAME,STATUS,CITY", si.Heading.ToNames().Join(","));

      var s8i = RelationNode.Import(SourceKind.Csv, Testdata, "S8", "SNo:text,SName:text,Status:integer,City:text");
      Assert.AreEqual("SNo,SName,Status,City", s8i.Heading.ToNames().Join(","));

      var pi = RelationNode.Import(SourceKind.Csv, Testdata, "P", "PNo:text,PName:text,Color:text,Weight:number,City:text");
      Assert.AreEqual(2, pi.Count(t => t.ToString().Contains("Blue")));

      var spi = RelationNode.Import(SourceKind.Csv, Testdata, "SP", "SNo:text,PNo:text,Qty:integer");
      Assert.AreEqual(12, spi.Cardinality);

      var mmqi = RelationNode.Import(SourceKind.Csv, Testdata, "MMQ", "MajorPNo:text,MinorPno:text,Qty:number");
      Assert.AreEqual(3, mmqi.Degree);
    }

    [TestMethod]
    public void Monadic() {
      var si = RelationNode.Import(SourceKind.Csv, Testdata, "S");

      var siren = si.Rename("CITY,XXXCITY");
      Assert.AreEqual("S#,SNAME,STATUS,XXXCITY", siren.Heading.ToNames().Join(","));
      Assert.AreEqual(4, siren.Degree);
      Assert.AreEqual(5, siren.Cardinality);

      var siext = si.Extend("STATUS,PlusA", TupExtend.F(t => (string)t[0] + "A"));
      Assert.AreEqual("S#,SNAME,STATUS,CITY,PlusA", siext.Heading.ToNames().Join(","));
      Assert.AreEqual(5, siext.Degree);
      Assert.AreEqual(5, siext.Cardinality);
      Assert.AreEqual(2, siext.Count(t => t.ToString().Contains("30A")));
      Assert.AreEqual(1, siext.Count(t => t.ToString().Contains("S3,Blake,30,Paris,30A")));
      Assert.AreEqual("S3,Blake,30,Paris,30A", siext.Where(t => t.ToString().Contains("S3")).Join(";"));

      var sirep = si.Extend("STATUS,STATUS", TupExtend.F(t => (string)t[0] + "B"));
      Assert.AreEqual("S#,SNAME,STATUS,CITY", sirep.Heading.ToNames().Join(","));
      Assert.AreEqual(4, sirep.Degree);
      Assert.AreEqual(5, sirep.Cardinality);
      Assert.AreEqual(2, sirep.Count(t => t.ToString().Contains("30B")));
      Assert.AreEqual("S3,Blake,30B,Paris", sirep.Where(t => t.ToString().Contains("S3")).Join(";"));
      Assert.AreEqual(1, sirep.Count(t => t.ToString().Contains("S3,Blake,30B,Paris")));

      var sisel = si.Restrict("CITY", TupRestrict.F(t => (string)t[0] == "Paris"));
      Assert.AreEqual("S#,SNAME,STATUS,CITY", sisel.Heading.ToNames().Join(","));
      Assert.AreEqual(4, sisel.Degree);
      Assert.AreEqual(2, sisel.Cardinality);
      Assert.AreEqual(1, sisel.Count(t => t.ToString().Contains("30")));
      Assert.AreEqual("S3,Blake,30,Paris", sisel.Where(t => t.ToString().Contains("S3")).Join(";"));
      Assert.AreEqual(2, sisel.Count(t => t.ToString().Contains("Paris")));
      Assert.AreEqual(1, sisel.Count(t => t.ToString().Contains("S3,Blake,30,Paris")));

      var sipro = si.Project("CITY");
      Assert.AreEqual("CITY", sipro.Heading.ToNames().Join(","));
      Assert.AreEqual(1, sipro.Degree);
      Assert.AreEqual(3, sipro.Cardinality);
      Assert.AreEqual("Paris", sipro.Where(t => t.ToString().Contains("Paris")).Join(";"));
      Assert.AreEqual(1, sipro.Count(t => t.ToString().Contains("London")));
      Assert.AreEqual(1, sipro.Count(t => t.ToString().Contains("Paris")));
      Assert.AreEqual(1, sipro.Count(t => t.ToString().Contains("Athens")));

      var sirem = si.Remove("S#,SNAME");
      Assert.AreEqual("STATUS,CITY", sirem.Heading.ToNames().Join(","));
      Assert.AreEqual(2, sirem.Degree);
      Assert.AreEqual(4, sirem.Cardinality);
      Assert.AreEqual(1, sirem.Count(t => t.ToString().Contains("London")));
      Assert.AreEqual(2, sirem.Count(t => t.ToString().Contains("Paris")));
      Assert.AreEqual(1, sirem.Count(t => t.ToString().Contains("Athens")));
    }

    [TestMethod]
    public void Join() {
      var S = RelationNode.Import(SourceKind.Csv, Testdata, "S");
      var P = RelationNode.Import(SourceKind.Csv, Testdata, "P");
      var SP = RelationNode.Import(SourceKind.Csv, Testdata, "SP");

      var jfull = P.Join(SP);
      Assert.AreEqual(7, jfull.Degree);
      Assert.AreEqual("P#,PNAME,COLOR,WEIGHT,CITY,S#,QTY", jfull.Heading.ToNames().Join(","));
      Assert.AreEqual(12, jfull.Cardinality);
      Assert.AreEqual(2, jfull.Count(t => t.ToString().Contains("P1")));
      Assert.AreEqual(1, jfull.Count(t => t.ToString().Contains("P6")));
      Assert.AreEqual(2, jfull.Count(t => t.ToString().Contains("Nut")));
      Assert.AreEqual(4, jfull.Count(t => t.ToString().Contains("Bolt")));
      Assert.AreEqual(3, jfull.Count(t => t.ToString().Contains("Screw")));
      Assert.AreEqual(2, jfull.Count(t => t.ToString().Contains("Cam")));
      Assert.AreEqual(1, jfull.Count(t => t.ToString().Contains("Cog")));

      var jcomp = P.Compose(SP);
      Assert.AreEqual(6, jcomp.Degree);
      Assert.AreEqual("PNAME,COLOR,WEIGHT,CITY,S#,QTY", jcomp.Heading.ToNames().Join(","));
      Assert.AreEqual(12, jcomp.Cardinality);
      Assert.AreEqual(0, jcomp.Count(t => t.ToString().Contains("P1")));
      Assert.AreEqual(0, jcomp.Count(t => t.ToString().Contains("P6")));
      Assert.AreEqual(2, jcomp.Count(t => t.ToString().Contains("Nut")));
      Assert.AreEqual(4, jcomp.Count(t => t.ToString().Contains("Bolt")));
      Assert.AreEqual(3, jcomp.Count(t => t.ToString().Contains("Screw")));
      Assert.AreEqual(2, jcomp.Count(t => t.ToString().Contains("Cam")));
      Assert.AreEqual(1, jcomp.Count(t => t.ToString().Contains("Cog")));

      var jsemi = S.Semijoin(SP);
      Assert.AreEqual(4, jsemi.Degree);
      Assert.AreEqual("S#,SNAME,STATUS,CITY", jsemi.Heading.ToNames().Join(","));
      Assert.AreEqual(4, jsemi.Cardinality);
      Assert.AreEqual(1, jsemi.Count(t => t.ToString().Contains("S1")));
      Assert.AreEqual(0, jsemi.Count(t => t.ToString().Contains("S5")));
      Assert.AreEqual(2, jsemi.Count(t => t.ToString().Contains("London")));
      Assert.AreEqual(2, jsemi.Count(t => t.ToString().Contains("Paris")));
      Assert.AreEqual(0, jsemi.Count(t => t.ToString().Contains("Athens")));

      var janti = S.Antijoin(SP);
      Assert.AreEqual(4, janti.Degree);
      Assert.AreEqual("S#,SNAME,STATUS,CITY", janti.Heading.ToNames().Join(","));
      Assert.AreEqual(1, janti.Cardinality);
      Assert.AreEqual(0, janti.Count(t => t.ToString().Contains("S1")));
      Assert.AreEqual(1, janti.Count(t => t.ToString().Contains("S5")));
      Assert.AreEqual(0, janti.Count(t => t.ToString().Contains("London")));
      Assert.AreEqual(0, janti.Count(t => t.ToString().Contains("Paris")));
      Assert.AreEqual(1, janti.Count(t => t.ToString().Contains("Athens")));
    }

    [TestMethod]
    public void Set() {
      var S = RelationNode.Import(SourceKind.Csv, Testdata, "S");
      var S8 = RelationNode.Import(SourceKind.Csv, Testdata, "S8");

      var juni = S.Union(S8);
      Assert.AreEqual(4, juni.Degree);
      Assert.AreEqual("S#,SNAME,STATUS,CITY", juni.Heading.ToNames().Join(","));
      Assert.AreEqual(9, juni.Cardinality);
      Assert.AreEqual(2, juni.Count(t => t.ToString().Contains("London")));
      Assert.AreEqual(2, juni.Count(t => t.ToString().Contains("Paris")));
      Assert.AreEqual(1, juni.Count(t => t.ToString().Contains("Athens")));
      Assert.AreEqual(4, juni.Count(t => t.ToString().Contains("Sydney")));

      var jmin = S.Minus(S8);
      Assert.AreEqual(4, jmin.Degree);
      Assert.AreEqual("S#,SNAME,STATUS,CITY", jmin.Heading.ToNames().Join(","));
      Assert.AreEqual(1, jmin.Cardinality);
      Assert.AreEqual(0, jmin.Count(t => t.ToString().Contains("London")));
      Assert.AreEqual(0, jmin.Count(t => t.ToString().Contains("Paris")));
      Assert.AreEqual(1, jmin.Count(t => t.ToString().Contains("Athens")));
      Assert.AreEqual(0, jmin.Count(t => t.ToString().Contains("Sydney")));

      var jint = S.Intersect(S8);
      Assert.AreEqual(4, jint.Degree);
      Assert.AreEqual("S#,SNAME,STATUS,CITY", jint.Heading.ToNames().Join(","));
      Assert.AreEqual(4, jint.Cardinality);
      Assert.AreEqual(2, jint.Count(t => t.ToString().Contains("London")));
      Assert.AreEqual(2, jint.Count(t => t.ToString().Contains("Paris")));
      Assert.AreEqual(0, jint.Count(t => t.ToString().Contains("Athens")));
      Assert.AreEqual(0, jint.Count(t => t.ToString().Contains("Sydney")));
    }
    [TestMethod]
    public void While() {
      var MMQ = RelationNode.Import(SourceKind.Csv, Testdata, "MMQ");
      var MM = MMQ.Remove("QTY");

      // BUG: need to preserve order of heading, easy to get it wrong 
      var wtc = MM.While(TupWhile.F(t => t
        .Rename("MINOR_P#,zzz")
        .Compose(MM
          .Rename("MAJOR_P#,zzz"))));
      Assert.AreEqual(2, wtc.Degree);
      Assert.AreEqual("MAJOR_P#,MINOR_P#", wtc.Heading.ToNames().Join(","));
      Assert.AreEqual(11, wtc.Cardinality);
      Assert.AreEqual(5, wtc.Count(t => t.ToString().Contains("P1")));
      Assert.AreEqual(1, wtc.Count(t => t.ToString().Contains("P2,P5")));
      Assert.AreEqual(3, wtc.Count(t => t.ToString().Contains("P6")));
    }
    [TestMethod]
    public void Aggregate() {
      var P = RelationNode.Import(SourceKind.Csv, Testdata, "P", "PNo:text,PName:text,Color:text,Weight:number,City:text");

      var pagg = P
        .Project("Color,Weight")
        .Aggregate("Weight,TotWeight", TupAggregate.F((v, a) => (decimal)v + (decimal)a));
      Assert.AreEqual(2, pagg.Degree);
      Assert.AreEqual("Color,TotWeight", pagg.Heading.ToNames().Join(","));
      Assert.AreEqual(3, pagg.Cardinality);
      Assert.AreEqual("Red,45.0;Green,17.0;Blue,29.0", pagg.Join(";"));
    }
  }
}