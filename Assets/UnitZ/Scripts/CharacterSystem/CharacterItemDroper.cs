//----------------------------------------------
//      UnitZ Battleground : Online PVP starter kit
//    Copyright © Hardworker studio 2018 
// by Rachan Neamprasert www.hardworkerstudio.com

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[RequireComponent (typeof(CharacterSystem))]
public class CharacterItemDroper : NetworkBehaviour
{
    public bool DropSplit = true;
	public GameObject Backpack;
	CharacterSystem character;

	void Start ()
	{
		character = this.GetComponent<CharacterSystem> ();
	}

	[Command(channel=0)]
	void CmdDropItem (string itemdata)
	{
        if (DropSplit)
        {
            // spawn all item to ground
            ItemManager item = (ItemManager)GameObject.FindObjectOfType(typeof(ItemManager));
            string[] data = itemdata.Split("|"[0]);
            if (data.Length >= 4)
            {
                string[] indexList = data[0].Split(","[0]);
                string[] numList = data[1].Split(","[0]);
                string[] numtagList = data[2].Split(","[0]);
                string[] shortcutList = data[3].Split(","[0]);
                for (int i = 0; i < indexList.Length; i++)
                {
                    if (indexList[i] != "")
                    {
                        ItemCollector itemCol = new ItemCollector();
                        itemCol.Item = item.GetItemDataByID(indexList[i]);
                        if (itemCol.Item != null)
                        {
                            int.TryParse(numList[i], out itemCol.Num);
                            int.TryParse(numtagList[i], out itemCol.NumTag);
                            int.TryParse(shortcutList[i], out itemCol.Shortcut);

                            itemCol.Active = true;
                            UnitZ.gameNetwork.RequestSpawnItem(itemCol.Item.gameObject, itemCol.NumTag, itemCol.Num, this.transform.position, Quaternion.identity);
                        }
                    }
                }
            }
        }
        else
        {
            // spawn backpack included with all item
            if (Backpack)
            {
                UnitZ.gameNetwork.RequestSpawnBackpack(Backpack.gameObject, itemdata, this.transform.position, Quaternion.identity);
            }
        }
	}

	public void DropItem ()
	{
		if (isLocalPlayer) {
			if (character != null && character.inventory != null) {
				CmdDropItem (character.inventory.GetItemDataText ());
			}
		}
	}
}
