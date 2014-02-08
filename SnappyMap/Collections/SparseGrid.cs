﻿namespace SnappyMap.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SparseGrid<T> : ISparseGrid<T>
    {
        private readonly IDictionary<int, T> values;

        public SparseGrid(int width, int height)
        {
            this.values = new Dictionary<int, T>();
            this.Width = width;
            this.Height = height;
        }

        public int Width
        {
            get;
            private set;
        }

        public int Height
        {
            get;
            private set;
        }

        public IEnumerable<KeyValuePair<int, T>> IndexEntries
        {
            get
            {
                return this.values;
            }
        }

        public IEnumerable<KeyValuePair<GridCoordinates, T>> CoordinateEntries
        {
            get
            {
                return this.values.Select(
                    x => new KeyValuePair<GridCoordinates, T>(
                        this.ToCoords(x.Key),
                        x.Value));
            }
        }

        public IEnumerable<T> Values
        {
            get
            {
                return this.values.Values;
            }
        }

        public T this[int x, int y]
        {
            get
            {
                this.CheckIndexInBounds(x, y);

                T val;
                this.values.TryGetValue(this.ToIndex(x, y), out val);
                return val;
            }

            set
            {
                this.CheckIndexInBounds(x, y);

                if (EqualityComparer<T>.Default.Equals(value, default(T)))
                {
                    this.Remove(x, y);
                }
                else
                {
                    this.values[this.ToIndex(x, y)] = value;
                }
            }
        }

        public T this[int index]
        {
            get
            {
                var coords = this.ToCoords(index);
                return this[coords.X, coords.Y];
            }

            set
            {
                var coords = this.ToCoords(index);
                this[coords.X, coords.Y] = value;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int y = 0; y < this.Height; y++)
            {
                for (int x = 0; x < this.Width; x++)
                {
                    yield return this[x, y];
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool HasValue(int x, int y)
        {
            return this.HasValue(this.ToIndex(x, y));
        }

        public bool HasValue(int index)
        {
            this.CheckIndexInBounds(index);
            return this.values.ContainsKey(index);
        }

        public bool TryGetValue(int x, int y, out T val)
        {
            return this.TryGetValue(this.ToIndex(x, y), out val);
        }

        public bool TryGetValue(int index, out T val)
        {
            this.CheckIndexInBounds(index);
            return this.values.TryGetValue(index, out val);
        }

        public bool Remove(int x, int y)
        {
            return this.Remove(this.ToIndex(x, y));
        }

        public bool Remove(int index)
        {
            this.CheckIndexInBounds(index);
            return this.values.Remove(index);
        }

        private void CheckIndexInBounds(int index)
        {
            if (index < 0 || index >= this.Width * this.Height)
            {
                throw new IndexOutOfRangeException(
                    string.Format("index {0} is out of range", index));
            }
        }

        private void CheckIndexInBounds(int x, int y)
        {
            if (x < 0 || y < 0 || x >= this.Width || y >= this.Height)
            {
                throw new IndexOutOfRangeException(string.Format("Coordinates ({0}, {1}) out of range", x, y));
            }
        }
    }
}
