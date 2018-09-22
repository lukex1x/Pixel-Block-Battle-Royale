//----------------------------------------------
//      UnitZ : FPS Sandbox Starter Kit
//    Copyright © Hardworker studio 2015 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class TooltipUsing : TooltipInstance
{

    private static TooltipInstance tooltip;
    private bool interactive = false;
    private bool onpress = false;
    private ItemCollector Item;

    void Start()
    {
        tooltip = this;
        interactive = false;
        HideTooltip();
    }

    void LateUpdate()
    {
        tooltip = this;
        // if any mouse is pressed so checking if it pressed on no button.
        // pressed on no button will hide this tooltip.
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            onpress = true;
        }
        else
        {
            if (onpress || MouseLock.MouseLocked)
            {
                if (!interactive)
                {
                    HideTooltip();
                }
                interactive = false;
                onpress = false;
            }
        }
    }

    public void Use()
    {
        // function for use item
        interactive = true;
        if (UnitZ.playerManager != null && UnitZ.playerManager.PlayingCharacter != null && Item != null)
        {
            UnitZ.playerManager.PlayingCharacter.inventory.EquipItemByCollector(Item);
            HideTooltip();
        }

    }

    public void Unequip()
    {
        // function for unequip item
        interactive = true;
        if (UnitZ.playerManager != null && UnitZ.playerManager.PlayingCharacter != null && Item != null)
        {
            UnitZ.playerManager.PlayingCharacter.inventory.RemoveEquipItemByCollector(Item);
        }
        HideTooltip();
    }

    public void Delete()
    {
        // function for remove item
        interactive = true;
        if (UnitZ.playerManager != null && UnitZ.playerManager.PlayingCharacter != null && Item != null)
        {
            UnitZ.playerManager.PlayingCharacter.inventory.DropItemByCollector(Item, Item.Num);
        }
        HideTooltip();
    }

    public override void ShowTooltip(TooltipTrigger trigger, Vector3 pos)
    {
        GUIItemCollector guiItem = trigger.GetComponent<GUIItemCollector>();
        if (guiItem == null)
            return;
        Item = guiItem.Item;

        onpress = false;
        base.ShowTooltip(trigger, pos);
    }

    public override void ShowTooltip(TooltipTrigger trigger, Vector3 pos, string type)
    {
        onpress = false;
        base.ShowTooltip(trigger, pos, type);
    }



    public static TooltipInstance Instance
    {
        get
        {
            if (tooltip == null)
            {
                TooltipsManager toolmanage = (TooltipsManager)GameObject.FindObjectOfType(typeof(TooltipsManager));
                if (toolmanage)
                {
                    for (int i = 0; i < toolmanage.AllToolTips.Length; i++)
                    {
                        if (toolmanage.AllToolTips[i].GetComponent<TooltipUsing>())
                        {
                            tooltip = toolmanage.AllToolTips[i].GetComponent<TooltipUsing>();
                            break;
                        }
                    }
                }
            }
            return tooltip;
        }
    }
}
