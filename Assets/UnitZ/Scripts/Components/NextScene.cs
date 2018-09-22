//----------------------------------------------
//      UnitZ : FPS Sandbox Starter Kit
//    Copyright © Hardworker studio 2015 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using UnityEngine;
using System.Collections;

public class NextScene : MonoBehaviour {

	public string SceneName = "mainmenu";
	
	void Start () {
		UnityEngine.SceneManagement.SceneManager.LoadScene(SceneName);
	}
	
}
