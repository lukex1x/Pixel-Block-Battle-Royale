//----------------------------------------------
//      UnitZ Battleground : Online PVP starter kit
//    Copyright © Hardworker studio 2018 
// by Rachan Neamprasert www.hardworkerstudio.com


using UnityEngine.Networking;
using UnityEngine;

public class CharacterSkyDriving : NetworkBehaviour
{
    // parachute graphic
    public GameObject Parachute;
    private CharacterSystem character;
    private PlayerView view;
    public float UpForce = 10;
    public float ViewDistance = 10;
    private bool isJumpped = false;
    private bool isGrounded = false;
    public bool IsGrounded;
    [SyncVar]
    public bool Released;
    private float timeTmp;
    public float TimeReleaseParachute = 5;
    private float timeReleaseTmp = 0;
    private bool parachuteActive = false;

    private void Awake()
    {
        view = this.GetComponent<PlayerView>();
        character = this.GetComponent<CharacterSystem>();
    }

    void Start()
    {
        isJumpped = false;
        isGrounded = false;
    }

    public void OnOutOfVehicle(bool needparachute)
    {
        // on exit vehicle if parachute needed, command to jump off the plane
        if (needparachute)
            CmdJumpOutThePlane();
    }

    public void ResetParachute()
    {
        CmdResetParachute();
        parachuteActive = false;
        isJumpped = false;
        isGrounded = false;
        if (Parachute)
            Parachute.SetActive(false);
    }

    [Command(channel = 0)]
    private void CmdResetParachute()
    {
        Released = false;
        if (Parachute)
            Parachute.SetActive(false);
    }

    [Command(channel = 0)]
    private void CmdJumpOutThePlane()
    {
        if (!isJumpped)
        {
            character.ChangeState(3);
            RpcJumpOutThePlane();
            timeTmp = Time.time;
            isJumpped = true;
            isGrounded = false;
            timeReleaseTmp = Time.time;
        }
    }

    [ClientRpc(channel = 0)]
    void RpcJumpOutThePlane()
    {
        isJumpped = true;
        isGrounded = false;
        timeReleaseTmp = Time.time;
        parachuteActive = true;
        if (view != null)
        {
            view.View = PlayerViewType.FreeView;
            view.OrbitDistance = ViewDistance;
        }
    }

    public void ReleaseParachute()
    {
        if (!Released && isJumpped && !isGrounded)
        {
            // if not ground or never released, command to released a parachute
            CmdReleaseParachute();
            Released = true;
        }
    }

    [Command(channel = 0)]
    private void CmdReleaseParachute()
    {
        // change character stand to first stand
        Released = true;
        character.ChangeState(4);
    }

    [Command(channel = 0)]
    private void CmdLanded()
    {
        isGrounded = true;
        character.ChangeState(0);
        RpcLanded();
    }

    [ClientRpc(channel = 0)]
    void RpcLanded()
    {
        isGrounded = true;
        Released = false;
        // if landed change view to FPS view
        if (view.View == PlayerViewType.FreeView)
        {
            view.View = PlayerViewType.FirstVeiw;
        }
    }

    void Update()
    { 
        // hide parachute graphic if not release
        if (Parachute)
            Parachute.SetActive(Released);

        if (character == null || character.Motor == null || !parachuteActive)
            return;

        // check if grounded
        IsGrounded = character.Motor.grounded;

        if (isServer)
        {
            if (isJumpped && !isGrounded)
            {

                // wait 2 sec before start landing check, to prevent from hit a plane collision instead of ground
                if (Time.time > timeTmp + 2)
                {
                    if (IsGrounded)
                    {
                        // if landed
                        CmdLanded();
                    }
                }
                if (!Released)
                {
                    // parachute can automatically release in time
                    if (Time.time >= timeReleaseTmp + TimeReleaseParachute)
                    {
                        ReleaseParachute();
                    }
                }
            }
        }
        else
        {
            if (isJumpped && !isGrounded)
            {
                view.OrbitDistance = ViewDistance; 
            }
        }
        if (Released && isLocalPlayer)
        {
            this.transform.position += Vector3.up * Time.deltaTime * UpForce;
        }
    }
}
