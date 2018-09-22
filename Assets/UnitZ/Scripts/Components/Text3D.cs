using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Text3D : MonoBehaviour {

    public TextMesh text;
    public float Distance = 3000;

	void Awake () {
        text = this.GetComponent<TextMesh>();
    }

    public void SetText(string texts)
    {
        if (text)
            text.text = texts;
    }

	void Update () {
        if (Camera.current != null)
        {
            if (Vector3.Distance(Camera.current.transform.position, this.transform.position) > Distance)
            {
                this.transform.localScale = Vector3.zero;
            }
            else
            {
                this.transform.localScale = Vector3.one;
                Quaternion rota = Quaternion.LookRotation((this.transform.position - Camera.current.transform.position).normalized);
                this.transform.rotation = rota;
            }
        }
    }
}
