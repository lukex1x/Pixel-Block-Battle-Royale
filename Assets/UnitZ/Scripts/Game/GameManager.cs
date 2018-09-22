//----------------------------------------------
//      UnitZ Battleground : Online PVP starter kit
//    Copyright © Hardworker studio 2018 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameView
{
    FPS,TPS,Both
}

public class GameManager : MonoBehaviour
{
	public string UserName = "";
	public string Team = "";
	public string UserID = "";
	public string CharacterKey = "";
    public int PlayerNetID = -1;
    public int BotNumber = 1;
    public int BotMax = 20;
    public GameView GameViewType;

    [HideInInspector]
	public bool IsRefreshing = false;
	[HideInInspector]
	public AsyncOperation loadingScene;
    [HideInInspector]
    public bool IsPlaying = false;
    [HideInInspector]
    public bool IsBattleStart = false;
    [HideInInspector]
    public bool IsAutoSpawn = true;

    void Awake ()
	{
		DontDestroyOnLoad (this.gameObject);
	}

    public void UpdateProfile()
    {
        PlayerPrefs.SetString("user_name", UserName);
    }

	void Start ()
	{
		UserName = PlayerPrefs.GetString ("user_name");
	}

	public void RestartGame ()
	{
		if (UnitZ.playerManager != null)
			UnitZ.playerManager.Reset ();
	}

    public void QuitGame ()
	{
		if (UnitZ.NetworkGameplay != null)
			UnitZ.NetworkGameplay.chatLog.Clear ();
		
		if (UnitZ.playerManager != null)
			UnitZ.playerManager.Reset ();

        UnitZ.gameNetwork.Disconnect ();
        UnitZ.aiManager.Clear();
    }

	public void StartLoadLevel (string level)
	{
		loadingScene = SceneManager.LoadSceneAsync(level);
	}

}
