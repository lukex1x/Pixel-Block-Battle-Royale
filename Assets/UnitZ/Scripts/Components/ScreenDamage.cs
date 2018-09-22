using UnityEngine;
using UnityEngine.UI;

public class ScreenDamage : MonoBehaviour
{

    public Image DamageSprite;
    public float FadeSpeed = 15;
    private float alpha;

    void Start()
    {

    }

    public void GetHit(float duration)
    {
        alpha = duration;
    }

    private byte latestHP = 0;
    void Update()
    {
        if (UnitZ.playerManager.PlayingCharacter == null)
            return;

        if (DamageSprite == null)
            return;

        if(UnitZ.playerManager.PlayingCharacter.HP != latestHP)
        {
            if(UnitZ.playerManager.PlayingCharacter.HP < latestHP)
            {
                GetHit(1);
            }
            latestHP = UnitZ.playerManager.PlayingCharacter.HP;
        }

        alpha = Mathf.Lerp(alpha, 0, FadeSpeed * Time.deltaTime);
        DamageSprite.color = new Color(DamageSprite.color.r, DamageSprite.color.g, DamageSprite.color.b, alpha);
    }
}
