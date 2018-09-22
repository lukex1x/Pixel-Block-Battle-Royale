//----------------------------------------------
//      UnitZ : FPS Sandbox Starter Kit
//    Copyright © Hardworker studio 2015 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using UnityEngine;
using System.Collections;

public class ProjectileMove : MonoBehaviour
{

    public float Speed = 100;
    public float lifeTime = 1;
    void Start()
    {
        GameObject.Destroy(this.gameObject, lifeTime);
    }

    void Update()
    {
        this.transform.position += this.transform.forward * Speed * Time.deltaTime;
    }
}
