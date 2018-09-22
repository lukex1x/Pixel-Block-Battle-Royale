#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class CentreOfBattle : MonoBehaviour {

    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
        Gizmos.DrawWireCube(transform.position, this.transform.localScale);
        Handles.Label(transform.position, "Centre Of Battle");
#endif
    }

    void Start () {
		
	}
}
