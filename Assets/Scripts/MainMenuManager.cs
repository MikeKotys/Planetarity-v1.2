using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mechanics;

namespace UI
{
	/// <summary>Handles the logic of the main menu.</summary>
	public class MainMenuManager : MonoBehaviour
	{
		[Tooltip("The GameObject that parents all the elements of the main menu section.")]
		public CanvasGroup MainMenuGroup;
		[Tooltip("The GameObject that parents all the elements of the new game section.")]
		public CanvasGroup NewGameGroup;
		[Tooltip("The GameObject that parents all the elements of the load game section.")]
		public CanvasGroup LoadGameGroup;
		[Tooltip("The text object that displays the chosen number of enemies.")]
		public TextMeshProUGUI NumberOfEnemiesText;
		[Tooltip("The link to the class that handles game loading logic.")]
		public LoadGameMenuManager LoadGameMenuManager;
		[Tooltip("Main game music.")]
		public AudioSource OST;

		void Awake()
		{
			var music = GameObject.Find("Main Menu OST");
			if (music != null && music.GetComponent<AudioSource>().isPlaying)
				Destroy(OST);
			else
			{
				OST.Play();
				OST.ignoreListenerPause = true;
				DontDestroyOnLoad(OST);
			}

			ShowMainMenuGroup();
		}

		/// <summary>Display the main menu section.</summary>
		public void ShowMainMenuGroup()
		{
			LoadGameMenuManager.CurrentLoadGamePage = 0;
			LoadGameGroup.gameObject.SetActive(false);
			NewGameGroup.gameObject.SetActive(false);
			MainMenuGroup.gameObject.SetActive(true);
		}

		/// <summary>Display the new game section.</summary>
		public void ShowNewGameGroup()
		{
			NumberOfEnemiesText.text = GameController.NumberOfEnemies.ToString();
			LoadGameGroup.gameObject.SetActive(false);
			MainMenuGroup.gameObject.SetActive(false);
			NewGameGroup.gameObject.SetActive(true);
		}


		/// <summary>Display the load game section.</summary>
		public void ShowLoadGameGroup()
		{
			MainMenuGroup.gameObject.SetActive(false);
			NewGameGroup.gameObject.SetActive(false);
			LoadGameMenuManager.ShowNextLoadGamePage();
			LoadGameGroup.gameObject.SetActive(true);
		}

		/// <summary>Increase the number of enemies counter in the new game section.</summary>
		public void MoreEnemies()
		{
			GameController.NumberOfEnemies++;
			GameController.NumberOfEnemies = Mathf.Clamp(GameController.NumberOfEnemies, 1, 4);
			NumberOfEnemiesText.text = GameController.NumberOfEnemies.ToString();
		}


		/// <summary>Decrease the number of enemies counter in the new game section.</summary>
		public void LessEnemies()
		{
			GameController.NumberOfEnemies--;
			GameController.NumberOfEnemies = Mathf.Clamp(GameController.NumberOfEnemies, 1, 4);
			NumberOfEnemiesText.text = GameController.NumberOfEnemies.ToString();
		}


		/// <summary>Launch a new game.</summary>
		public void StartNewGame()
		{
			GameController.SpawnPlanetsOnAwake = true;
			LoadGameMenuManager.LoadFileOnAwake = null;
			SceneManager.LoadScene("The Universe");
		}

		/// <summary>Load a savegame file at slot 1.</summary>
		public void LoadGameAtSlot1()
		{
			LoadGameMenuManager.LoadGame(0);
		}

		/// <summary>Load a savegame file at slot 2.</summary>
		public void LoadGameAtSlot2()
		{
			LoadGameMenuManager.LoadGame(1);
		}

		/// <summary>Load a savegame file at slot 3.</summary>
		public void LoadGameAtSlot3()
		{
			LoadGameMenuManager.LoadGame(2);
		}

		/// <summary>Load a savegame file at slot 4.</summary>
		public void LoadGameAtSlot4()
		{
			LoadGameMenuManager.LoadGame(3);
		}

		/// <summary>Exit the game.</summary>
		public void Exit()
		{
			Application.Quit();
		}
	}
}
