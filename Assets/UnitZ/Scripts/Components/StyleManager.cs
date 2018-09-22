//----------------------------------------------
//      UnitZ : FPS Sandbox Starter Kit
//    Copyright © Hardworker studio 2015 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using UnityEngine;
using System.Collections;

public class StyleManager : MonoBehaviour {

	public GUISkin[] Skins;

	void Start () {
	
	}
	public GUISkin GetSkin(int i){
		if (i < 0)
			i = 0;

		if (i < Skins.Length) {
			return Skins [i];
		} else {
			return null;
		}
	}

	void Update () {
	
	}
}
