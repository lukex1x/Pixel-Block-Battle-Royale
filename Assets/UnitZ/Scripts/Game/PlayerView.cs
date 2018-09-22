//----------------------------------------------
//      UnitZ : FPS Sandbox Starter Kit
//    Copyright © Hardworker studio 2015 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------
using UnityEngine;

public enum PlayerViewType
{
    FirstVeiw, ThirdView, FreeView, None
}
public class PlayerView : MonoBehaviour
{
    public FPSCamera FPSCamera;
    public OrbitCamera FreeCamera;
    public GameObject[] PlayerObjects;
    [System.NonSerialized]
    public OrbitCamera freeCamera;
    public float OrbitDistance = 10;
    public PlayerViewType View;
    private CharacterSystem character;
   
    public void OnPlayerAuthorized()
    {
        if (freeCamera == null)
        {
            freeCamera = GameObject.Instantiate(FreeCamera, this.transform.position, Quaternion.identity);
            freeCamera.Target = this.transform.root.gameObject;
        }
        ViewUpdate();
    }

    void Awake()
    {
        character = this.GetComponent<CharacterSystem>();
        FPSCamera = GetComponentInChildren<FPSCamera>();
    }

    public void SwithView()
    {
        switch (View)
        {
            case PlayerViewType.FirstVeiw:
                View = PlayerViewType.ThirdView;
                break;
            case PlayerViewType.ThirdView:
                View = PlayerViewType.FirstVeiw;
                break;
        }
    }
    public void SwithViewSide()
    {
        FPSCamera.ThirdViewInvert = FPSCamera.ThirdViewInvert * -1;
    }

    void LateUpdate()
    {
        if (freeCamera)
            freeCamera.gameObject.SetActive(View == PlayerViewType.FreeView);

        ViewUpdate();
        FPSCamera.IsThirdView = (View == PlayerViewType.ThirdView);
        if (character.Motor.MotorPreset.Length > character.MovementIndex)
            FPSCamera.transform.localPosition = character.Motor.MotorPreset[character.MovementIndex].FPSCamOffset;
    }

    public void ViewUpdate()
    {
        if (character && character.IsMine)
        {
            // is player view
            if (View == PlayerViewType.FirstVeiw)
            {
                hidePlayerObjects(false);
                if (FPSCamera)
                    FPSCamera.gameObject.SetActive(true);
                if (freeCamera)
                    freeCamera.gameObject.SetActive(false);
            }

            if (View == PlayerViewType.FreeView)
            {
                hidePlayerObjects(true);
                if (FPSCamera)
                    FPSCamera.gameObject.SetActive(false);
                if (freeCamera)
                {
                    freeCamera.gameObject.SetActive(true);
                    freeCamera.distance = OrbitDistance;
                }
            }

            if (View == PlayerViewType.ThirdView)
            {
                if (FPSCamera.zooming && FPSCamera.hideWhenScoping)
                {
                    hidePlayerObjects(false);
                }
                else
                {
                    hidePlayerObjects(true);
                }
            }

            if (UnitZ.gameManager.GameViewType != GameView.Both)
            {
                if (UnitZ.gameManager.GameViewType == GameView.FPS)
                {
                    if (View == PlayerViewType.ThirdView)
                        View = PlayerViewType.FirstVeiw;
                }
                if (UnitZ.gameManager.GameViewType == GameView.TPS)
                {
                    if (View == PlayerViewType.FirstVeiw)
                        View = PlayerViewType.ThirdView;
                }
            }

        }
        if (character && !character.IsMine)
        {
            // other player see just only character object

            if (FPSCamera)
                FPSCamera.gameObject.SetActive(false);
            if (freeCamera)
                freeCamera.gameObject.SetActive(false);

            hidePlayerObjects(true);
        }
    }

    private void hidePlayerObjects(bool hide)
    {
        foreach (GameObject go in PlayerObjects)
        {
            if (go != null)
                go.SetActive(hide);
        }
    }

    public void Hide(Transform trans, bool hide)
    {
        foreach (Transform ob in trans)
        {
            ob.gameObject.SetActive(hide);
        }
        trans.gameObject.SetActive(hide);
    }

}
