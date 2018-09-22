using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Compass : MonoBehaviour {

    public RectTransform CompassImage;

	void Start () {
		
	}
	

	void Update () {
		if(UnitZ.playerManager.PlayingCharacter != null)
        {
           Vector3 angle = UnitZ.playerManager.PlayingCharacter.transform.rotation.eulerAngles;
            if(CompassImage != null)
            {
                CompassImage.anchoredPosition = new Vector2((CompassImage.sizeDelta.x / 360) * angle.y, CompassImage.anchoredPosition.y);
            }
        }
	}
}
