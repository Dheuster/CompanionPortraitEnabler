using System;

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Rules.Conditions
{

	//#################################################################################
	// The Basics....
	// ----------------------------------------------------------
	//
	// CONDITION:
	// ----------
	// Compare Property with VALUE
    //
    //     {<PROP> : { eq  : <VALUE> }}  <-- {<PROP> : <VALUE>} is alias/shortcut for this
    //     {<PROP> : { gt  : <VALUE> }}
    //     {<PROP> : { lt  : <VALUE> }}
    //     {<PROP> : { gte : <VALUE> }}
    //     {<PROP> : { lte : <VALUE> }}
    //     {<PROP> : { neq : <VALUE> }}
	//
	// When the Property is in regards to something with a limited list of values (ENUM or SET):
	//
    //     {<PROP> : { any     : [<VALUE>,<VALUE>,...] }}
    //     {<PROP> : { all     : [<VALUE>,<VALUE>,...] }}
    //     {<PROP> : { notany  : [<VALUE>,<VALUE>,...] }}
    //     {<PROP> : { notall  : [<VALUE>,<VALUE>,...] }}
	//
	// Special : DisjuctionGroup
	//
	//     A Condition that contains a list of other conditions. The container evaluates to true if
    //     any of the inner conditions are true. In MONGO, this may appear as:
	//
	//     { $or : [ {CONDITION},{CONDITION},....] }
	//
	// ----------------------------------------------------------
	// Converting to a Conjuction of Disjuctions...
	// ----------------------------------------------------------
	// Suppose we have:
	//
	// Rule: [
    //   {A},
    //   {$or: [{B},{C}]},
	//   {D}
	// ]
	//
	// This will evaluate as:
	//
	//  A AND (B OR C) AND D
	//
	// Q: How can one make a rule for (A AND B) OR (C AND D)?
	//
	// A: By applying the distributive property:
	//
	//        |------------------| 
	//        |------------v     v
	//       (A AND B) OR (C AND D)
	//              |-------^    ^
	//              |------------|
	//
	// The above breaks out into:
	//     
	// (A OR C) AND (A OR D) AND (B OR C) AND (B OR D)
	//
	// Rule: [
    //   {$or : [{A},{C}] },
	//   {$or : [{A},{D}] },
	//   {$or : [{B},{C}] },
	//   {$or : [{B},{D}] }
	// ]
	//
	// While this makes defining rules a bit more complex for
	// the users, it keeps the engine efficient. Engine can 
	// early bail on a rule as soon as the first false condition 
	// is encountered. And when evaluating an DisjunctionGroup, it
	// can early bail/move on as soon as the first True condition is 
	// encountered.
    // 
	// For more information and examples, see:
	//
    //      https://www.creationkit.com/index.php?title=Conditions#Complex_Conditions

	public interface IConditionInstance {
	    bool evaluate(RuleContext rc);
	}
}