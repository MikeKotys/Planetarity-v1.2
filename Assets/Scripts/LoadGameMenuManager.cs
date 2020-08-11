using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using Mechanics;

namespace UI
{
	/// <summary>Handles the logic behind game loading as well as load game menu display logic.</summary>
	public class LoadGameMenuManager : MonoBehaviour
	{
		[Tooltip("The UI for the first diplayed savegame file name.")]
		public TextMeshProUGUI LoadSlot1;
		[Tooltip("The UI for the second diplayed savegame file name.")]
		public TextMeshProUGUI LoadSlot2;
		[Tooltip("The UI for the third diplayed savegame file name.")]
		public TextMeshProUGUI LoadSlot3;
		[Tooltip("The UI for the fourth diplayed savegame file name.")]
		public TextMeshProUGUI LoadSlot4;
		[Tooltip("The UI for the number of pages the load game list has.")]
		public TextMeshProUGUI LoadGamePageNumber;

		/// <summary>All the savegame files stored on disk.</summary>
		Dictionary<string, string> SavedGameFiles = new Dictionary<string, string>();
		/// <summary>How many load game pages can we show in the load game menu?</summary>
		int MaxLoadGamePages = 1;

		/// <summary>What load game menu page is the player on right now?</summary>
		[HideInInspector]
		public int CurrentLoadGamePage = 0;

		/// <summary>Should this class load a file stored in this string in Awake()? (null == no).</summary>
		public static string LoadFileOnAwake = null;

		private void Awake()
		{
			if (LoadFileOnAwake != null)
			{
				// Open the file
				StreamReader reader = new StreamReader(LoadFileOnAwake);
				if (reader != null)
				{
					var gameController = FindObjectOfType<GameController>();

					gameController.EnemyPlanets.Add(null);
					gameController.EnemyPlanets.Add(null);
					gameController.EnemyPlanets.Add(null);
					gameController.EnemyPlanets.Add(null);

					while (!reader.EndOfStream)
					{
						int outInt;
						float outFloat;

						// Read prefab type
						if (!int.TryParse(reader.ReadLine(), out outInt))
							break;

						// Spawn/select objects depending on their prefab type.
						var prefabType = (PrefabType)outInt;
						GameObject go = null;
						switch (prefabType)
						{
							case PrefabType.PlayerPlanet:
								go = GameObject.Find("Player's planet");
								break;
							case PrefabType.EnemyPlanet:
								go = Instantiate(gameController.EnemyPlanet);
								break;
							case PrefabType.Stinger:
								go = Instantiate(gameController.Stinger);
								break;
							case PrefabType.Thunder:
								go = Instantiate(gameController.Thunder);
								break;
							default:
								go = Instantiate(gameController.Megaton);
								break;
						}

						// Read position.
						Vector3 vector = Vector3.zero;

						if (!float.TryParse(reader.ReadLine(), out outFloat))
							break;
						vector.x = outFloat;
						if (!float.TryParse(reader.ReadLine(), out outFloat))
							break;
						vector.y = outFloat;
						if (!float.TryParse(reader.ReadLine(), out outFloat))
							break;
						vector.z = outFloat;
						go.transform.position = vector;

						// Read rotation.
						Quaternion rotation = Quaternion.identity;

						if (!float.TryParse(reader.ReadLine(), out outFloat))
							break;
						rotation.x = outFloat;
						if (!float.TryParse(reader.ReadLine(), out outFloat))
							break;
						rotation.y = outFloat;
						if (!float.TryParse(reader.ReadLine(), out outFloat))
							break;
						rotation.z = outFloat;
						if (!float.TryParse(reader.ReadLine(), out outFloat))
							break;
						rotation.w = outFloat;
						go.transform.rotation = rotation;

						// Read scale.
						vector = Vector3.one;

						if (!float.TryParse(reader.ReadLine(), out outFloat))
							break;
						vector.x = outFloat;
						if (!float.TryParse(reader.ReadLine(), out outFloat))
							break;
						vector.y = outFloat;
						if (!float.TryParse(reader.ReadLine(), out outFloat))
							break;
						vector.z = outFloat;
						go.transform.localScale = vector;

						// Read Planet/Rocket component parameters and apply them.
						var planet = go.GetComponent<Planet>();
						if (planet != null)
						{
							JsonUtility.FromJsonOverwrite(reader.ReadLine(), planet);

							if (prefabType == PrefabType.EnemyPlanet)
							{
								planet.SetColorsAndMaterials();
								if (gameController != null)
									gameController.EnemyPlanets[planet.PlanetNumber] = planet;
							}
						}
						else
						{
							var rocket = go.GetComponent<Rocket>();
							JsonUtility.FromJsonOverwrite(reader.ReadLine(), rocket);
						}
					}
				}

				LoadFileOnAwake = null;
			}
		}

