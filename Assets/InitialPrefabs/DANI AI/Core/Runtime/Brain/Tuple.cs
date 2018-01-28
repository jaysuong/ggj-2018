namespace InitialPrefabs.DANI {

    internal struct Tuple<T, U> {
        public T Item1 { get; set; }
        public U Item2 { get; set; }

        public Tuple (T item1, U item2) {
            Item1 = item1;
            Item2 = item2;
        }
    }
}