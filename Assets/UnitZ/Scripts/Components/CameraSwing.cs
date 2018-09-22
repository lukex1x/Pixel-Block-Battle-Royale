//----------------------------------------------
//      UnitZ : FPS Sandbox Starter Kit
//    Copyright © Hardworker studio 2015 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using UnityEngine;
using System.Collections;

public class CameraSwing : MonoBehaviour {

	public Vector3 Speed = new Vector3(0,0.01f,0);
	
	void Start () {
        
	}
	

	void Update () {
		this.transform.Rotate(Speed * Mathf.Sin(Time.time)); 
	}
}
