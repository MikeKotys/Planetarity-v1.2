using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using Mechanics;
using System.IO;

namespace UI
{
	/// <summary>Handles the logic of the in-game menu. Processes player input related to in-game menu.</summary>
	public class GameMenuManager : MonoBehaviour
	{
		#region Parameters
		[Tooltip("The texture that will be used to replace the default mouse pointer in-game.")]
		public Texture2D CursorTexture;

		[Tooltip("The game menu parent object.")]
		public CanvasGroup GameMenu;
		[Tooltip("The main menu secton parent object.")]
		public CanvasGroup MainMenuGroup;
		[Tooltip("The load game section parent object.")]
		public CanvasGroup LoadGameGroup;
		[Tooltip("The save game section parent object.")]
		public CanvasGroup SaveGameGroup;
		[Tooltip("The game was saved screen parent object.")]
		public CanvasGroup GameWasSavedGroup;
		[Tooltip("The victory screen parent object.")]
		public CanvasGroup Victory;
		[Tooltip("The defeat screen parent object.")]
		public CanvasGroup Defeat;
		[Tooltip("The input field that hold the chosen name for the savegame file.")]
		public TextMeshProUGUI SaveFileName;
		[Tooltip("The video file of the sun.")]
		public VideoPlayer SunVideo;
		[Tooltip("The link to the class that handles game loading logic.")]
		public LoadGameMenuManager LoadGameMenuManager;

		/// <summary>Are we in the main menu section right now or not?</summary>
		bool CurrentMenuIsMainMenu = true;
		#endregion

		#region Awake
		private void Awake()
		{
			Cursor.SetCursor(CursorTexture, new Vector2(15, 15), CursorMode.Auto);

			ShowMainMenu();
			GameMenu.gameObject.SetActive(false);
		}
		#endregion

		#region Menu logic
		/// <summary>Display the main menu section.</summary>
		public void ShowMainMenu()
		{
			LoadGameMenuManager.CurrentLoadGamePage = 0;
			CurrentMenuIsMainMenu = true;
			Victory.gameObject.SetActive(false);
			Defeat.gameObject.SetActive(false);
			GameWasSavedGroup.gameObject.SetActive(false);
			LoadGameGroup.gameObject.SetActive(false);
			SaveGameGroup.gameObject.SetActive(false);
			MainMenuGroup.gameObject.SetActive(true);
		}


		/// <summary>Display the load game section.</summary>
		public void ShowLoadGameMenu()
		{
			CurrentMenuIsMainMenu = false;
			Victory.gameObject.SetActive(false);
			Defeat.gameObject.SetActive(false);
			GameWasSavedGroup.gameObject.SetActive(false);
			MainMenuGroup.gameObject.SetActive(false);
			SaveGameGroup.gameObject.SetActive(false);
			LoadGameMenuManager.ShowNextLoadGamePage();
			LoadGameGroup.gameObject.SetActive(true);
		}

		/// <summary>Display the save game section.</summary>
		public void ShowSaveGameMenu()
		{
			CurrentMenuIsMainMenu = false;
			Victory.gameObject.SetActive(false);
			Defeat.gameObject.SetActive(false);
			GameWasSavedGroup.gameObject.SetActive(false);
			MainMenuGroup.gameObject.SetActive(false);
			LoadGameGroup.gameObject.SetActive(false);
			SaveGameGroup.gameObject.SetActive(true);
		}

		/// <summary>Display the victory screen.</summary>
		public void ShowVictoryMenu()
		{
			PauseTheGame();
			Defeat.gameObject.SetActive(false);
			MainMenuGroup.gameObject.SetActive(false);
			LoadGameGroup.gameObject.SetActive(false);
			SaveGameGroup.gameObject.SetActive(false);
			GameWasSavedGroup.gameObject.SetActive(false);
			Victory.gameObject.SetActive(true);
		}

		/// <summary>Display the defeat screen.</summary>
		public void ShowDefeatMenu()
		{
			PauseTheGame();
			MainMenuGroup.gameObject.SetActive(false);
			LoadGameGroup.gameObject.SetActive(false);
			SaveGameGroup.gameObject.SetActive(false);
			GameWasSavedGroup.gameObject.SetActive(false);
			Victory.gameObject.SetActive(false);
			Defeat.gameObject.SetActive(true);
		}

		/// <summary>Exits to the main menu.</summary>
		public void ExitToMenu()
		{
			SceneManager.LoadScene("Main Menu");
			UnpauseTheGame();
			Cursor.SetCursor(null, new Vector2(0, 0), CursorMode.Auto);
		}

