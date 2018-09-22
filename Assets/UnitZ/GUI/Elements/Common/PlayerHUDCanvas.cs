//----------------------------------------------
//      UnitZ : FPS Sandbox Starter Kit
//    Copyright © Hardworker studio 2015 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHUDCanvas : PanelsManager
{
    public GUIIStockItemLoader SecondItemLoader;
    public RectTransform Canvas;
    public GameObject SystemHUD;
    public ValueBar HPbar, WaterBar;
    public GameObject MiniMap;
    public Minimap Map;
    public Text AmmoText;
    public Image Crosshair, CrosshairHit, CrosshairScope;
    public Text GameTime;
    public Image ImageDrag;
    public KillFeedPopup KillFeed;
    public Text AliveText;
    public Text CountdownText;
    public Text BroadcastText;
    public GUIItemEquippedLoader Equipped;
    public Text Info;
    private bool isShowinfo;
    private float timeInfo;
    public GameObject ProcessPopup;
    public GameObject MobileController;
    public Text DebugInfo;

    public void ResetAllHud()
    {
        if (Equipped)
            Equipped.RestEquipShortcut();
        CloseAllPanels();
    }

    private void Awake()
    {
        UnitZ.Hud = this;
        // make sure every panels are closed.
        if (Pages.Length > 0)
            ClosePanel(Pages[0]);
    }

    public Vector2 GetWorldSpaceUIposition(Vector3 position)
    {
        if (Camera.main == null)
            return Vector3.zero;

        Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(position);
        Vector2 WorldObject_ScreenPosition = new Vector2(
        ((ViewportPosition.x * Canvas.sizeDelta.x) - (Canvas.sizeDelta.x * 0.5f)),
        ((ViewportPosition.y * Canvas.sizeDelta.y) - (Canvas.sizeDelta.y * 0.5f)));
        return WorldObject_ScreenPosition;
    }

    public void ShowInfo(string info, Vector3 position)
    {
        if (Info != null)
        {
            isShowinfo = true;
            Info.gameObject.SetActive(true);
            RectTransform inforec = Info.GetComponent<RectTransform>();
            inforec.anchoredPosition = GetWorldSpaceUIposition(position);
            Info.text = info;
            timeInfo = Time.time;
        }
    }

    private void Update()
    {
        if (UnitZ.playerManager == null || Canvas == null)
            return;

        bool isMouselocked = false;
        if (UnitZ.playerManager.PlayingCharacter == null || (UnitZ.playerManager.PlayingCharacter && !UnitZ.playerManager.PlayingCharacter.IsAlive))
        {
            Canvas.gameObject.SetActive(false);
            if (Crosshair)
                Crosshair.gameObject.SetActive(false);
            if (CrosshairHit)
                CrosshairHit.gameObject.SetActive(false);
            if (CrosshairScope)
                CrosshairScope.gameObject.SetActive(false);
        }
        else
        {
            HumanCharacter player = UnitZ.playerManager.PlayingCharacter.GetComponent<HumanCharacter>();
            if (player != null)
            {
                if (Equipped)
                {
                    Equipped.UpdateEquippedShortcut();
                }

                isMouselocked = true;
                Canvas.gameObject.SetActive(true);

                if (Map)
                {
                    Map.Player = player.gameObject;
                    bool iscrosshairopen = false;
                    if (CrosshairScope)
                        iscrosshairopen = !CrosshairScope.gameObject.activeSelf;

                    if (Map.gameObject.activeSelf && !iscrosshairopen)
                    {
                        Map.gameObject.SetActive(false);
                    }
                }

                if (HPbar)
                {
                    HPbar.Value = player.HP;
                    HPbar.ValueMax = player.HPmax;
                }

                if (WaterBar && player)
                {
                    WaterBar.Value = player.Pill;
                    WaterBar.ValueMax = player.PillMax;
                }

                if (AmmoText != null)
                {
                    if (UnitZ.playerManager.PlayingCharacter.inventory.FPSEquipment != null)
                        AmmoText.text = UnitZ.playerManager.PlayingCharacter.inventory.FPSEquipment.Info;
                }
            }
        }

        for (int i = 0; i < Pages.Length; i++)
        {
            if (!Pages[i].LockMouse && Pages[i].gameObject.activeSelf)
            {
                isMouselocked = false;
            }
        }
        if (MobileController != null && UnitZ.IsMobile)
        {
            MobileController.gameObject.SetActive(isMouselocked);
        }

        if (MiniMap)
        {
            MiniMap.SetActive(isMouselocked);
        }

        MouseLock.MouseLocked = isMouselocked;

        if (Info != null)
        {
            Info.gameObject.SetActive(isShowinfo);
            if (Time.time >= timeInfo + 0.1f)
            {
                isShowinfo = false;
            }
        }
        if (BroadcastText != null)
        {
            if (Time.time < broadcastingTime + broadcastDuration)
            {
                BroadcastText.text = broadcastText;
            }
            else
            {
                BroadcastText.text = "";
            }
        }
    }

    public void OpenSecondInventory(CharacterInventory inventory, string type)
    {
        if (IsPanelOpened("InventoryTrade"))
        {
            ClosePanelByName("InventoryTrade");
        }
        else
        {
            SecondItemLoader.OpenInventory(inventory, type);
            OpenPanelByName("InventoryTrade");
        }
    }

    public void CloseSecondInventory()
    {
        ClosePanelByName("InventoryTrade");
    }

    float broadcastingTime = 0;
    float broadcastDuration = 0;
    string broadcastText = "";
    public void BroadcastingText(string text, float time)
    {
        broadcastDuration = time;
        broadcastingTime = Time.time;
        broadcastText = text;
    }

}
