using System;
using System.Collections.Generic;

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
[Serializable]
public class PocketDefinition
{
    public string pocketName = "Kieszeń";
    public int width = 2;
    public int height = 2;
}

[Serializable]
public class ContainerModule : ItemModule
{
    public List<PocketDefinition> pockets = new List<PocketDefinition>();
}