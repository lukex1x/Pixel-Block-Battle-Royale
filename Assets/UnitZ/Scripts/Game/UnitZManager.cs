//----------------------------------------------
//      UnitZ Battleground : Online PVP starter kit
//    Copyright © Hardworker studio 2018 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using UnityEngine;

public class UnitZManager : MonoBehaviour
{   
	public int TargetFrameRate = 60;
    public bool IsMobile = false;

    void Start()
    {
		Application.targetFrameRate = TargetFrameRate;
    }

    void Awake()
    {
        UnitZ.gameNetwork = (GameNetwork)GameObject.FindObjectOfType(typeof(GameNetwork));
        UnitZ.gameManager = (GameManager)GameObject.FindObjectOfType(typeof(GameManager));
        UnitZ.characterManager = (CharacterManager)GameObject.FindObjectOfType(typeof(CharacterManager));
        UnitZ.itemManager = (ItemManager)GameObject.FindObjectOfType(typeof(ItemManager));
        UnitZ.itemCraftManager = (ItemCrafterManager)GameObject.FindObjectOfType(typeof(ItemCrafterManager));
        UnitZ.playerManager = (PlayerManager)GameObject.FindObjectOfType(typeof(PlayerManager));
        UnitZ.playerSave = (PlayerSave)GameObject.FindObjectOfType(typeof(PlayerSave));
        UnitZ.levelManager = (LevelManager)GameObject.FindObjectOfType(typeof(LevelManager));
        UnitZ.aiManager = (AIManager)GameObject.FindObjectOfType(typeof(AIManager));
        UnitZ.Hud = (PlayerHUDCanvas)GameObject.FindObjectOfType(typeof(PlayerHUDCanvas));
        UnitZ.dedicatedManager = (DedicatedManager)GameObject.FindObjectOfType(typeof(DedicatedManager));
        UnitZ.GameKeyVersion = Application.version;
        UnitZ.IsMobile = IsMobile;
    }
}


public static class UnitZ
{
    public static AIManager aiManager;
    public static GameNetwork gameNetwork;
    public static GameManager gameManager;
    public static CharacterManager characterManager;
    public static ItemManager itemManager;
    public static ItemCrafterManager itemCraftManager;
    public static PlayerManager playerManager;
    public static PlayerSave playerSave;
    public static LevelManager levelManager;
    public static PlayerHUDCanvas Hud;
    public static string GameKeyVersion = "";
    public static bool IsOnline = false;
    public static NetworkGameplayManager NetworkGameplay;
    public static DedicatedManager dedicatedManager;
    public static bool IsMobile = false;

}