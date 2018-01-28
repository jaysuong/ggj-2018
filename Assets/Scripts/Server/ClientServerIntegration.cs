using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace boc.Server {

	public delegate void Event1Handler ();
	public delegate void Event2Handler ();
	public delegate void Event3Handler ();
	public delegate void ResetAllEventHandler ();

	public class ClientServerIntegration : MonoBehaviour {

		public static event Event1Handler OnResetEvent1;
		public static event Event2Handler OnResetEvent2;
		public static event Event3Handler OnResetEvent3;
		public static event ResetAllEventHandler OnResetEvents;

		public EventData EventData { get { return EventData.CreateFromJSON (eventData); } }
		public string Event1URL { get { return string.Format ("{0}/{1}-{2}", hostURL, resetURLPrefix, event1URL); } }
		public string Event2URL { get { return string.Format ("{0}/{1}-{2}", hostURL, resetURLPrefix, event2URL); } }
		public string Event3URL { get { return string.Format ("{0}/{1}-{2}", hostURL, resetURLPrefix, event3URL); } }
		public string ResetAllURL { get { return string.Format ("{0}/{1}{2}", hostURL, resetURLPrefix, "-all-events"); } }

		private const string resetURLPrefix = "_reset";

		[SerializeField, Tooltip ("What is the host name for the server?")]
		private string hostURL;
		[SerializeField, Tooltip ("What is the event 1 url?")]
		private string event1URL = "event1";
		[SerializeField, Tooltip ("What is the event 2 url?")]
		private string event2URL = "event2";
		[SerializeField, Tooltip ("What is the event 3 url?")]
		private string event3URL = "event3";
		[SerializeField, Tooltip ("What is the url for retrieving a JSON obj?")]
		private string retrieveDataURL = "get-data";
		[SerializeField]
		private EventDataStorage eventDataStorage;

		private string eventData;

		private void OnEnable () {
			OnResetEvent1 += InvokeEvent1Reset;
			OnResetEvent2 += InvokeEvent2Reset;
			OnResetEvent3 += InvokeEvent3Reset;
			OnResetEvents += InvokeResetAllEvents;
		}

		private void OnDisable () {
			OnResetEvent1 -= InvokeEvent1Reset;
			OnResetEvent2 -= InvokeEvent2Reset;
			OnResetEvent3 -= InvokeEvent3Reset;
			OnResetEvents -= InvokeResetAllEvents;
		}

		private void Start () {
			Assert.IsFalse (string.IsNullOrEmpty (hostURL), "Server URL does not exist!");
		}

		private IEnumerator RetrieveEventData () {
			var dataURL = string.Format ("{0}/{1}", hostURL, retrieveDataURL);
			var www = new WWW (dataURL);
			yield return www;
			eventData = www.text;
#if UNITY_EDITOR
			Debug.LogFormat ("Event 1: {0}, Event 2: {1}, Event 3: {2}", EventData.Event1, EventData.Event2, EventData.Event3);
#endif
			// Set the event data
			eventDataStorage.EventData = EventData;
		}

		private IEnumerator ResetEvent (string url) {
			var www = new WWW (url);
			yield return www;
			eventData = www.text;
			eventDataStorage.EventData = EventData;
		}

		/// <summary>
		/// Starts an async coroutine to retrieve data.
		/// </summary>
		public void InvokeDataRetrieval () {
			StartCoroutine (RetrieveEventData ());
		}

		/// <summary>
		/// Starts an async coroutine to reset the event data from a specific event.
		/// </summary>
		/// <param name="url">The URL to invoke</param>
		public void InvokeResetEvent (string url) {
			StartCoroutine (ResetEvent (url));
		}

		public void InvokeEvent1Reset () {
			InvokeResetEvent (Event1URL);
		}

		public void InvokeEvent2Reset () {
			InvokeResetEvent (Event2URL);
		}

		public void InvokeEvent3Reset () {
			InvokeResetEvent (Event3URL);
		}

		public void InvokeResetAllEvents () {
			InvokeResetEvent (ResetAllURL);
		}

	}
}