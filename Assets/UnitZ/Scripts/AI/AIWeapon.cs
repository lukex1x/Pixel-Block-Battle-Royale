//----------------------------------------------
//      UnitZ Battleground : Online PVP starter kit
//    Copyright © Hardworker studio 2018 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using UnityEngine;

public class AIWeapon : MonoBehaviour {

    public FPSWeaponEquipment EquippedWeapon;
    void Start () {
		
	}
    // attached FPS weapon for ai but no visible 
    public void OnAttached(GameObject item)
    {
        EquippedWeapon = item.GetComponent<FPSWeaponEquipment>();
        if (EquippedWeapon != null)
        {
            EquippedWeapon.InfinityAmmo = true;
            EquippedWeapon.IsVisible = false;
            EquippedWeapon.GetComponent<AudioSource>().enabled = false;
        }
    }
}
