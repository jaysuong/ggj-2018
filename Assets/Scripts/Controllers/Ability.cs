using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace boc {
	public abstract class Ability : ScriptableObject {
		public virtual void OnAbilityStart (GameObject player) { }
		public virtual void OnAbilityUpdate (GameObject player) { }
		public virtual void OnAbilityEnd (GameObject player) { }

	}
}