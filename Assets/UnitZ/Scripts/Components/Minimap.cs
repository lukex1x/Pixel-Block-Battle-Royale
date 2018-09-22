using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{

    public bool FixedPlayer;
    public Vector2 WorldRealSize;
    public RectTransform MiniMap;
    public GameObject Player;
    public RectTransform SafeArea;
    public RectTransform DeadArea;
    public RectTransform PlayerIndicator;
    public Text TimeText;
    public float MapScale = 1;


    public void Zoom(float zoom)
    {
        if (MiniMap == null)
            return;

        MapScale += zoom;
        if (MapScale < 1)
        {
            MiniMap.anchoredPosition = Vector2.zero;
            MapScale = 1;
        }

        MiniMap.localScale = Vector2.one * MapScale;
    }

    Vector2 WorldPositionToMap(Vector2 minimap, Vector3 realposition)
    {
        Vector2 res = Vector2.zero;
        float scaleRatioX = resize(minimap.x, WorldRealSize.x);
        float scaleRatioY = resize(minimap.y, WorldRealSize.y);
        res = new Vector2(realposition.x * scaleRatioX, realposition.z * scaleRatioY);
        return res;
    }

    float resize(float minimap, float realsize)
    {
        return minimap / realsize;
    }

    private void OnEnable()
    {
        if (UnitZ.levelManager == null)
            return;

        LevelPreset currentlevel = UnitZ.levelManager.GetCurrentLevel();
        if (currentlevel != null)
        {
            if (MiniMap != null && MiniMap.GetComponent<Image>() != null)
                MiniMap.GetComponent<Image>().sprite = currentlevel.Minimap;
            WorldRealSize = new Vector2(currentlevel.MinimapSize, currentlevel.MinimapSize);
        }
    }


    void Start()
    {

    }

    void Update()
    {
        if (UnitZ.NetworkGameplay != null)
        {
            BattleRoyaleGamePlay gameplay = UnitZ.NetworkGameplay.GetComponent<BattleRoyaleGamePlay>();
            if (gameplay != null)
            {
                if (TimeText != null)
                {
                    float timer = gameplay.GetTimeToNextArea;
                    float minutes = Mathf.Floor(timer / 60);
                    float seconds = Mathf.RoundToInt(timer % 60);
                    TimeText.text = minutes + ":" + seconds;

                }
                if (SafeArea)
                {
                    if (gameplay.safeArea != null)
                    {
                        SafeArea.anchoredPosition = WorldPositionToMap(new Vector2(MiniMap.rect.width, MiniMap.rect.height), gameplay.safeArea.transform.position);
                        Vector2 newsize = new Vector2(resize(MiniMap.rect.width, WorldRealSize.x) * gameplay.safeArea.transform.localScale.x, resize(MiniMap.rect.height, WorldRealSize.y) * gameplay.safeArea.transform.localScale.z);
                        SafeArea.sizeDelta = newsize;
                    }
                    SafeArea.gameObject.SetActive(gameplay.safeArea && gameplay.safeArea.activeSelf);
                }

                if (DeadArea)
                {
                    if (gameplay.lastDeadArea != null)
                    {
                        DeadArea.anchoredPosition = WorldPositionToMap(new Vector2(MiniMap.rect.width, MiniMap.rect.height), gameplay.lastDeadArea.transform.position);
                        Vector2 newsize = new Vector2(resize(MiniMap.rect.width, WorldRealSize.x) * gameplay.lastDeadArea.transform.localScale.x, resize(MiniMap.rect.height, WorldRealSize.y) * gameplay.lastDeadArea.transform.localScale.z);
                        DeadArea.sizeDelta = newsize;
                    }
                    DeadArea.gameObject.SetActive(gameplay.lastDeadArea && gameplay.lastDeadArea.activeSelf);
                }

            }
            else
            {
                if (SafeArea)
                    SafeArea.gameObject.SetActive(false);
                if (DeadArea)
                    DeadArea.gameObject.SetActive(false);
            }

            if (Player == null && UnitZ.playerManager && UnitZ.playerManager.PlayingCharacter)
                Player = UnitZ.playerManager.PlayingCharacter.gameObject;

            if (MiniMap != null && PlayerIndicator != null && Player != null)
            {
                if (!FixedPlayer)
                {
                    Vector2 position = WorldPositionToMap(new Vector2(MiniMap.rect.width, MiniMap.rect.height), Player.transform.position);
                    PlayerIndicator.anchoredPosition = position;
                    Quaternion rota = PlayerIndicator.rotation;
                    rota.eulerAngles = new Vector3(rota.eulerAngles.x, rota.eulerAngles.y, -Player.transform.rotation.eulerAngles.y);
                    PlayerIndicator.rotation = rota;

                }
                else
                {
                    MiniMap.anchoredPosition = WorldPositionToMap(new Vector2(-MiniMap.rect.width, -MiniMap.rect.height), Player.transform.position);
                    PlayerIndicator.anchoredPosition = Vector2.zero;
                    Quaternion rota = PlayerIndicator.rotation;
                    rota.eulerAngles = new Vector3(rota.eulerAngles.x, rota.eulerAngles.y, -Player.transform.rotation.eulerAngles.y);
                    PlayerIndicator.rotation = rota;
                }

            }
        }
    }
}
