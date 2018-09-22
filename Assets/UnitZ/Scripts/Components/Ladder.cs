//----------------------------------------------
//      UnitZ : FPS Sandbox Starter Kit
//    Copyright © Hardworker studio 2015 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using UnityEngine;
using System.Collections;

public class Ladder : MonoBehaviour {

	void Start () {
	
	}

	void OnTriggerStay(Collider player) {
		
		FPSController fpsController = player.GetComponent<FPSController>();
		
        if(fpsController){
			fpsController.Climb(fpsController.inputDirection.z);
		}
		
    }
		
		
}
