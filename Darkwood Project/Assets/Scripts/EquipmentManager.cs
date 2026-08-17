using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    [Header("Hand slots")]
    public ItemData rightHandSlot;
    public ItemData leftHandSlot;

    public bool TryEquip(ItemData item, bool toRightHand)
    {
        if (item == null) return false;

        EquippableModule equipModule = item.GetModule<EquippableModule>();
        if (equipModule == null)
        {
            Debug.Log($"Przedmiot {item.displayName} nie jest do trzymania w rękach!");
            return false;
        }

        if (equipModule.isTwoHanded)
        {
            if (rightHandSlot != null) DropItem(rightHandSlot);
            if (leftHandSlot != null && leftHandSlot != rightHandSlot) DropItem(leftHandSlot);

            rightHandSlot = item;
            leftHandSlot = item;

            Debug.Log($"Założono oburącz: {item.displayName}");
            return true;
        }

        if (toRightHand)
        {
            if (rightHandSlot != null && rightHandSlot.GetModule<EquippableModule>()?.isTwoHanded == true)
            {
                DropItem(rightHandSlot);
                leftHandSlot = null;
            }
            else if (rightHandSlot != null)
            {
                DropItem(rightHandSlot);
            }

            rightHandSlot = item;
            Debug.Log($"Prawa ręka chwyta: {item.displayName}");
        }
        else
        {
            if (leftHandSlot != null && leftHandSlot.GetModule<EquippableModule>()?.isTwoHanded == true)
            {
                DropItem(leftHandSlot);
                rightHandSlot = null;
            }
            else if (leftHandSlot != null)
            {
                DropItem(leftHandSlot);
            }

            leftHandSlot = item;
            Debug.Log($"Lewa ręka chwyta: {item.displayName}");
        }

        return true;
    }

    private void DropItem(ItemData item)
    {
        if (item == null) return;

        Debug.Log($"<color=orange>Wyrzucono na ziemię: {item.displayName}</color>");
    }
}