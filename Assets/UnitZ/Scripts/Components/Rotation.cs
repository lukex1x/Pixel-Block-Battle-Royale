//----------------------------------------------
//      UnitZ : FPS Sandbox Starter Kit
//    Copyright © Hardworker studio 2015 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using UnityEngine;
using System.Collections;

public class Rotation : MonoBehaviour {

	public Vector3 Axis = Vector3.up;
	void Start () {
	
	}

	void Update () {
		this.transform.Rotate(Axis * Time.deltaTime);
	}
}
