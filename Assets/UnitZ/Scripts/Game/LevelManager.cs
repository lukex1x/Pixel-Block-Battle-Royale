//----------------------------------------------
//      UnitZ : FPS Sandbox Starter Kit
//    Copyright © Hardworker studio 2015 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------
using UnityEngine;
using UnityEngine.SceneManagement;

public enum LevelType
{
    BattleRoyale,
    Sandbox,
	Custom
}

public class LevelManager : MonoBehaviour
{
    public LevelPreset[] LevelPresets;
    public GameObject Preloader;
    private string currentLevel;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    public LevelPreset GetLevel(int index)
    {
        if(index >= 0 && LevelPresets.Length > index)
        {
            return LevelPresets[index];
        }
        return null;
    }

    public LevelPreset GetCurrentLevel()
    {
        for(int i = 0; i < LevelPresets.Length; i++)
        {
            if(LevelPresets[i] != null && LevelPresets[i].SceneName == currentLevel)
            {
                return LevelPresets[i];
            }
        }
        return null;
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        currentLevel = scene.name;
        if (Preloader != null)
            Preloader.SetActive(false);
    }

    public void OnGameIsBegin()
    {
        if (Preloader != null)
            Preloader.SetActive(true);
    }
    public void OnGameplay()
    {
        if (Preloader != null)
            Preloader.SetActive(false);
    }

    bool isSceneCurrentlyLoaded(string sceneName_no_extention)
    {
        for (int i = 0; i < SceneManager.sceneCount; ++i)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name == sceneName_no_extention)
            {
                return true;
            }
        }
        return false;
    }

    private void Update()
    {
        if (UnitZ.gameManager.IsPlaying)
        {
            if (Preloader != null)
                Preloader.SetActive(false);
        }
    }
}