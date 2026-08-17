using System;

[Serializable]
public abstract class ItemModule
{

}

[Serializable]
public class StackableModule : ItemModule
{
    public int maxStackSize = 30;
}

[Serializable]
public class WeaponModule : ItemModule
{
    public int baseDamage = 10;
    public float fireRate = 0.5f;
}

[Serializable]
public class EquippableModule : ItemModule
{
    public bool isTwoHanded = false;
}