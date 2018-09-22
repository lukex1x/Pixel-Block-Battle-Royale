//----------------------------------------------
//      UnitZ : FPS Sandbox Starter Kit
//    Copyright © Hardworker studio 2015 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using UnityEngine;
using System.Collections;

public class PlacingArea : MonoBehaviour {

	public bool Snap = false;
	public string[] KeyPair = {""};
	
	// Can place an object only when KeyPair is match
	public bool KeyPairChecker(string[] keys ){
		
		if(keys.Length<=0 && KeyPair.Length<=0)
			return true;
		
		for(int i=0;i<KeyPair.Length;i++){
			for(int k=0;k<keys.Length;k++){
				if(keys[k] == KeyPair[i]){
					return true;	
				}
			}
		}
		return false;	
	}
	
	// set a defult if blank
	void Start () {
		if(KeyPair.Length<=0)
			KeyPair = new string[]{""};
	}

	void Update () {
	
	}
}
