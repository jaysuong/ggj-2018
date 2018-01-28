using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace boc {
	public class Health : MonoBehaviour, IHealth {
		[SerializeField] private float InitialMaxHP = 100;
		[SerializeField] private GameEvent DeathEvent;
		public float CurrentHP { get; private set; }
		public float MaxHP {
			get { return InitialMaxHP; }
		}
		void Start () {
			CurrentHP = InitialMaxHP;
		}
		public void Heal (float amount) {
			CurrentHP = Mathf.Min (CurrentHP + amount, MaxHP);
		}
		public void Damage (float amount) {
			CurrentHP -= amount;
			// gameevent on overkill
			if (CurrentHP <= 0 && DeathEvent != null)
				DeathEvent.Invoke ();

		}
	}
}