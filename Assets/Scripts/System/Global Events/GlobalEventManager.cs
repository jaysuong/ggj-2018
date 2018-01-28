using System.Collections;
using System.Collections.Generic;
using boc.Server;
using UnityEngine;

namespace boc {
	public class GlobalEventManager : MonoBehaviour {
		[SerializeField]
		private EventPool event1;
		[SerializeField]
		private EventPool event2;
		[SerializeField]
		private EventPool event3;

		[SerializeField, Space]
		private EventDataStorage storage;
		[SerializeField]
		private ClientServerIntegration client;
		[SerializeField]
		private GameEvent startEvent;
		[SerializeField]
		private GameEvent endEvent;

		public GlobalEvent ActiveEvent { get; private set; }

		[System.Serializable]
		public struct EventPool {
			public GlobalEvent globalEvent;
			public int poolSize;
		}

		private void OnEnable () {
			if (startEvent != null) {
				startEvent.Register (HandleStartEvent);
			}

			if (endEvent != null) {
				endEvent.Register (StopAllCoroutines);
			}
		}

		private void OnDisable () {
			if (startEvent != null) {
				startEvent.Unregister (HandleStartEvent);
			}

			if (endEvent != null) {
				endEvent.Unregister (StopAllCoroutines);
			}
		}

		private void Update () {
			if (ActiveEvent != null) {
				ActiveEvent.OnEventUpdate (this);
			}
		}

		public void EndEvent (GlobalEvent ge) {
			if (ActiveEvent == ge) {
				ActiveEvent.OnEventEnd (this);
			}
		}

		private void HandleStartEvent () {
			StopAllCoroutines ();
			StartCoroutine (PlayPoolCheckSequence ());
		}

		private IEnumerator PlayPoolCheckSequence () {
			while (true) {
				if (storage.EventData.Event1 >= event1.poolSize) {
					PlayGlobalEvent (event1.globalEvent);
				} else if (storage.EventData.Event2 >= event2.poolSize) {
					PlayGlobalEvent (event2.globalEvent);
				} else if (storage.EventData.Event3 >= event3.poolSize) {
					PlayGlobalEvent (event3.globalEvent);
				}

				yield return null;
			}
		}

		private void PlayGlobalEvent (GlobalEvent ge) {
			if (ActiveEvent != ge) {
				ActiveEvent.OnEventEnd (this);
			}

			ActiveEvent = ge;
			ActiveEvent.OnEventStart (this);

			client.InvokeResetAllEvents ();
		}

	}
}