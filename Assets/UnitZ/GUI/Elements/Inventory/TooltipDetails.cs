//----------------------------------------------
//      UnitZ : FPS Sandbox Starter Kit
//    Copyright © Hardworker studio 2015 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using UnityEngine;
using UnityEngine.UI;

public class TooltipDetails : TooltipInstance
{
    // gui elements, need to assign them to these parameter.
    public Text Header;
    public Text Content;
    private static TooltipDetails tooltip;

    void Start()
    {
        tooltip = this;
        HideTooltip();
    }

    public override void ShowTooltip(TooltipTrigger trigger, Vector3 pos)
    {
        GUIItemCollector guiItem = trigger.GetComponent<GUIItemCollector>();
        if (guiItem == null)
            return;

        ItemCollector itemCol = guiItem.Item;
        if (itemCol == null || itemCol.Item == null || MouseLock.MouseLocked)
            return;

        if (Header)
            Header.text = itemCol.Item.ItemName;
        if (Content)
            Content.text = itemCol.Item.Description;

        if (TooltipUsing.Instance.gameObject.activeSelf)
            TooltipUsing.Instance.StopHover();

        base.ShowTooltip(trigger, pos);
    }

    public static TooltipDetails Instance
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
                        if (toolmanage.AllToolTips[i].GetComponent<TooltipDetails>())
                        {
                            tooltip = toolmanage.AllToolTips[i].GetComponent<TooltipDetails>();
                            break;
                        }
                    }
                }
            }
            return tooltip;
        }
    }

}
