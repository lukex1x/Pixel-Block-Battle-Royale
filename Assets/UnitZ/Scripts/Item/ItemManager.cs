//----------------------------------------------
//      UnitZ : FPS Sandbox Starter Kit
//    Copyright © Hardworker studio 2015 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;

public class ItemManager : MonoBehaviour
{
    public ItemData[] ItemsList;
    public Dictionary<int, FPSItemEquipment> ItemEquipments = new Dictionary<int, FPSItemEquipment>();
    public string Suffix = "UZ";
    public int itemCount = 0;

    void Start()
    {
        for (int i = 0; i < ItemsList.Length; i++)
        {
            UnitZ.gameNetwork.spawnPrefabs.Add(ItemsList[i].gameObject);
            if (ItemsList[i].ItemFPS)
            {
                FPSItemPlacing fpsItemPlacer = ItemsList[i].ItemFPS.GetComponent<FPSItemPlacing>();
                if (fpsItemPlacer)
                {
                    if (fpsItemPlacer.Item != null)
                    {
                        NetworkIdentity objPlace = fpsItemPlacer.Item.GetComponent<NetworkIdentity>();
                        if (objPlace)
                        {
                            UnitZ.gameNetwork.spawnPrefabs.Add(fpsItemPlacer.Item.gameObject);
                        }
                    }
                }

                FPSItemThrow fpsItemThrow = ItemsList[i].ItemFPS.GetComponent<FPSItemThrow>();
                if (fpsItemThrow)
                {
                    if (fpsItemThrow.Item != null)
                    {
                        NetworkIdentity objPlace = fpsItemThrow.Item.GetComponent<NetworkIdentity>();
                        if (objPlace)
                        {
                            UnitZ.gameNetwork.spawnPrefabs.Add(fpsItemThrow.Item.gameObject);
                        }
                    }
                }
            }
        }
    }

    void Awake()
    {
        ItemEquipments = new Dictionary<int, FPSItemEquipment>(ItemsList.Length);
        for (int i = 0; i < ItemsList.Length; i++)
        {
            ItemsList[i].ItemID = Suffix + i;
            ItemsList[i].ItemIndex = i;
            // Set ItemID to every items.
            if (ItemsList[i].ItemFPS)
            {
                FPSItemEquipment fpsItemEquipment = ItemsList[i].ItemFPS.GetComponent<FPSItemEquipment>();
                if (fpsItemEquipment)
                {
                    fpsItemEquipment.ItemID = ItemsList[i].ItemID;
                    fpsItemEquipment.ItemIndex = i;
                    ItemEquipments[i] = fpsItemEquipment;
                }
                FPSWeaponEquipment weapon = ItemsList[i].ItemFPS.GetComponent<FPSWeaponEquipment>();
                if (weapon)
                {
                    if (ItemsList[i].ItemEquip)
                    {
                        ItemsList[i].ItemEquip.UsingType = weapon.UsingType;
                    }
                }

                FPSItemPlacing fpsItemPlacer = ItemsList[i].ItemFPS.GetComponent<FPSItemPlacing>();
                if (fpsItemPlacer)
                {
                    if (fpsItemPlacer.Item != null)
                    {
                        ObjectSpawn objSpawn = fpsItemPlacer.Item.GetComponent<ObjectSpawn>();
                        if (objSpawn)
                        {
                            objSpawn.ItemID = ItemsList[i].ItemID;
                            if (objSpawn.Item)
                            {
                                ObjectPlacing objPlace = objSpawn.Item.GetComponent<ObjectPlacing>();
                                if (objPlace)
                                {
                                    objPlace.ItemID = ItemsList[i].ItemID;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public FPSItemEquipment GetFPSitem(int index)
    {
        return ItemEquipments[index];
    }

    public ItemData GetItem(int index)
    {
        if (index < ItemsList.Length && index >= 0)
        {
            return ItemsList[index];
        }
        else
        {
            return null;
        }
    }

    public int GetIndexByID(string itemid)
    {
        for (int i = 0; i < ItemsList.Length; i++)
        {
            if (itemid == ItemsList[i].ItemID)
            {
                return i;
            }
        }
        return -1;
    }

    public int GetIndexByName(string itemname)
    {
        for (int i = 0; i < ItemsList.Length; i++)
        {
            if (itemname == ItemsList[i].ItemName)
            {
                return i;
            }
        }
        return -1;
    }

    public ItemData CloneItemData(ItemData item)
    {
        for (int i = 0; i < ItemsList.Length; i++)
        {
            if (item.ItemID == ItemsList[i].ItemID)
            {
                return ItemsList[i];
            }
        }
        return null;
    }

    public int GetItemIndexByItemData(ItemData item)
    {
        for (int i = 0; i < ItemsList.Length; i++)
        {
            if (item.ItemID == ItemsList[i].ItemID)
            {
                return i;
            }
        }
        return -1;
    }

    public ItemData CloneItemDataByIndex(string itemID)
    {
        for (int i = 0; i < ItemsList.Length; i++)
        {
            if (ItemsList[i].ItemID == itemID)
            {
                return ItemsList[i];
            }
        }
        return null;
    }

    public ItemData GetItemDataByID(string itemid)
    {
        for (int i = 0; i < ItemsList.Length; i++)
        {
            if (itemid == ItemsList[i].ItemID)
            {
                return ItemsList[i];
            }
        }
        return null;
    }

    public ItemData GetItemDataByName(string itemname)
    {
        for (int i = 0; i < ItemsList.Length; i++)
        {
            if (itemname == ItemsList[i].ItemName)
            {
                return ItemsList[i];
            }
        }
        return null;
    }

    public void PlacingObject(string itemid, Vector3 position, Vector3 normal)
    {
        ItemData itemdata = GetItemDataByID(itemid);
        if (itemdata.ItemFPS)
        {
            FPSItemPlacing fpsplacing = itemdata.ItemFPS.GetComponent<FPSItemPlacing>();
            if (fpsplacing)
            {
                if (fpsplacing.Item)
                {

                    GameObject obj = UnitZ.gameNetwork.RequestSpawnObject(fpsplacing.Item, position, Quaternion.identity);
                    if (obj)
                    {
                        ObjectPlacing objplaced = obj.GetComponent<ObjectPlacing>();
                        objplaced.transform.forward = normal;
                        objplaced.SetItemID(itemid);

                    }
                }
            }
        }
    }

    public void DirectPlacingObject(string itemid, string itemuid, Vector3 position, Quaternion rotation)
    {
        //Debug.Log("Direct place "+itemid);
        ItemData itemdata = GetItemDataByID(itemid);
        if (itemdata.ItemFPS)
        {
            FPSItemPlacing fpsplacing = itemdata.ItemFPS.GetComponent<FPSItemPlacing>();
            if (fpsplacing)
            {
                if (fpsplacing.Item)
                {

                    GameObject obj = UnitZ.gameNetwork.RequestSpawnObject(fpsplacing.Item, position, rotation);
                    if (obj)
                    {
                        ObjectPlacing objplaced = obj.GetComponent<ObjectPlacing>();
                        objplaced.SetItemID(itemid);
                        objplaced.SetItemUID(itemuid);
                    }
                }
            }
        }
    }
}

