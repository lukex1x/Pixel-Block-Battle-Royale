using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SubSpawner : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    // draw gizmose on Editor
    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 2);
        Gizmos.DrawWireCube(transform.position, this.transform.localScale);
        Handles.Label(transform.position, "Sub Spawner");
#endif
    }
}
