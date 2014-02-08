﻿namespace SnappyMap.Collections
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class Grid<T> : IGrid<T>
    {
        private readonly T[] arr;

        public Grid(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            this.arr = new T[width * height];
        }

        public Grid(IGrid<T> source)
            : this(source.Width, source.Height)
        {
            for (int i = 0; i < source.Width * source.Height; i++)
            {
                this[i] = source[i];
            }
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public T this[int index]
        {
            get
            {
                return this.arr[index];
            }

            set
            {
                this.arr[index] = value;
            }
        }

        public T this[int x, int y]
        {
            get
            {
                return this.arr[this.ToIndex(x, y)];
            }

            set
            {
                this.arr[this.ToIndex(x, y)] = value;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.arr.Cast<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.arr.GetEnumerator();
        }
    }
}
