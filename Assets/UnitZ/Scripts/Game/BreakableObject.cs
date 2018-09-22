using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BreakableObject : DamageManager
{
    public DropInstance[] Drops;

    public override void OnThisThingDead()
    {
        if (isServer)
        {
            DropInstance drop = Drops[Random.Range(0, Drops.Length)];
            UnitZ.gameNetwork.RequestSpawnItem(drop.Item.gameObject, -1, drop.Number, this.transform.position, Quaternion.identity);
        }
        base.OnThisThingDead();
    }
}

[System.Serializable]
public struct DropInstance
{
    public ItemData Item;
    public int Number;
}