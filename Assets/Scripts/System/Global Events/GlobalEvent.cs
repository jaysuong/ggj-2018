using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace boc {
	public abstract class GlobalEvent : ScriptableObject {
		

		public virtual void OnEventStart(GlobalEventManager manager) {

		}

		public virtual void OnEventUpdate(GlobalEventManager manager) {

		}

		public virtual void OnEventEnd(GlobalEventManager manager) {

		}
	}
}