//----------------------------------------------
//      UnitZ : FPS Sandbox Starter Kit
//    Copyright © Hardworker studio 2015 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using UnityEngine;
using System.Collections;

public class CharacterManager : MonoBehaviour
{
	// this is chracter preset index.
	public int CharacterIndex = 0;
	// this is chracter preset.
	public CharacterPreset[] CharacterPresets;
	private CharacterSaveData characterCreate;
	public CharacterSaveData SelectedCharacter;
	public GameObject CharacterSelected;

	void Start ()
	{
		for (int i = 0; i < CharacterPresets.Length; i++) {
			UnitZ.gameNetwork.spawnPrefabs.Add (CharacterPresets [i].CharacterPrefab.gameObject);
		}
	}

	public bool CreateCharacter (string characterName)
	{
		// create new character
		if (UnitZ.gameManager) {
			CreateResult res = new CreateResult ();
			if (characterName != "") {
				// get new Chracter Key
				res = SaveNewCharacter (characterName);
				if (res.IsSuccess) {
					// if passed
					UnitZ.gameManager.UserName = characterName;
					UnitZ.gameManager.CharacterKey = res.CharacterKey;
					return true;
				}
			}
		}
		return false;
	}

	public void SetCharacter ()
	{
		// when you select a character from the list.
		// you will get character preset index.
		CharacterIndex = SelectedCharacter.CharacterIndex;
		
		if (CharacterPresets.Length > 0) {
			// just chack if CharacterIndex is correct.
			if (CharacterIndex >= CharacterPresets.Length)
				CharacterIndex = CharacterPresets.Length - 1;
			
			if (CharacterIndex < 0)
				CharacterIndex = 0;
			
			// Set UserName and CharacterKey
			if (UnitZ.gameManager) {
				UnitZ.gameManager.UserName = SelectedCharacter.PlayerName;
				UnitZ.gameManager.CharacterKey = SelectedCharacter.CharacterKey;
				CharacterSelected = UnitZ.characterManager.CharacterPresets [CharacterIndex].CharacterPrefab.gameObject;
			}
		}
	}

	public void SetupCharacter (CharacterSaveData character)
	{
		// when you select a character from the list.
		// you will get character preset index.
		SelectedCharacter = character;
		CharacterIndex = SelectedCharacter.CharacterIndex;
		
		if (CharacterPresets.Length > 0) {
			// just chack if CharacterIndex is correct.
			if (CharacterIndex >= CharacterPresets.Length)
				CharacterIndex = CharacterPresets.Length - 1;
			
			if (CharacterIndex < 0)
				CharacterIndex = 0;
			
			// Set UserName and CharacterKey
			if (UnitZ.gameManager) {
				UnitZ.gameManager.UserName = SelectedCharacter.PlayerName;
				UnitZ.gameManager.CharacterKey = SelectedCharacter.CharacterKey;
				CharacterSelected = UnitZ.characterManager.CharacterPresets [CharacterIndex].CharacterPrefab.gameObject;
			}
		}
	}

	public void SelectCreateCharacter (int index)
	{
		characterCreate = new CharacterSaveData ();
		characterCreate.CharacterIndex = index;
	}

	public CreateResult SaveNewCharacter (string characterName)
	{
		// create new character and save.
		CreateResult res = new CreateResult ();
		res.IsSuccess = false;
		if (characterName != "" && UnitZ.gameManager && UnitZ.playerSave) {
			characterCreate.PlayerName = characterName;
			res = UnitZ.playerSave.CreateCharacter (characterCreate);
		}
		return res;
	}

	public void RemoveCharacter (CharacterSaveData character)
	{
		if (UnitZ.playerSave) {
			UnitZ.playerSave.RemoveCharacter (character);
		}
	}
}
