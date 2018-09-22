//----------------------------------------------
//      UnitZ Battleground : Online PVP starter kit
//    Copyright © Hardworker studio 2018 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------
using UnityEngine;

public class AIItemStarter : MonoBehaviour
{
    [HideInInspector]
    public CharacterSystem character;
    [HideInInspector]
    public AICharacterShooterNav ai;
    public ItemSet[] ItemSets;
    public float TimeWait = 60;
    private float timeTmp = 0;
    [HideInInspector]
    public int CurrentSet = 0;

    void Start()
    {
        character = this.GetComponent<CharacterSystem>();
        ai = character.GetComponent<AICharacterShooterNav>();
        timeTmp = Time.time;
        TimeWait = Random.Range(TimeWait / 2, TimeWait);
    }

    void Update()
    {
        // AI will get many more item whan time has passed, this is seem like they are farmed
        if (character)
        {
            if (character.isServer)
            {
                if (ItemSets.Length > CurrentSet)
                {
                    if (Time.time >= timeTmp + TimeWait)
                    {
                        if (getNewSet())
                        {
                            CurrentSet += 1;
                            if (ai)
                                ai.Fighting = true;
                        }
                        timeTmp = Time.time;
                    }
                }
            }
        }
    }

    bool getNewSet()
    {
        if (character != null && character.inventory != null)
        {
            for (int i = 0; i < ItemSets.Length; i++)
            {
                if (i == CurrentSet)
                {
                    TimeWait = ItemSets[i].NextTime + Random.Range(0, ItemSets[i].NextTime / 2);

                    if (ItemSets[i].RandomEquipOne)
                    {
                        // random equip some item
                        character.inventory.AddItemByItemData(ItemSets[i].Items[Random.Range(0, ItemSets[i].Items.Length)], 1, -1, -1);
                        character.inventory.UpdateInventoryToAll();
                    }
                    else
                    {
                        //equip all in the list
                        for (int v = 0; v < ItemSets[i].Items.Length; v++)
                        {
                            character.inventory.AddItemByItemData(ItemSets[i].Items[v], 1, -1, -1);
                        }
                        character.inventory.UpdateInventoryToAll();
                    }
                    return true;
                }
            }
        }
        return false;
    }
}

[System.Serializable]
public class ItemSet
{
    public ItemData[] Items;
    public bool RandomEquipOne = true;
    public float NextTime = 60;
}