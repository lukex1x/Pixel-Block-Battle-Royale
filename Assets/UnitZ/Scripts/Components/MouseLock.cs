//----------------------------------------------
//      UnitZ : FPS Sandbox Starter Kit
//    Copyright © Hardworker studio 2015 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using UnityEngine;

public static class MouseLock
{
	private static bool mouseLocked;
    public static bool IsMobileControl = false;


    public static bool MouseLocked {
		get {
			return mouseLocked;
		}
		set {
            if (IsMobileControl)
            {
                mouseLocked = false;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                mouseLocked = value;
                Cursor.visible = !value;
                if (mouseLocked)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                }
            }
		}
	}
	

}

