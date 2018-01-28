using UnityEngine;

namespace boc.Server {

	[RequireComponent (typeof (ClientServerIntegration))]
	public class EventDataManager : MonoBehaviour {

		[SerializeField, Tooltip ("How long should we wait to pull from the server?")]
		private float waitTime = 15f;
		[SerializeField]
		private EventDataStorage eventDataStorage;

		private ClientServerIntegration clientServerIntegration;
		private float timer;

		private void Start () {
			clientServerIntegration = GetComponent<ClientServerIntegration> ();
			timer = 0f;
		}

		private void Update () {
			if (timer < waitTime) {
				timer += Time.deltaTime;
#if UNITY_EDITOR
				Debug.LogFormat ("Event 1: {0}, Event 2: {1}, Event 3: {2}",
					eventDataStorage.EventData.Event1,
					eventDataStorage.EventData.Event2,
					eventDataStorage.EventData.Event3);
#endif
			} else {
				clientServerIntegration.InvokeDataRetrieval ();
				timer = 0f;
			}
		}
	}
}