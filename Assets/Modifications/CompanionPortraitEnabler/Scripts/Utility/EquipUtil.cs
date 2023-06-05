using System; // String
using System.Collections.Generic; // Needed for List, Set, Dictionary, etc...

using Kingmaker.EntitySystem.Entities;          // UnitEntityData
using Kingmaker.Blueprints.Items;               // BlueprintItem
using Kingmaker.Items;                          // ItemEntity
using Kingmaker.Items.Slots;                    // ItemSlot

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Utility
{
    public static class EquipUtil
    {

		public static bool equipOnNPC(UnitEntityData npc, BlueprintItem itemBlueprint) {
			ItemEntity item = itemBlueprint.CreateEntity();
			ItemSlot itemSlot = null;
			if (null != item.HoldingSlot) {
				itemSlot = item.HoldingSlot;
            } else {
				foreach (ItemSlot slot in npc.Body.EquipmentSlots) {
					if (slot.CanInsertItem(item)) {
						itemSlot = slot;
						break;
					}
				}
            }
			if (null != itemSlot) {
				try { 
					npc.Body.TryInsertItem(itemBlueprint,itemSlot);
				} catch (Exception e)
                {
					Log.trace($"TRACE: Exception throw: {e}");
					return false;
                }
				return (itemSlot.MaybeItem != null);
			}
			return false;
		}
    }
}
