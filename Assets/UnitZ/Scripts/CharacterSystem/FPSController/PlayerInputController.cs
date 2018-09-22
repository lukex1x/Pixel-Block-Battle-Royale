//----------------------------------------------
//      UnitZ Battleground : Online PVP starter kit
//    Copyright © Hardworker studio 2018 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using UnityEngine;

public class PlayerInputController : MonoBehaviour
{
    private FPSController fpsControl;

    private void Start()
    {
        
    }
    void Update()
    {
        if (UnitZ.IsMobile)
            return;
        // *** You can change your controller here 

        if (UnitZ.gameManager.IsPlaying)
        {
            // only in playing mode
            // open in game main menu
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UnitZ.Hud.TogglePanelByName("InGameMenu");
            }
            // open score board
            if (Input.GetKeyDown(KeyCode.N))
            {
                UnitZ.Hud.TogglePanelByName("Scoreboard");
            }
        }

        if (UnitZ.playerManager != null && UnitZ.playerManager.PlayingCharacter != null)
        {
            // get fps controller from current player
            FPSController fpsControl = UnitZ.playerManager.PlayingCharacter.GetComponent<FPSController>();

            if (UnitZ.playerManager.PlayingCharacter.isLocalPlayer && fpsControl != null)
            {
                // move
                fpsControl.MoveCommand(new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")), Input.GetButton("Jump"));

                // change stand sit / prone / stand
                if (Input.GetKeyDown(KeyCode.C))
                {
                    fpsControl.Sit();
                }
                // interact to vihicle
                if (Input.GetKeyDown(KeyCode.F))
                {
                    fpsControl.OutVehicle();
                }
                // sprint
                fpsControl.Sprint(Input.GetKey(KeyCode.LeftShift));
                // aiming control
                if (MouseLock.MouseLocked)
                {
                    fpsControl.Aim(new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")));
                    fpsControl.Trigger1(Input.GetButton("Fire1"));
                    fpsControl.Trigger2(Input.GetButtonDown("Fire2"));
                }
                // interact to thing
                if (Input.GetKeyDown(KeyCode.F))
                {
                    fpsControl.Interactive();
                }
                // change view
                if (Input.GetKeyDown(KeyCode.V))
                {
                    fpsControl.SwithView();
                }
                // change view side
                if (Input.GetKeyDown(KeyCode.B))
                {
                    fpsControl.SwithSideView();
                }
                // reload gun
                if (Input.GetKeyDown(KeyCode.R))
                {
                    fpsControl.Reload();
                }
                // open inventory
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    UnitZ.Hud.TogglePanelByName("Inventory");
                }
                if (Input.GetKeyDown(KeyCode.E))
                {
                    UnitZ.Hud.TogglePanelByName("Craft");
                }
                // open map
                if (Input.GetKeyDown(KeyCode.M))
                {
                    UnitZ.Hud.TogglePanelByName("Map");
                }

                // always check all interactive thing
                fpsControl.Checking();
            }
        }
    }

}
