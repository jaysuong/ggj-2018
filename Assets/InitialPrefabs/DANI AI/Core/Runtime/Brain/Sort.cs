namespace InitialPrefabs.DANI {

	internal static class Sort {

		public static void QuickSort<T> (ref Tuple<T, float>[] arr, int low, int high) {
			if (low < high) {
				var pivotPoint = Partition (ref arr, low, high);
				QuickSort (ref arr, low, pivotPoint - 1);
				QuickSort (ref arr, pivotPoint + 1, high);
			}
		}

		/// <summary>
		/// Create partitions of an array.
		/// </summary>
		/// <param name="arr">Array of tupled values</param>
		/// <param name="low"></param>
		/// <param name="high"></param>
		/// <returns></returns>
		private static int Partition<T> (ref Tuple<T, float>[] arr, int low, int high) {
			var pivotValue = arr[high];
			var i = low - 1;

			for (var j = low; j <= high - 1; j++) {
				if (arr[j].Item2 <= pivotValue.Item2) {
					i++;
					Swap (ref arr[i], ref arr[j]);
				}
			}
			Swap (ref arr[i + 1], ref arr[high]);
			return i + 1;
		}

		/// <summary>
		/// Swaps the references of two elements.
		/// </summary>
		/// <param name="lhs">The reference of the supposed left element</param>
		/// <param name="rhs">The reference of the supposed right element</param>
		private static void Swap<T> (ref Tuple<T, float> lhs, ref Tuple<T, float> rhs) {
			var t = lhs;
			lhs = rhs;
			rhs = t;
		}
	}
}