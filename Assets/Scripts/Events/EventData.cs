using UnityEngine;

namespace boc.Server {

	[System.Serializable]
	public struct EventData {

		/// <summary>
		/// Returns the number of votes from the event_1 key.
		/// </summary>
		public int Event1 { get { return event_1; } }

		/// <summary>
		/// Returns the number of votes from the event_2 key.
		/// </summary>
		public int Event2 { get { return event_2; } }

		/// <summary>
		/// Returns the number of votes from the event_3 key.
		/// </summary>
		public int Event3 { get { return event_3; } }

		[SerializeField]
		private int event_1;
		[SerializeField]
		private int event_2;
		[SerializeField]
		private int event_3;

		/// <summary>
		/// Creates an event data from the event data.
		/// </summary>
		/// <param name="jsonData">The JSON string</param>
		/// <returns>The EventData representation of the JSON</returns>
		public static EventData CreateFromJSON (string jsonData) {
			return JsonUtility.FromJson<EventData> (jsonData);
		}
	}

}