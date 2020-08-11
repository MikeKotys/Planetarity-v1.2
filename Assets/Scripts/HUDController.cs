using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mechanics;
using TMPro;
using UnityEngine.UI;
using System.Globalization;

namespace UI
{
	/// <summary>Handles the logic of heads up display for planets, rockets and rocket cooldowns.</summary>
	public class HUDController : MonoBehaviour
	{
		[Tooltip("The scene's GameController.")]
		public GameController GameController;

		[Tooltip("The transparent black square to show when Stinger is on cooldown.")]
		public Image StingerBackground;
		[Tooltip("The transparent black square to show when Thunder is on cooldown.")]
		public Image ThunderBackground;
		[Tooltip("The transparent black square to show when Megaton is on cooldown.")]
		public Image MegatonBackground;
		[Tooltip("The image that will be activated when Stinger is selected.")]
		public Image StingerSelected;
		[Tooltip("The image that will be activated when Thunder is selected.")]
		public Image ThunderSelected;
		[Tooltip("The image that will be activated when Megaton is selected.")]
		public Image MegatonSelected;
		[Tooltip("The text that shows the remaining Stinger cooldown.")]
		public TextMeshProUGUI StingerCooldown;
		[Tooltip("The text that shows the remaining Thunder cooldown.")]
		public TextMeshProUGUI ThunderCooldown;
		[Tooltip("The text that shows the remaining Megaton cooldown.")]
		public TextMeshProUGUI MegatonCooldown;

		[Header("HP HUDs.")]
		[Tooltip("The 3d text that shows the player's HP.")]
		public TextMeshPro PlayerPlanetHP;
		[Tooltip("The 3d text that shows the enemies' HP.")]
		public List<TextMeshPro> EnemyHP;

		/// <summary>Change the actively selected rocket.</summary>
		public void ChangeSelectedRocket(int newSelection)
		{
			StingerSelected.gameObject.SetActive(false);
			ThunderSelected.gameObject.SetActive(false);
			MegatonSelected.gameObject.SetActive(false);

			if (newSelection == 1)
				StingerSelected.gameObject.SetActive(true);
			else if (newSelection == 2)
				ThunderSelected.gameObject.SetActive(true);
			else
				MegatonSelected.gameObject.SetActive(true);
		}


		void Update()
		{
			if (GameController.PlayerPlanet != null)
			{
				PlayerPlanetHP.gameObject.SetActive(true);
				PlayerPlanetHP.text = GameController.PlayerPlanet.CurrentHP.ToString();
				PlayerPlanetHP.transform.position = GameController.PlayerPlanet.transform.position
					+ Vector3.back * GameController.PlayerPlanet.transform.localScale.x * 0.9f;
			}
			else
				PlayerPlanetHP.gameObject.SetActive(false);

			for (int i = 0; i < GameController.EnemyPlanets.Count; i++)
			{
				if (GameController.EnemyPlanets[i] != null)
				{
					EnemyHP[i].gameObject.SetActive(true);
					EnemyHP[i].text = GameController.EnemyPlanets[i].CurrentHP.ToString();
					EnemyHP[i].transform.position = GameController.EnemyPlanets[i].transform.position
						+ Vector3.back * GameController.EnemyPlanets[i].transform.localScale.x * 0.9f;
				}
				else
					EnemyHP[i].gameObject.SetActive(false);

			}

			if (GameController.PlayerPlanet != null)
			{
				if (GameController.PlayerPlanet.StingerCooldown > 0)
				{
					StingerBackground.gameObject.SetActive(true);
					StingerCooldown.gameObject.SetActive(true);
					StingerCooldown.text = GameController.PlayerPlanet.StingerCooldown.ToString("#,##0.00", CultureInfo.GetCultureInfo("en-US"));
				}
				else
				{
					StingerBackground.gameObject.SetActive(false);
					StingerCooldown.gameObject.SetActive(false);
				}

				if (GameController.PlayerPlanet.ThunderCooldown > 0)
				{
					ThunderBackground.gameObject.SetActive(true);
					ThunderCooldown.gameObject.SetActive(true);
					ThunderCooldown.text = GameController.PlayerPlanet.ThunderCooldown.ToString("#,##0.00", CultureInfo.GetCultureInfo("en-US"));
				}
				else
				{
					ThunderBackground.gameObject.SetActive(false);
					ThunderCooldown.gameObject.SetActive(false);
				}

				if (GameController.PlayerPlanet.MegatonCooldown > 0)
				{
					MegatonBackground.gameObject.SetActive(true);
					MegatonCooldown.gameObject.SetActive(true);
					MegatonCooldown.text = GameController.PlayerPlanet.MegatonCooldown.ToString("#,##0.00", CultureInfo.GetCultureInfo("en-US"));
				}
				else
				{
					MegatonBackground.gameObject.SetActive(false);
					MegatonCooldown.gameObject.SetActive(false);
				}
			}
		}
	}
}
