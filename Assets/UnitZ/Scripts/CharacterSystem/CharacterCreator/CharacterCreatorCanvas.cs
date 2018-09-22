//----------------------------------------------
//      UnitZ Battleground : Online PVP starter kit
//    Copyright © Hardworker studio 2018 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CharacterCreatorCanvas : MonoBehaviour
{

    public RectTransform CharacterBadgePrefab;
    public RectTransform Canvas;
    [HideInInspector]
    public CharacterSaveData[] Characters;
    private Vector2 scrollPosition;
    private int indexCharacter = 0;
    public Transform previewSpot;
    public ItemDataPackage[] ItemsPreview;
    private GameObject characterPreviewer;
    [HideInInspector]
    public CharacterSaveData characterLoaded;

    void Start()
    {
        // load and setup all characters
        Setup();
        StartCoroutine(LoadCharacters());
    }
    public void Setup()
    {
        ClearCanvas();
        indexCharacter = PlayerPrefs.GetInt("INDEX_CRE_CHAR");
    }

    void ClearCanvas()
    {
        if (Canvas == null)
            return;

        foreach (Transform child in Canvas.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    public IEnumerator LoadCharacters()
    {
        if (UnitZ.playerSave)
        {
            // load all characters from Save
            Characters = UnitZ.playerSave.LoadAllCharacters();

            while (Characters == null)
            {
                yield return new WaitForEndOfFrame();
            }

            if (Characters.Length > 0)
            {
                // if we have a character
                if (indexCharacter >= Characters.Length)
                    indexCharacter = Characters.Length - 1;

                if (indexCharacter < 0)
                    indexCharacter = 0;

                // spawn Character object for preview
                characterLoaded = Characters[indexCharacter];
                if (UnitZ.characterManager)
                {
                    UnitZ.characterManager.SetupCharacter(characterLoaded);
                    if (UnitZ.characterManager.CharacterSelected != null)
                    {
                        if (characterPreviewer != null)
                            Destroy(characterPreviewer);

                        // disable all unnecessary component
                        characterPreviewer = GameObject.Instantiate(UnitZ.characterManager.CharacterSelected, previewSpot.position, previewSpot.rotation);
                        if (characterPreviewer.GetComponent<CharacterMotor>())
                            characterPreviewer.GetComponent<CharacterMotor>().enabled = false;
                        if (characterPreviewer.GetComponent<HumanCharacter>())
                            characterPreviewer.GetComponent<HumanCharacter>().enabled = false;
                        if (characterPreviewer.GetComponent<CharacterInventory>())
                        {
                            characterPreviewer.GetComponent<CharacterInventory>().StarterItems = ItemsPreview;
                            characterPreviewer.GetComponent<CharacterInventory>().SetupStarterItem();
                            characterPreviewer.GetComponent<CharacterInventory>().enabled = false;
                        }
                        if (characterPreviewer.GetComponent<CharacterAnimation>())
                            characterPreviewer.GetComponent<CharacterAnimation>().enabled = false;
                        if (characterPreviewer.GetComponent<CharacterFootStep>())
                            characterPreviewer.GetComponent<CharacterFootStep>().enabled = false;
                        if (characterPreviewer.GetComponent<CharacterDriver>())
                            characterPreviewer.GetComponent<CharacterDriver>().enabled = false;
                        if (characterPreviewer.GetComponent<FPSController>())
                            characterPreviewer.GetComponent<FPSController>().enabled = false;
                        if (characterPreviewer.GetComponent<CharacterItemDroper>())
                            characterPreviewer.GetComponent<CharacterItemDroper>().enabled = false;
                        characterPreviewer.GetComponent<PlayerView>().FPSCamera.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                // if we don't have any character go to Create character page
                MainMenuManager menu = (MainMenuManager)GameObject.FindObjectOfType(typeof(MainMenuManager));
                if (menu)
                    menu.OpenPanelByName("CreateCharacter");
            }
        }
    }

    public void DrawCharactersToCanvas()
    {
        // draw CharacterBadgePrefab to canvas
        if (Canvas == null || CharacterBadgePrefab == null || Characters == null)
            return;
        ClearCanvas();
        for (int i = 0; i < Characters.Length; i++)
        {
            GameObject obj = (GameObject)GameObject.Instantiate(CharacterBadgePrefab.gameObject, Vector3.zero, Quaternion.identity);
            obj.transform.SetParent(Canvas.transform);
            CharacterBadge charbloger = obj.GetComponent<CharacterBadge>();
            RectTransform rect = obj.GetComponent<RectTransform>();
            if (rect)
            {
                rect.anchoredPosition = new Vector2(5, -(((CharacterBadgePrefab.sizeDelta.y + 5) * i)));
                rect.localScale = CharacterBadgePrefab.gameObject.transform.localScale;
            }
            if (charbloger)
            {
                // just update a GUI elements data
                charbloger.Index = i;
                charbloger.CharacterData = Characters[i];
                if (UnitZ.characterManager)
                {
                    if (UnitZ.characterManager.CharacterPresets.Length > 0 && Characters[i].CharacterIndex < UnitZ.characterManager.CharacterPresets.Length)
                    {
                        charbloger.GUIImage.texture = UnitZ.characterManager.CharacterPresets[Characters[i].CharacterIndex].Icon;
                    }
                }
                charbloger.GUIName.text = Characters[i].PlayerName;
                charbloger.CharacterCreatorS = this;
                charbloger.name = Characters[i].PlayerName;
            }
        }
        Canvas.sizeDelta = new Vector2(Canvas.sizeDelta.x, (CharacterBadgePrefab.sizeDelta.y + 5) * Characters.Length);
    }

    public void CreateCharacter(Text textName)
    {
        // function for create character
        if (UnitZ.characterManager && textName)
        {
            if (UnitZ.characterManager.CreateCharacter(textName.text))
            {
                Setup();
                StartCoroutine(LoadCharacters());
                MainMenuManager menu = (MainMenuManager)GameObject.FindObjectOfType(typeof(MainMenuManager));
                if (menu)
                    menu.OpenPanelByNameNoPreviousSave("Home");
            }
        }
    }

    public void ChangeCharacter(int index)
    {
        if (UnitZ.playerSave)
        {
            ClearCanvas();
            // modify character data and save
            characterLoaded.CharacterIndex = index;
            //characterLoaded.PlayerName = "your new name"; yoi can also change the name too,
            if (UnitZ.playerSave.UpdateCharacter(characterLoaded))
            {
                StartCoroutine(LoadCharacters());
            }
        }
    }

    public void SelectCharacter(CharacterSaveData character)
    {
        // function for select character
        if (UnitZ.characterManager)
            UnitZ.characterManager.SetupCharacter(character);

        MainMenuManager menu = (MainMenuManager)GameObject.FindObjectOfType(typeof(MainMenuManager));
        if (menu)
            menu.OpenPanelByName("EnterWorld");
    }

    public void SetCharacter()
    {
        // function for setup character
        if (UnitZ.characterManager)
            UnitZ.characterManager.SetCharacter();
    }

    public void SelectCreateCharacter(int index)
    {
        // function for select character
        if (UnitZ.characterManager)
            UnitZ.characterManager.SelectCreateCharacter(index);
    }

    public void RemoveCharacter(int index)
    {
        // function for remove character
        if (UnitZ.characterManager)
        {
            UnitZ.characterManager.RemoveCharacter(Characters[index]);
            StartCoroutine(LoadCharacters());
        }
    }


}
