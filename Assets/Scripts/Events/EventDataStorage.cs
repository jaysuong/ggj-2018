using UnityEngine;

namespace boc.Server {

	[CreateAssetMenu (fileName = "Event Data Storage", menuName = "ScriptableObjects/Event Data Storage")]
	public class EventDataStorage : ScriptableObject {

		public EventData EventData {
			get { return eventData; }
			set { eventData = value; }
		}

		private EventData eventData;
	}

}