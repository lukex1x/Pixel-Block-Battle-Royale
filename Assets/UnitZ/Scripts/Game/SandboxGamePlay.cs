using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandboxGamePlay : NetworkGameplayManager {
    public override void Start()
    {
        UnitZ.gameManager.IsBattleStart = true;
        Debug.Log("game set");
        base.Start();
    }
}