		/// <summary>Only pauses the game.</summary>
		void PauseTheGame()
		{
			Cursor.SetCursor(null, new Vector2(0, 0), CursorMode.Auto);
			SunVideo.Pause();
			Time.timeScale = 0;
			AudioListener.pause = true;
		}

		/// <summary>Only unpauses the game.</summary>
		void UnpauseTheGame()
		{
			Cursor.SetCursor(CursorTexture, new Vector2(15, 15), CursorMode.Auto);
			SunVideo.Play();
			Time.timeScale = 1;
			AudioListener.pause = false;
		}

		/// <summary>Closes the in-game menu and unpauses the game.</summary>
		public void BackToGame()
		{
			GameMenu.gameObject.SetActive(false);
			UnpauseTheGame();
		}


		/// <summary>Saves the game into a file with a chosen name.</summary>
		public void SaveGame()
		{
			if (!string.IsNullOrWhiteSpace(SaveFileName.text))
			{
				string saveFileText = "";

				// Get all GameObjects in scene root.
				var allGOs = SceneManager.GetActiveScene().GetRootGameObjects();
				for (int i = 0; i < allGOs.Length; i++)
				{
					var savable = allGOs[i].GetComponent<Savable>();

					// If a GameObject has a Savable component - it must be saved.
					if (savable != null)
					{
						// Save prefab type.
						saveFileText += ((int)savable.PrefabType).ToString() + "\n";

						// Save position.
						saveFileText += savable.transform.position.x.ToString() + "\n";
						saveFileText += savable.transform.position.y.ToString() + "\n";
						saveFileText += savable.transform.position.z.ToString() + "\n";

						// Save rotation.
						saveFileText += savable.transform.rotation.x.ToString() + "\n";
						saveFileText += savable.transform.rotation.y.ToString() + "\n";
						saveFileText += savable.transform.rotation.z.ToString() + "\n";
						saveFileText += savable.transform.rotation.w.ToString() + "\n";

						// Save scale.
						saveFileText += savable.transform.localScale.x.ToString() + "\n";
						saveFileText += savable.transform.localScale.y.ToString() + "\n";
						saveFileText += savable.transform.localScale.z.ToString() + "\n";

						// Save the planet/rocket components parameters.
						var planet = allGOs[i].GetComponent<Planet>();
						if (planet != null)
							saveFileText += JsonUtility.ToJson(planet) + "\n";
						else
						{
							var rocket = allGOs[i].GetComponent<Rocket>();
							saveFileText += JsonUtility.ToJson(rocket) + "\n";
						}
					}
				}
				File.WriteAllText(Application.dataPath + "/PSG_" + SaveFileName.text + ".txt", saveFileText);

				// Show the game was saved screen.
				CurrentMenuIsMainMenu = false;
				Victory.gameObject.SetActive(false);
				Defeat.gameObject.SetActive(false);
				MainMenuGroup.gameObject.SetActive(false);
				LoadGameGroup.gameObject.SetActive(false);
				SaveGameGroup.gameObject.SetActive(false);
				GameWasSavedGroup.gameObject.SetActive(true);
			}
		}

		/// <summary>Load a savegame file at slot 1.</summary>
		public void LoadGameAtSlot1()
		{
			LoadGameMenuManager.LoadGame(0);
			UnpauseTheGame();
		}

		/// <summary>Load a savegame file at slot 2.</summary>
		public void LoadGameAtSlot2()
		{
			LoadGameMenuManager.LoadGame(1);
			UnpauseTheGame();
		}

		/// <summary>Load a savegame file at slot 3.</summary>
		public void LoadGameAtSlot3()
		{
			LoadGameMenuManager.LoadGame(2);
			UnpauseTheGame();
		}

		/// <summary>Load a savegame file at slot 4.</summary>
		public void LoadGameAtSlot4()
		{
			LoadGameMenuManager.LoadGame(3);
			UnpauseTheGame();
		}
		#endregion



		#region Update
		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				// Toggle menu with Escape
				if (GameMenu.gameObject.activeSelf)
				{
					if (CurrentMenuIsMainMenu)
					{
						GameMenu.gameObject.SetActive(false);
						UnpauseTheGame();
					}
					else
						ShowMainMenu();
				}
				else
				{
					PauseTheGame();
					GameMenu.gameObject.SetActive(true);
				}
			}
		}
		#endregion
	}
}
