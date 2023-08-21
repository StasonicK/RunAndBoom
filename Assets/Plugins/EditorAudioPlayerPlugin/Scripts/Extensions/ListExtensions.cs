using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MBS.Tools
{
    /// <summary>
    /// List extensions
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Swaps two items in a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        public static void MBSSwap<T>(this IList<T> list, int i, int j)
        {
            T temporary = list[i];
            list[i] = list[j];
            list[j] = temporary;
        }

        /// <summary>
        /// Shuffles a list randomly
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void MBSShuffle<T>(this IList<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list.MBSSwap(i, Random.Range(i, list.Count));
            }
        }

        public static List<T> Clone<T>(this List<T> list)
        {
            return list.GetRange(0, list.Count);
        }

        public static T GetLast<T>(this List<T> list)
        {
            return list[list.Count - 1];
        }

        /// <summary>
        /// Checks if the contents of two lists are equal
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="otherList"></param>
        /// <param name="orderMatters"></param>
        /// <returns></returns>
        public static bool ContentsAreEqual<T>(this List<T> list, List<T> otherList, bool orderMatters = false)
        {
            if (list.Count != otherList.Count)
                return false;

            if (orderMatters)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (!list[i].Equals(otherList[i]))
                        return false;
                }
            }
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (!list.Contains(otherList[i]))
                        return false;
                }
            }

            return true;
        }
    }
}