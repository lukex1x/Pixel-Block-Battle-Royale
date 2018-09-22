//----------------------------------------------
//      UnitZ : FPS Sandbox Starter Kit
//    Copyright © Hardworker studio 2015 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

public class ItemBackpack : ItemData
{
    public List<ItemCollector> Items = new List<ItemCollector>();
    public string SyncItemdata;

    public void SetDropItem(string itemdata)
    {
        SyncItemdata = itemdata;
    }

    public override void Pickup(GameObject character)
    {
        character.SendMessage("PickupItemBackpackCallback", this);
        RemoveItem();
    }

}
