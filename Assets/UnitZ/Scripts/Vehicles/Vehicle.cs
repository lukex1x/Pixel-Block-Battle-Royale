//----------------------------------------------
//      UnitZ : FPS Sandbox Starter Kit
//    Copyright © Hardworker studio 2015 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(CarControl))]
public class Vehicle : DamageManager
{
    public bool NeedParachute = false;
    public Seat[] Seats;
    public string VehicleName;
    [SyncVar]
    public string VehicleID;
    [HideInInspector]
    public bool incontrol;
    [SyncVar(hook = "OnSeatDataChanged")]
    public string SeatsData;
    [SyncVar]
    private Vector3 positionSync;
    [SyncVar]
    private Quaternion rotationSync;
    public bool hasDriver;

    public override void Awake()
    {
        if (Seats.Length <= 0)
        {
            Seats = (Seat[])this.GetComponentsInChildren(typeof(Seat));
        }
        base.Awake();
    }

    public override void OnDestroyed()
    {
        for (int i = 0; i < Seats.Length; i++)
        {
            Seats[i].CleanSeat();
        }
        base.OnDestroyed();
    }

    public override void OnStartClient()
    {
        if (isServer)
        {
            VehicleID = netId.ToString();
        }
        OnSeatDataChanged(SeatsData);
        base.OnStartClient();
    }

    void OnSeatDataChanged(string seatsdata)
    {
        SeatsData = seatsdata;
        string[] passengerData = seatsdata.Split(","[0]);
        if (passengerData.Length >= Seats.Length)
        {
            for (int i = 0; i < Seats.Length; i++)
            {
                Seats[i].PassengerID = passengerData[i];
            }
        }
    }

    void GenSeatsData()
    {
        string seatdata = "";
        for (int i = 0; i < Seats.Length; i++)
        {
            if (Seats[i].PassengerID != "none")
            {
                seatdata += Seats[i].PassengerID + ",";
            }
            else
            {
                seatdata += "none,";
            }
        }
        SeatsData = seatdata;
    }

    public void EjectAllSeat()
    {
        for (int i = 0; i < Seats.Length; i++)
        {
            if (Seats[i].passenger != null)
            {
                Seats[i].passenger.OutVehicle();
            }
        }
    }

    private GameObject findPlayerByID(string userID)
    {
        TargetCollector allplayer = UnitZ.aiManager.FindTargetTag("Player");
        GameObject[] players = allplayer.Targets;
        for (int i=0;i< players.Length; i++)
        {
            if(players[i] != null)
            {
                CharacterSystem character = players[i].GetComponent<CharacterSystem>();
                if(character != null)
                {
                    if(character.UserID == userID)
                    {
                        return players[i];
                    }
                }
            }
        }
        return null;
    }

    void UpdatePassengerOnSeats()
    {
        hasDriver = false;
        for (int i = 0; i < Seats.Length; i++)
        {
            if (Seats[i].PassengerID != "none")
            {
                // Searching for a player with passenger id to snap them into a vehicle.
                GameObject obj = findPlayerByID(Seats[i].PassengerID);
                if (obj)
                {
                    CharacterDriver driver = obj.GetComponent<CharacterDriver>();

                    if (driver)
                    {
                        driver.NeedParachute = NeedParachute;

                        FPSController fps = driver.GetComponent<FPSController>();
                        if (fps)
                        {
                            fps.FixedRotation();
                        }

                        if (Seats[i].ForceView != PlayerViewType.None)
                        {
                            PlayerView view = driver.GetComponent<PlayerView>();
                            if (view)
                            {
                                view.View = Seats[i].ForceView;
                                view.OrbitDistance = Seats[i].ViewDistance;
                            }
                        }
                        if (Seats[i].FixRotation)
                        {
                            driver.transform.rotation = Seats[i].transform.rotation;
                        }
                        driver.character.Motor.grounded = false;
                        driver.transform.position = Seats[i].transform.position;
                        driver.transform.parent = Seats[i].transform;
                        driver.CurrentVehicle = this;
                        driver.character.controller.enabled = false;
                        driver.DrivingSeat = Seats[i];
                        hasDriver = true;
                        if (driver.character.IsAlive == false)
                        {
                            Seats[i].PassengerID = "none";
                        }
                    }
                }
            }
            else
            {
                Seats[i].CleanSeat();
            }
        }

        if (isServer)
        {
            GenSeatsData();
        }
    }

    public void GetOutTheVehicle(CharacterDriver driver)
    {
        for (int i = 0; i < Seats.Length; i++)
        {
            if (Seats[i].PassengerID == driver.character.UserID)
            {
                Seats[i].PassengerID = "none";
                return;
            }
        }
    }

    public virtual void Pickup(GameObject character)
    {
        character.SendMessage("PickupCarCallback", this);
    }

    public void GetInTheVehicle(CharacterDriver driver, int seatID)
    {
        if (driver && seatID != -1 && seatID >= 0 && seatID < Seats.Length)
        {
            driver.CurrentVehicle = this;
            Seats[seatID].PassengerID = driver.character.UserID;
            Seats[seatID].passenger = driver;
        }
    }

    public int FindOpenSeatID()
    {
        for (int i = 0; i < Seats.Length; i++)
        {
            if (Seats[i].PassengerID == "none")
            {
                return i;
            }
        }
        return -1;
    }

    public virtual void Drive(Vector2 input, bool brake)
    {

    }

    public override void Update()
    {
        UpdateFunction();
        UpdateDriver();
        base.Update();
    }

    public void UpdateFunction()
    {
        UpdatePassengerOnSeats();

        if (isServer)
        {
            positionSync = this.transform.position;
            rotationSync = this.transform.rotation;
        }

        this.transform.position = Vector3.Lerp(this.transform.position, positionSync, 0.5f);
        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, rotationSync, 0.5f);
    }


    public void UpdateDriver()
    {
        for (int i = 0; i < Seats.Length; i++)
        {
            if (Seats[i].IsDriver && Seats[i].passenger != null)
            {
                return;
            }
        }
        incontrol = false;
    }

    public void GetInfo()
    {
        string info = "Get in\n" + VehicleName;
        UnitZ.Hud.ShowInfo(info, this.transform.position);
    }

}
