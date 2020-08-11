using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mechanics
{
	/// <summary>If this component is present on a GameObject, the GameObject will be saved into a savegame file.</summary>
	public class Savable : MonoBehaviour
	{
		[Tooltip("The prefab to use when loading this GameObject from a savegame file.")]
		public PrefabType PrefabType;
	}


	public enum PrefabType
	{
		PlayerPlanet,
		EnemyPlanet,
		Stinger,
		Thunder,
		Megaton
	}
}
