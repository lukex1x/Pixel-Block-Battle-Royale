using UnityEngine;
using UnityEngine.Networking;
[NetworkSettings(channel = 0, sendInterval = 1.0f)]
public class EnvironmentManager : NetworkBehaviour
{

    public DayNightCycle dayNight;
    public TreesManager Trees;
    [SyncVar]
    public float dayTimeSync;
    [SyncVar(hook = "OnTreesChanged")]
    public string TreesCutData;

    public override void OnStartLocalPlayer()
    {
        if (Trees != null)
            Trees.OnClientStart();
        base.OnStartLocalPlayer();
    }

    private void OnTreesChanged(string treecutdata)
    {
        TreesCutData = treecutdata;
        if (Trees != null)
            Trees.UpdateRemovedTrees(treecutdata);
    }

    public void UpdateTrees(string treecutdata)
    {
        if (isServer)
        {
            TreesCutData = treecutdata;
        }
    }

    void Start()
    {
        Trees = (TreesManager)GameObject.FindObjectOfType(typeof(TreesManager));
        if (dayNight == null)
        {
            dayNight = (DayNightCycle)GameObject.FindObjectOfType(typeof(DayNightCycle));
        }
    }

    void Update()
    {
        if (dayNight)
        {
            if (isServer)
                dayTimeSync = dayNight.Timer;

            dayNight.Timer = dayTimeSync;
        }

    }
}
