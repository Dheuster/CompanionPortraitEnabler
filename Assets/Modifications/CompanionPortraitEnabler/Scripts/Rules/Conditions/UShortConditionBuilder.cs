using System;

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Rules.Conditions
{
	public class UShortConditionBuilder : IConditionInstanceBuilder
    {
		int index = 0;
		public UShortConditionBuilder(int valueIndex)
        {
			this.index = valueIndex;
        }
		public IConditionInstance equals(uint value) {
			if (Log.traceEnabled)
            {
				return new TracingConditionUShortEQ(this.index, (ushort) value);
			}
			return new ConditionUShortEQ(this.index, (ushort) value);
		}
		public IConditionInstance greaterThan(uint value) {
			if (Log.traceEnabled)
            {
				return new TracingConditionUShortGT(this.index, (ushort) value);
            }
			return new ConditionUShortGT(this.index, (ushort) value);
		}
		public IConditionInstance lessThan(uint value) {
			if (Log.traceEnabled)
            {
				return new TracingConditionUShortLT(this.index, (ushort) value);
			}
			return new ConditionUShortLT(this.index, (ushort) value);
		}
		public IConditionInstance greaterThanOrEqualTo(uint value) {
			if (Log.traceEnabled)
            {
				return new TracingConditionUShortGTE(this.index, (ushort) value);
			}
			return new ConditionUShortGTE(this.index, (ushort) value);
		}
		public IConditionInstance lessThanOrEqualTo(uint value) {
			if (Log.traceEnabled)
            {
				return new TracingConditionUShortLTE(this.index, (ushort) value);
			}
			return new ConditionUShortLTE(this.index, (ushort) value);
		}
		public IConditionInstance notEqualTo(uint value) {
			if (Log.traceEnabled)
            {
				return new TracingConditionUShortNEQ(this.index, (ushort) value);
			}
			return new ConditionUShortNEQ(this.index, (ushort) value);
		}
		//---------- NOPE...-------------------------------
		public IConditionInstance MatchesAny(params string[] args) { return null; }
		public IConditionInstance MatchesAll(params string[] args) { return null; }
		public IConditionInstance DoesNotMatchAny(params string[] args) { return null; }
		public IConditionInstance DoesNotMatchAll(params string[] args) { return null; }
    }
}