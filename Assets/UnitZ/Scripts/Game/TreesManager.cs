//----------------------------------------------
//      UnitZ : FPS Sandbox Starter Kit
//    Copyright © Hardworker studio 2015 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

// Cut tree system
// ***** Warning! this class will take effect directly to a Terrain data
// Touching the Trees on terrain is risk, even work fine but still risk.
// Please use it carefully. disable this system if possible.


public class TreesManager : MonoBehaviour
{
	public int TreeHP = 200;
	public int DropNum = 10;
	public float DropArea = 3;
	public Terrain terrain;
	public float BlastArea = 0.5f;
	public GameObject WoodDrop;
	public float ResetTime = 600;
	
	private List<int> RemovedIndexList = new List<int> ();
	private List<Logger> Loggers = new List<Logger> ();
	private TreeInstance[] treesBackup;
	private int ChopIndex;
	private bool hited = false;
	private float timeTemp;

	
	void Start ()
	{
		if (terrain == null)
			terrain = (Terrain)GameObject.FindObjectOfType (typeof(Terrain));

		BackupTree ();
		timeTemp = Time.time;

	}

	public void OnClientStart(){
		if (UnitZ.NetworkGameplay && UnitZ.NetworkGameplay.environment) {
            UnitZ.NetworkGameplay.environment.UpdateTrees(GetRemovedTrees ());
		}
	}

	void Update ()
	{
		
		hited = false;
		if (Time.time >= ResetTime + timeTemp) {
			if (UnitZ.NetworkGameplay && UnitZ.NetworkGameplay.environment.isServer) {
				ResetTrees();
				if (UnitZ.NetworkGameplay.environment)
					UnitZ.NetworkGameplay.environment.UpdateTrees(GetRemovedTrees ());
			} 
			timeTemp = Time.time;
		}
	}

	public void ResetTrees ()
	{
		RemovedIndexList.Clear ();
		Loggers.Clear ();
		if(terrain != null)
		terrain.terrainData.treeInstances = treesBackup;
	}

	bool LoggerChecker (int index)
	{
		// we cannot put HP variable to every trees in the scene
		// so we have to use something to define it by Index of a tree
		// a Logger using for define a HP to each trees
		
		foreach (Logger logger in Loggers) {
			if (logger.index == index)
				return false;
		}
		Logger newLogger = new Logger ();
		newLogger.index = index;
		newLogger.HP = TreeHP;
		Loggers.Add (newLogger);
		return true;
	}

	bool LoggerApplyDamage (int damage, int index)
	{
		// apply damage to a tree
		if (!LoggerChecker (index)) {
			foreach (Logger logger in Loggers) {
				if (logger.index == index) {
					logger.HP -= damage;
					if (logger.HP <= 0) {
						return true;	
					}
				}
			}
		}
		return false;
	}

	public void Cuttree (Vector3 position, int damage)
	{
		if (terrain == null)
			return;
		
		// we cannot specific a tree, we just find possibility around the position 
		int index = 0;
		foreach (TreeInstance tree in terrain.terrainData.treeInstances) {
			Vector3 treepos = Vector3.Scale (tree.position, terrain.terrainData.size) + Terrain.activeTerrain.transform.position;
			var distance = Vector3.Distance (new Vector3 (treepos.x, position.y, treepos.z), position);
			// hit a tree if in ranged
			if (distance < BlastArea) {
				if (LoggerApplyDamage (damage, index)) {
					Drop (position);
					RemovedIndexList.Add (index);
					SendARemovedTree (index);
					return;
				}
			}
			index++;
		}
		
	}

	public void SendARemovedTree (int index)
	{
		if (UnitZ.NetworkGameplay && UnitZ.NetworkGameplay.environment.isServer) {
			RemoveATrees (index);
			// only server can send a removed tree index;
			if (UnitZ.NetworkGameplay.environment)
				UnitZ.NetworkGameplay.environment.UpdateTrees(GetRemovedTrees ());
		}
		
	}

	public string GetRemovedTrees ()
	{
		// convert array to string using for send via RPC network
		string arraylist = "";
		foreach (int index in RemovedIndexList) {
			arraylist += index + ",";
		}
		return arraylist;
	}

	public void UpdateRemovedTrees (string indexremoved)
	{
		// get all indexes of removed trees as a string
		if (terrain == null)
			return;
		
		RemovedIndexList.Clear ();
		
		// convert them to array
		string[] indexes = indexremoved.Split ("," [0]);
		for (int i = 0; i < indexes.Length; i++) {
			int res = -1;
			if (int.TryParse (indexes [i], out res))
				RemovedIndexList.Add (res);
		}
		// save them to a removed index list
		
		List<TreeInstance> instancesTmp = new List<TreeInstance> (terrain.terrainData.treeInstances);
		foreach (int removed in RemovedIndexList) {
			// loop a removed index list find and remove a tree
			instancesTmp.RemoveAt (removed);
		}
		// replace a tree instances in terrain data with new array
		terrain.terrainData.treeInstances = instancesTmp.ToArray ();
	}

	public void RemoveATrees (int index)
	{
		// remove a specific tree by index
		List<TreeInstance> instancesTmp = new List<TreeInstance> ();
		int i = 0;
		foreach (TreeInstance tree in terrain.terrainData.treeInstances) {
			// loop a all of a tree on terrain
			if (i != index) {
				// basically add every tree to a Tmp array, Except a tree with a specfic index
				instancesTmp.Add (tree);
			}
			i++;
		}
		// replace a tree instances in terrain data with a Tmp array
		terrain.terrainData.treeInstances = instancesTmp.ToArray ();
	}

	public void OnHit (DamagePackage dm)
	{
		if (!hited) {
			if (UnitZ.NetworkGameplay && UnitZ.NetworkGameplay.environment.isServer) {
				Cuttree (dm.Position, dm.Damage);
				hited = true;
			}
		}
	}

	public void Drop (Vector3 position)
	{
		// drap a wood
		if (WoodDrop == null)
			return;
		
		for (int i = 0; i < DropNum; i++) {
			UnitZ.gameNetwork.RequestSpawnObject (WoodDrop, DetectGround (position + new Vector3 (Random.Range (-DropArea, DropArea), 0, Random.Range (-DropArea, DropArea))), WoodDrop.transform.rotation);
		}
	}

	Vector3 DetectGround (Vector3 position)
	{
		RaycastHit hit;
		if (Physics.Raycast (position, -Vector3.up, out hit, 1000.0f)) {
			return hit.point;
		}
		return position;
	}

	void BackupTree ()
	{
		if (terrain == null)
			return;
		// create backup trees data/
		treesBackup = terrain.terrainData.treeInstances;
	}

	void OnApplicationQuit ()
	{
		// Importance!! Need to restore all tree before close
		// if not, all the removed trees will be lose forever
		if (terrain == null)
			return;

		ResetTrees();
	}

	
}

public class Logger
{
	public int index;
	public int HP;
}
