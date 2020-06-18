/// Andl is A New Data Language. See http://andl.org.
///
/// Copyright © David M. Bennett 2015-20 as an unpublished work. All rights reserved.
/// 
/// This work is licensed under the Creative Commons Attribution-NonCommercial 4.0 International License. 
/// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc/4.0/.
/// 
/// In summary, you are free to share and adapt this software freely for any non-commercial purpose provided 
/// you give due attribution and do not impose additional restrictions.
/// 
/// This software is provided in the hope that it will be useful, but with 
/// absolutely no warranties. You assume all responsibility for its use.
/// 
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
    public void Basic() {

      var heading = "SNo:text,SName:text,Status:integer,City:text";
      var t1 = Tup.Data("S6", "White", 25, "Sydney");
      Assert.AreEqual(4, t1.Degree);
      Assert.IsTrue(t1.ToString().Contains("White"));
      Assert.AreEqual(1, t1.Count(v => v.ToString().Contains("White")));
      Assert.AreEqual(2, t1.Count(v => v.ToString().Contains("S")));
      Assert.AreEqual(t1, t1);

      var t1a = Tup.Data("S6", "White", 25, "Sydney");
      var t2 = Tup.Data("S7", "Black", 15, "London");
      Assert.AreEqual(t1, t1a);
      Assert.AreNotEqual(t1, t2);

      var sd = RelNode.Data(heading,
        Tup.Data("S6", "White", 25, "Sydney"),
        Tup.Data("S7", "Black", 15, "London"));
      Assert.AreEqual(4, sd.Degree);
      Assert.AreEqual(2, sd.Cardinality);
      Assert.AreEqual(1, sd.Count(t => t.ToString().Contains("Black")));
      Assert.AreEqual(2, sd.Count(t => t.ToString().Contains("S")));
      Assert.AreEqual(sd, sd);

      var sda = RelNode.Data(heading, t2, t1);
      Assert.AreEqual(sd.ToRelVar(), sda.ToRelVar());
      //Assert.AreEqual(sd.ToRelVar(), sda.ToRelVar());

    }

    [TestMethod]
    public void Import() {
      var si = RelNode.Import(SourceKind.Csv, Testdata, "S");
      Assert.AreEqual("S#,SNAME,STATUS,CITY", si.Heading.ToNames().Join(","));

      var s8i = RelNode.Import(SourceKind.Csv, Testdata, "S8", "SNo:text,SName:text,Status:integer,City:text");
      Assert.AreEqual("SNo,SName,Status,City", s8i.Heading.ToNames().Join(","));

      var pi = RelNode.Import(SourceKind.Csv, Testdata, "P", "PNo:text,PName:text,Color:text,Weight:number,City:text");
      Assert.AreEqual(2, pi.Count(t => t.ToString().Contains("Blue")));

      var spi = RelNode.Import(SourceKind.Csv, Testdata, "SP", "SNo:text,PNo:text,Qty:integer");
      Assert.AreEqual(3, spi.Degree);
      Assert.AreEqual(12, spi.Cardinality);

      var mmqi = RelNode.Import(SourceKind.Csv, Testdata, "MMQ", "MajorPNo:text,MinorPno:text,Qty:number");
      Assert.AreEqual(3, mmqi.Degree);

    }

    [TestMethod]
    public void Monadic() {
      var si = RelNode.Import(SourceKind.Csv, Testdata, "S");

      var siren = si.Rename("CITY,XXXCITY");
      Assert.AreEqual("S#,SNAME,STATUS,XXXCITY", siren.Heading.ToNames().Join(","));
      Assert.AreEqual(4, siren.Degree);
      Assert.AreEqual(5, siren.Cardinality);

      var sirenn = si.Rename("CITY,XXXCITY", "SNAME,XXXNAME");
      Assert.AreEqual("S#,XXXNAME,STATUS,XXXCITY", sirenn.Heading.ToNames().Join(","));
      Assert.AreEqual(4, sirenn.Degree);
      Assert.AreEqual(5, sirenn.Cardinality);

      var siext = si.Extend("STATUS,PlusA", Eval.Text(s => s + "A"));
      //      var siext = si.Extend("STATUS,PlusA", TupExtend.F(t => (string)t[0] + "A"));
      Assert.AreEqual("S#,SNAME,STATUS,CITY,PlusA", siext.Heading.ToNames().Join(","));
      Assert.AreEqual(5, siext.Degree);
      Assert.AreEqual(5, siext.Cardinality);
      Assert.AreEqual(2, siext.Count(t => t.ToString().Contains("30A")));
      Assert.AreEqual(1, siext.Count(t => t.ToString().Contains("S3,Blake,30,Paris,30A")));
      Assert.AreEqual("S3,Blake,30,Paris,30A", siext.Where(t => t.ToString().Contains("S3")).Join(";"));

      var sirep = si.Extend("STATUS,STATUS", Eval.Text(t => t + "B"));
      //var sirep = si.Extend("STATUS,STATUS", TupExtend.F(t => (string)t[0] + "B"));
      Assert.AreEqual("S#,SNAME,STATUS,CITY", sirep.Heading.ToNames().Join(","));
      Assert.AreEqual(4, sirep.Degree);
      Assert.AreEqual(5, sirep.Cardinality);
      Assert.AreEqual(2, sirep.Count(t => t.ToString().Contains("30B")));
      Assert.AreEqual("S3,Blake,30B,Paris", sirep.Where(t => t.ToString().Contains("S3")).Join(";"));
      Assert.AreEqual(1, sirep.Count(t => t.ToString().Contains("S3,Blake,30B,Paris")));

      //var sisel = si.Restrict("CITY", TupRestrict.F(t => (string)t[0] == "Paris"));
      var sisel = si.Restrict("CITY", Where.Text(t => t == "Paris"));
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
      var S = RelNode.Import(SourceKind.Csv, Testdata, "S");
      var P = RelNode.Import(SourceKind.Csv, Testdata, "P");
      var SP = RelNode.Import(SourceKind.Csv, Testdata, "SP");

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
      var S = RelNode.Import(SourceKind.Csv, Testdata, "S");
      var S8 = RelNode.Import(SourceKind.Csv, Testdata, "S8");

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
    public void Aggregate() {
      var P = RelNode.Import(SourceKind.Csv, Testdata, "P", "PNo:text,PName:text,Color:text,Weight:number,City:text");

      var pagg = P
        .Project("Color,Weight")
        .Aggregate("Weight,TotWeight", Eval.Number((v, a) => v + a));
      Assert.AreEqual(2, pagg.Degree);
      Assert.AreEqual("Color,TotWeight", pagg.Heading.ToNames().Join(","));
      Assert.AreEqual(3, pagg.Cardinality);
      Assert.AreEqual("Red,45.0;Green,17.0;Blue,29.0", pagg.Join(";"));
    }

    [TestMethod]
    public void TranClose() {
      var MM = RelNode.Import(SourceKind.Csv, Testdata, "MMQ")
        .Remove("QTY");

      var tc= MM.TranClose();
      Assert.AreEqual(2, tc.Degree);
      Assert.AreEqual("MAJOR_P#,MINOR_P#", tc.Heading.ToNames().Join(","));
      Assert.AreEqual(11, tc.Cardinality);
      Assert.AreEqual(5, tc.Count(t => t.ToString().Contains("P1")));
      Assert.AreEqual(1, tc.Count(t => t.ToString().Contains("P2,P5")));
      Assert.AreEqual(3, tc.Count(t => t.ToString().Contains("P6")));
    }

    [TestMethod]
    public void While() {
      var MMQ = RelNode.Import(SourceKind.Csv, Testdata, "MMQ", "MajorPNo:text,MinorPNo:text,Qty:number");
      var MM = MMQ.Remove("Qty");

      // BUG: need to preserve order of heading, easy to get it wrong 
      var wtc = MM.While(Eval.While(t => t
        .Rename("MinorPNo,zmatch")
        .Compose(MM.Rename("MajorPNo,zmatch"))));
      Assert.AreEqual(2, wtc.Degree);
      Assert.AreEqual("MajorPNo,MinorPNo", wtc.Heading.ToNames().Join(","));
      Assert.AreEqual(11, wtc.Cardinality);
      Assert.AreEqual(5, wtc.Count(t => t.ToString().Contains("P1")));
      Assert.AreEqual(1, wtc.Count(t => t.ToString().Contains("P2,P5")));
      Assert.AreEqual(3, wtc.Count(t => t.ToString().Contains("P6")));

      var mmexp = MMQ
        .Rename("Qty,ExpQty")
        .While(Eval.While(tw => tw
          .Rename("MinorPNo,zmatch")
          .Compose(MMQ.Rename("MajorPNo,zmatch"))
          .Extend("Qty,ExpQty,ExpQty", Eval.Number((v, w) => v * w))
          .Remove("Qty")));

      Assert.AreEqual(3, mmexp.Degree);
      Assert.AreEqual("MajorPNo,MinorPNo,ExpQty", mmexp.Heading.ToNames().Join(","));
      Assert.AreEqual(13, mmexp.Cardinality);
      Assert.AreEqual(7, mmexp.Count(t => t.ToString().Contains("P1")));
      Assert.AreEqual(1, mmexp.Count(t => t.ToString().Contains("P2,P5")));
      Assert.AreEqual(3, mmexp.Count(t => t.ToString().Contains("P6")));
      Assert.AreEqual(0, mmexp.Count(t => t.ToString().Contains("52")));

      var mmagg = mmexp
        .Aggregate("ExpQty,TotQty", Eval.Number((v, a) => v + a));
      Assert.AreEqual(3, mmagg.Degree);
      Assert.AreEqual("MajorPNo,MinorPNo,TotQty", mmagg.Heading.ToNames().Join(","));
      Assert.AreEqual(11, mmagg.Cardinality);
      Assert.AreEqual(5, mmagg.Count(t => t.ToString().Contains("P1")));
      Assert.AreEqual(1, mmagg.Count(t => t.ToString().Contains("P2,P5")));
      Assert.AreEqual(3, mmagg.Count(t => t.ToString().Contains("P6")));
      Assert.AreEqual(1, mmagg.Count(t => t.ToString().Contains("52")));
    }

    [TestMethod]
    public void GroupWrap() {
      var SP = RelNode.Import(SourceKind.Csv, Testdata, "SP");

      var spgrp = SP.Group("P#,QTY,PQ");
      Assert.AreEqual(2, spgrp.Degree);
      Assert.AreEqual(4, spgrp.Cardinality);
      Assert.AreEqual(2, spgrp.Count(t => t.ToString().Contains("P1")));
      Assert.AreEqual(4, spgrp.Count(t => t.ToString().Contains("P2")));
      Assert.AreEqual(1, spgrp.Count(t => t.ToString().Contains("P3")));
      Assert.AreEqual(2, spgrp.Count(t => t.ToString().Contains("P4")));
      Assert.AreEqual(2, spgrp.Count(t => t.ToString().Contains("P5")));
      Assert.AreEqual(1, spgrp.Count(t => t.ToString().Contains("P6")));

      var spugrp = spgrp.Ungroup("PQ");
      Assert.AreEqual(3, spugrp.Degree);
      Assert.AreEqual(12, spugrp.Cardinality);
      Assert.AreEqual(SP.ToRelVar(), spugrp.ToRelVar());

      var spwrp = SP.Wrap("P#,QTY,PQ");
      Assert.AreEqual(2, spwrp.Degree);
      Assert.AreEqual(12, spwrp.Cardinality);
      Assert.AreEqual(2, spwrp.Count(t => t.ToString().Contains("P1")));
      Assert.AreEqual(4, spwrp.Count(t => t.ToString().Contains("P2")));
      Assert.AreEqual(1, spwrp.Count(t => t.ToString().Contains("P3")));
      Assert.AreEqual(2, spwrp.Count(t => t.ToString().Contains("P4")));
      Assert.AreEqual(2, spwrp.Count(t => t.ToString().Contains("P5")));
      Assert.AreEqual(1, spwrp.Count(t => t.ToString().Contains("P6")));

      var spuwrp = spwrp.Unwrap("PQ");
      Assert.AreEqual(3, spuwrp.Degree);
      Assert.AreEqual(12, spuwrp.Cardinality);
      Assert.AreEqual(SP.ToRelVar(), spuwrp.ToRelVar());
    }

  }
}