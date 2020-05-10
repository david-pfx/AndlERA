using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndlEra {
  public static class RelOpExtension {
    ///===========================================================================
    /// relational operators as extensions 
    /// experimental, not needed
    /// 

    //public static RelationBase<Tnew> Rename<Ttup, Tnew>(this RelationBase<Ttup> relation)
    //where Ttup : TupleBase, new()
    //where Tnew : TupleBase, new() {

    //  var newbody = RelOps.Rename<Ttup, Tnew>(relation.Body);
    //  return RelationBase<Tnew>.Create<RelationBase<Tnew>>(newbody);
    //}

    //// generate a new relation that is a projection
    //public static RelationBase<Tnew> Project<Ttup, Tnew>(this RelationBase<Ttup> relation)
    //where Ttup : TupleBase, new()
    //where Tnew : TupleBase, new() {

    //  var newbody = RelOps.Project<Ttup, Tnew>(relation.Body);
    //  return RelationBase<Tnew>.Create<RelationBase<Tnew>>(newbody);
    //}

    //// generate a new relation that is a set union
    //public static RelationBase<Ttup> Union<Ttup, Tnew>(this RelationBase<Ttup> relation1, RelationBase<Ttup> relation2)
    //where Ttup : TupleBase, new() {

    //  var newbody = RelOps.Minus<Ttup>(relation1.Body, relation2.Body);
    //  return RelationBase<Ttup>.Create<RelationBase<Ttup>>(newbody);
    //}

    //// generate a new relation that is a set minus
    //public static RelationBase<Ttup> Minus<Ttup, Tnew>(this RelationBase<Ttup> relation1, RelationBase<Ttup> relation2)
    //where Ttup : TupleBase, new() {

    //  var newbody = RelOps.Minus<Ttup>(relation1.Body, relation2.Body);
    //  return RelationBase<Ttup>.Create<RelationBase<Ttup>>(newbody);
    //}

    //// generate a new relation that is a set intersection
    //public static RelationBase<Ttup> Intersect<Ttup, Tnew>(this RelationBase<Ttup> relation1, RelationBase<Ttup> relation2)
    //where Ttup : TupleBase, new() {

    //  var newbody = RelOps.Intersect<Ttup>(relation1.Body, relation2.Body);
    //  return RelationBase<Ttup>.Create<RelationBase<Ttup>>(newbody);
    //}

    //// generate a new relation that is a natural join (or semijoin)
    //public static RelationBase<T> Join<T1,T2,T>(this RelationBase<T1> relation1, RelationBase<T2> relation2)
    //where T : TupleBase, new()
    //where T1 : TupleBase, new()
    //where T2 : TupleBase, new() {

    //  var newbody = RelOps.Join<T, T1, T2>(relation1.Body, relation2.Body);
    //  return RelationBase<T>.Create<RelationBase<T>>(newbody);
    //}

    //// generate a new relation that is a natural antijoin 
    //public static RelationBase<T> AntiJoin<T1, T2, T>(this RelationBase<T1> relation1, RelationBase<T2> relation2)
    //where T : TupleBase, new()
    //where T1 : TupleBase, new()
    //where T2 : TupleBase, new() {

    //  var newbody = RelOps.AntiJoin<T, T1, T2>(relation1.Body, relation2.Body);
    //  return RelationBase<T>.Create<RelationBase<T>>(newbody);
    //}


  }
}
