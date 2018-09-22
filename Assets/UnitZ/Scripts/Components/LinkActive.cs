//----------------------------------------------
//      UnitZ : FPS Sandbox Starter Kit
//    Copyright © Hardworker studio 2015 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using UnityEngine;
using System.Collections;

public class LinkActive : ItemData
{

	public string Link = "http://www.hardworkerstudio.com";

	public override void Pickup (GameObject character)
	{
		Application.OpenURL (Link);
		base.Pickup (character);
	}
	
}