		/// <summary>Build the list of all savegames on the disk.</summary>
		private void UpdateSavedGameFiles()
		{
			SavedGameFiles.Clear();
			var files = Directory.GetFiles(Application.dataPath);
			for (int i = 0; i < files.Length; i++)
			{
				if (files[i].Contains("PSG_")
#if UNITY_EDITOR
					&& !files[i].Contains(".meta")
#endif
					)
				{
					var name = Path.GetFileName(files[i]);
					name = name.Substring(4, name.Length - 8);
					SavedGameFiles[name] = files[i];
				}
			}
			MaxLoadGamePages = SavedGameFiles.Count / 4;

			if (SavedGameFiles.Count % 4 != 0)
				MaxLoadGamePages++;
		}


		/// <summary>Update the load game menu page given the CurrentLoadGamePage.</summary>
		void ChangeLoadGamePage()
		{
			if (SavedGameFiles.Count > (CurrentLoadGamePage) * 4)
				LoadSlot1.text = SavedGameFiles.ElementAt((CurrentLoadGamePage) * 4).Key;

			if (SavedGameFiles.Count > (CurrentLoadGamePage) * 4 + 1)
				LoadSlot2.text = SavedGameFiles.ElementAt((CurrentLoadGamePage) * 4 + 1).Key;
			else
				LoadSlot2.text = "............";

			if (SavedGameFiles.Count > (CurrentLoadGamePage) * 4 + 2)
				LoadSlot3.text = SavedGameFiles.ElementAt((CurrentLoadGamePage) * 4 + 2).Key;
			else
				LoadSlot3.text = "............";

			if (SavedGameFiles.Count > (CurrentLoadGamePage) * 4 + 3)
				LoadSlot4.text = SavedGameFiles.ElementAt((CurrentLoadGamePage) * 4 + 3).Key;
			else
				LoadSlot4.text = "............";
		}

		/// <summary>Go to previous load game page.</summary>
		public void ShowPreviousLoadGamePage()
		{
			if (CurrentLoadGamePage > 1)
			{
				UpdateSavedGameFiles();
				CurrentLoadGamePage -= 2;
				CurrentLoadGamePage = Mathf.Clamp(CurrentLoadGamePage, 0, MaxLoadGamePages);
				ChangeLoadGamePage();
				CurrentLoadGamePage++;
				CurrentLoadGamePage = Mathf.Clamp(CurrentLoadGamePage, 0, MaxLoadGamePages);
				LoadGamePageNumber.text = CurrentLoadGamePage.ToString() + "/" + MaxLoadGamePages;
			}
		}

		/// <summary>Go to next load game page.</summary>
		public void ShowNextLoadGamePage()
		{
			UpdateSavedGameFiles();
			if (CurrentLoadGamePage < MaxLoadGamePages)
			{
				ChangeLoadGamePage();
				CurrentLoadGamePage++;
				CurrentLoadGamePage = Mathf.Clamp(CurrentLoadGamePage, 0, MaxLoadGamePages);
				LoadGamePageNumber.text = CurrentLoadGamePage.ToString() + "/" + MaxLoadGamePages;
			}
		}

		/// <summary>Load the selected savegame file. This loads 'The Universe' scene from scratch and saves the savegame file
		///		in a static variable to be loaded after the scen finished loading.</summary>
		public void LoadGame(int slotNum)
		{
			int saveNum = (CurrentLoadGamePage - 1) * 4 + slotNum;
			if (SavedGameFiles.Count > saveNum
				&& File.Exists(SavedGameFiles[SavedGameFiles.ElementAt(saveNum).Key]))
			{
				GameController.SpawnPlanetsOnAwake = false;
				LoadFileOnAwake = SavedGameFiles[SavedGameFiles.ElementAt(saveNum).Key];
				SceneManager.LoadScene("The Universe");
			}
		}
	}
}
