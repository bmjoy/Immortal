using System;

namespace com.ootii.Collections
{
    /// <summary>
    /// Provides extensions to arrays
    /// </summary>
    public static class ArrayExt
    {
        /// <summary>
        /// Updates the array by removing the specified index
        /// </summary>
        /// <typeparam name="T">Type of array to modify</typeparam>
        /// <param name="rSource">Source that is being modified</param>
        /// <param name="rIndex">Index ot remove</param>
        /// <returns>New array with the index removed</returns>
        public static void RemoveAt<T>(this T[] rSource, int rIndex)
        {
            if (rSource.Length == 0) { return; }
            if (rIndex < 0 || rIndex >= rSource.Length) { return; }

            T[] lNewArray = new T[rSource.Length - 1];

            if (rIndex > 0)
            {
                Array.Copy(rSource, 0, lNewArray, 0, rIndex);
            }

            if (rIndex < rSource.Length - 1)
            {
                Array.Copy(rSource, rIndex + 1, lNewArray, rIndex, rSource.Length - rIndex - 1);
            }

            rSource = lNewArray;
        }

        /// <summary>
        /// Provides an abstracted way to sort. We do it this way so we can change the method
        /// (ie using Linq) if we need to.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rSource"></param>
        /// <param name="rComparison"></param>
        public static void Sort<T>(this T[] rSource, Comparison<T> rComparison)
        {
            if (rSource.Length <= 1) { return; }

            Array.Sort(rSource, rComparison);
        }
    }
}
