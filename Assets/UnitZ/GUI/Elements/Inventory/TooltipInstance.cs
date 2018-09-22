//----------------------------------------------
//      UnitZ : FPS Sandbox Starter Kit
//    Copyright © Hardworker studio 2015 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;


public class TooltipInstance : MonoBehaviour
{
    protected PanelsManager panelsManager;
    protected TooltipTrigger LastTrigger;
    protected RectTransform rect;
    private bool hover = false;
    private Vector2 offset;
    private TooltipTrigger currentTrigger;

    void Awake()
    {
        rect = this.GetComponent<RectTransform>();
        panelsManager = this.GetComponent<PanelsManager>();
    }

    void Start()
    {
        panelsManager = this.GetComponent<PanelsManager>();
        HideTooltip();
    }

    public virtual void StopHover()
    {
        LastTrigger = null;
        hover = false;
        HideTooltip();
    }

    public void IsOpenChecker()
    {
        if (LastTrigger == null || MouseLock.MouseLocked)
        {
            if(hover)
                StopCoroutine("OnHover");

            LastTrigger = null;
            hover = false;
            HideTooltip();
        }
    }

    public virtual IEnumerator OnHover(PointerEventData eventData, TooltipTrigger trigger)
    {
        currentTrigger = trigger;
        if (!hover)
        {
            yield return new WaitForSeconds(0.2f);
            LastTrigger = trigger;
            hover = true;
        }
        // Keep showing until hover is "false"
        while (hover && !MouseLock.MouseLocked && LastTrigger == currentTrigger)
        {
            if (eventData.position.x > Screen.width - rect.sizeDelta.x)
            {
                rect.pivot = new Vector2(1, 1);
            }
            else
            {
                rect.pivot = new Vector2(0, 1);
            }

            ShowTooltip(LastTrigger, new Vector3(eventData.position.x, eventData.position.y - 18f, 0f));
            yield return new WaitForEndOfFrame();
        }
        StopHover();
    }

    public virtual void ShowTooltip(TooltipTrigger trigger, Vector3 pos)
    {
        if (LastTrigger == null || MouseLock.MouseLocked)
            return;
        transform.position = pos;
        gameObject.SetActive(true);
    }

    public virtual void ShowTooltip(TooltipTrigger trigger, Vector3 pos, string type)
    {
        if (trigger == null)
            return;

        if (panelsManager)
        {
            panelsManager.DisableAllPanels();
            panelsManager.OpenPanelByName(type);
        }

        LastTrigger = trigger;
        transform.position = pos;
        gameObject.SetActive(true);
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }
}
