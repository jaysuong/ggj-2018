using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace boc {
	[CreateAssetMenu (menuName = "ScriptableObjects/Global/Swords")]
	public class SwordsGlobalEvent : GlobalEvent {
		[SerializeField]
		private GameObject swordPrefab;
		[SerializeField]
		private Vector3 startPosition;
		[SerializeField]
		private float spawnDistance;
		[SerializeField]
		private int numberToSpawn;
		[SerializeField]
		private float interval;

		private int amountSpawned;
		private float timer;

		public override void OnEventStart (GlobalEventManager manager) {
			timer = 0f;
			amountSpawned = 0;
		}

		public override void OnEventUpdate (GlobalEventManager manager) {
			timer -= Time.deltaTime;

			if (timer <= 0f) {
				timer = interval;

				var random = Random.insideUnitSphere * spawnDistance;
				random.y = 0f;
				var position = startPosition + random;

				Instantiate (swordPrefab, position, Quaternion.identity);
				amountSpawned++;

				if (amountSpawned >= numberToSpawn) {
					manager.EndEvent (this);
				}
			}
		}

	}
}