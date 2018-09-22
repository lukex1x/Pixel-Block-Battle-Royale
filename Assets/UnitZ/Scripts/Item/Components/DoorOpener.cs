using UnityEngine;
using System.Collections;


public class DoorOpener : ObjectTrigger
{
	public DoorFrame Door;
	public override void Pickup (GameObject character)
	{
		if(Door){
			Door.Access(character);	
		}
		base.Pickup (character);
	}

}
