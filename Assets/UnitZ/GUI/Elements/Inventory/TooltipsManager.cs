using UnityEngine;
using System.Collections;

public class TooltipsManager : MonoBehaviour
{

    public TooltipInstance[] AllToolTips;

    void Start()
    {
        if (AllToolTips.Length <= 0)
            AllToolTips = (TooltipInstance[])GameObject.FindObjectsOfType(typeof(TooltipInstance));
    }

    void Update()
    {
        for (int i = 0; i < AllToolTips.Length; i++)
        {
            if (AllToolTips[i] != null)
            {
                AllToolTips[i].IsOpenChecker();
            }
        }
    }
}
