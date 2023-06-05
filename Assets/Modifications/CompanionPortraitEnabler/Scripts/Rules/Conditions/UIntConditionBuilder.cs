using System;

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Rules.Conditions
{
	public class UIntConditionBuilder : IConditionInstanceBuilder
    {
		int index = 0;
		public UIntConditionBuilder(int valueIndex)
        {
			this.index = valueIndex;
        }
		public IConditionInstance equals(uint value) {
			if (Log.traceEnabled)
            {
				return new TracingConditionUIntEQ(this.index, value);
			}
			return new ConditionUIntEQ(this.index, value);
		}
		public IConditionInstance greaterThan(uint value) {
			if (Log.traceEnabled)
            {
				return new TracingConditionUIntGT(this.index, value);
			}
			return new ConditionUIntGT(this.index, value);
		}
		public IConditionInstance lessThan(uint value) {
			if (Log.traceEnabled)
            {
				return new TracingConditionUIntLT(this.index, value);
			}
			return new ConditionUIntLT(this.index, value);
		}
		public IConditionInstance greaterThanOrEqualTo(uint value) {
			if (Log.traceEnabled)
            {
				return new TracingConditionUIntGTE(this.index, value);
			}
			return new ConditionUIntGTE(this.index, value);
		}
		public IConditionInstance lessThanOrEqualTo(uint value) {
			if (Log.traceEnabled)
            {
				return new TracingConditionUIntLTE(this.index, value);
			}
			return new ConditionUIntLTE(this.index, value);
		}
		public IConditionInstance notEqualTo(uint value) {
			if (Log.traceEnabled)
            {
				return new TracingConditionUIntNEQ(this.index, value);
			}
			return new ConditionUIntNEQ(this.index, value);
		}
		//---------- NOPE...-------------------------------
		public IConditionInstance MatchesAny(params string[] args) { return null; }
		public IConditionInstance MatchesAll(params string[] args) { return null; }
		public IConditionInstance DoesNotMatchAny(params string[] args) { return null; }
		public IConditionInstance DoesNotMatchAll(params string[] args) { return null; }
    }
}