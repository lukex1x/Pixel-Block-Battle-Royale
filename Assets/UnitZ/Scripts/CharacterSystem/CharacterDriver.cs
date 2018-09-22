//----------------------------------------------
//      UnitZ Battleground : Online PVP starter kit
//    Copyright © Hardworker studio 2018 
// by Rachan Neamprasert www.hardworkerstudio.com

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[RequireComponent(typeof(CharacterSystem))]
public class CharacterDriver : NetworkBehaviour
{
    [HideInInspector]
    public Seat DrivingSeat;
    [HideInInspector]
    public CharacterSystem character;
    public Vehicle CurrentVehicle;
    [HideInInspector]
    public bool NeedParachute;


    void Start()
    {
        character = this.GetComponent<CharacterSystem>();
    }

    // Player command a driving input
    public void Drive(Vector2 input, bool brake)
    {
        CmdDrive(input, brake);
    }

    [Command(channel = 1)]
    void CmdDrive(Vector2 input, bool brake)
    {
        if (DrivingSeat && DrivingSeat.IsDriver)
        {
            if (DrivingSeat.VehicleRoot)
                DrivingSeat.VehicleRoot.Drive(new Vector2(input.x, input.y), brake);
        }
    }

    // Player command to exit vehicle
    public void OutVehicle()
    {
        CmdOutVehicle();
    }

    [Command(channel = 0)]
    void CmdOutVehicle()
    {
        if (this.transform.root && this.transform.root.GetComponent<Vehicle>())
        {
            this.transform.root.GetComponent<Vehicle>().GetOutTheVehicle(this);
            RpcOutVehicle();
            this.transform.SendMessageUpwards("OnOutOfVehicle", NeedParachute, SendMessageOptions.DontRequireReceiver);
        }
    }

    [ClientRpc(channel = 0)]
    void RpcOutVehicle()
    {
        NoVehicle();
    }

    public void NoVehicle()
    {
        this.transform.parent = null;
        if (this.character && this.character.controller)
            this.character.controller.enabled = true;
        this.DrivingSeat = null;
        this.CurrentVehicle = null;
    }

    // Player command to pickup a car
    public void PickupCarCallback(Vehicle car)
    {
        CmdRequstToGetCar(car.netId);
    }

    // request for pickup a car to host
    [Command(channel = 0)]
    void CmdRequstToGetCar(NetworkInstanceId carid)
    {
        GameObject obj = ClientScene.FindLocalObject(carid);
        Vehicle car = obj.GetComponent<Vehicle>();
        if (car)
        {
            int openseat = car.FindOpenSeatID();
            car.GetInTheVehicle(this, openseat);
            RpcCarCallback(carid, openseat);
        }
    }

    // received callback from host by car id and opened seat id
    [ClientRpc(channel = 0)]
    void RpcCarCallback(NetworkInstanceId carid, int seatid)
    {
        Debug.Log("Found a vehicle netID(" + carid.ToString() + ") callback : Find open seat ID = " + seatid);
        if (seatid == -1)
            return;

        GameObject obj = ClientScene.FindLocalObject(carid);
        if (obj)
        {
            Vehicle vehicle = obj.GetComponent<Vehicle>();
            vehicle.GetInTheVehicle(this, seatid);
        }
    }


}
