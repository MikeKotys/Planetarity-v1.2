using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Destroys a GameObject after a delay.</summary>
public class DestroyAfter : MonoBehaviour
{
	[Tooltip("How many seconds should pass before this script destroys its GameObject?")]
	public float DestructionDelay = 1;

	/// <summary>Should this component show the Defeat screen after it destroys the GameObject?</summary>
	[HideInInspector]
	public bool ShowDefeatScreenAfterwards = false;
	/// <summary>Should this component show the Victory screen after it destroys the GameObject?</summary>
	[HideInInspector]
	public bool ShowVictoryScreenAfterwards = false;

	/// <summary>Time since level load this script will destroy the GameObject.</summary>
	float DestructionTime = 0;

    void Awake()
    {
		DestructionTime = Time.timeSinceLevelLoad + DestructionDelay;
    }

    void Update()
    {
		if (Time.timeSinceLevelLoad > DestructionTime)
		{
			Destroy(gameObject);

			if (ShowDefeatScreenAfterwards)
			{
				var menu = FindObjectOfType<UI.GameMenuManager>();
				menu.ShowDefeatMenu();
			}
			else if (ShowVictoryScreenAfterwards)
			{
				var menu = FindObjectOfType<UI.GameMenuManager>();
				menu.ShowVictoryMenu();
			}
		}
    }
}
