using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace boc {
	public class AbilityManager : MonoBehaviour {
		[SerializeField]
		private List<Ability> abilities;

		// Use this for initialization
		void Start () {
			for (int i = 0; i < abilities.Count; ++i) {
				abilities[i].OnAbilityStart(gameObject);

			}
		}

		// Update is called once per frame
		void Update () {
			for (int i = 0; i < abilities.Count; ++i) {
				abilities[i].OnAbilityUpdate (gameObject);
			}
		}
		public void AddAbility (Ability ability) {
			abilities.Add (ability);
			ability.OnAbilityStart (gameObject);
		}
		public void RemoveAbility (Ability ability) {
			abilities.Remove (ability);
			ability.OnAbilityEnd (gameObject);
		}
		private void OnDisable () {
			for (int i = 0; i < abilities.Count; ++i) {
				abilities[i].OnAbilityEnd (gameObject);
			}
		}
	}
}