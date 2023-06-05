using System;

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Rules.Conditions
{
	public class CondBuildStart
    {
		protected CondBuildStart() { }
		public static CondBuildStart New()
        {
			return new CondBuildStart();
        }
		public IConditionInstanceBuilder IfProp(PROPERTY prop)
        {
			PROPERTYTYPE pt = Meta.GetPropType(prop);
			if (pt == PROPERTYTYPE.USHORT)
            {
				return new UShortConditionBuilder((int)prop);
            }
			if (pt == PROPERTYTYPE.UINT) 
			{
				return new UIntConditionBuilder(((int)prop) >> 14);
			}
			if (pt == PROPERTYTYPE.STRSET) 
			{
				return new SetConditionBuilder(((int)prop )>> 10);
			}
			if ((int)pt > 0 && (int)pt < 13)
			{
				return new EnumConditionBuilder((((int)prop) >> 6), pt);
            }
			return null;
		}
		public IConditionInstanceBuilder IfProp(String propStr)
        {
			if (Meta.StrToPropertyEnum.TryGetValue(propStr.ToUpper(), out PROPERTY prop)) {
				return IfProp(prop);
            }
			return null;
		}
	}
}
