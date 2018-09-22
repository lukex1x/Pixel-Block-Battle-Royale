//----------------------------------------------
//      UnitZ : FPS Sandbox Starter Kit
//    Copyright © Hardworker studio 2015 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,IPointerClickHandler
{
	// add this script to any object with GUIItemCollector component attached.
	
	public ItemCollector Item;
	public string Type = "inventory";
	private Vector3 pointerPosition;

	public void Start ()
	{
		
	}

	public void OnPointerClick (PointerEventData eventData)
	{
		pointerPosition = new Vector3 (eventData.position.x, eventData.position.y - 18f, 0f);
		Select (pointerPosition);
        if (TooltipDetails.Instance)
            TooltipDetails.Instance.StopHover();
    }

	public void OnPointerEnter (PointerEventData eventData)
	{
        // show TooltipDetails when mouse is over it
        if (TooltipDetails.Instance) {
			StartCoroutine (TooltipDetails.Instance.OnHover (eventData, this));
		}
	}

	public void OnPointerExit (PointerEventData eventData)
	{
        if (TooltipDetails.Instance)
            TooltipDetails.Instance.StopHover();
	}

	void Select (Vector3 position)
	{
		if (Item != null) {
			if (TooltipUsing.Instance){
				TooltipUsing.Instance.ShowTooltip (this, position, Type);
			}
		}
	}

}
