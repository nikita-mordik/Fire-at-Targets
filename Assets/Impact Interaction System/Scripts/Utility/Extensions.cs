using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Impact.Utility
{
    public static class Extensions
    {
        /// <summary>
        /// Sets the bit at the given position to 1.
        /// </summary>
        /// <param name="bitmask">The bitmask to modify.</param>
        /// <param name="pos">The index of the bit to set.</param>
        /// <returns>The bitmask with the bit at the given position set to 1.</returns>
        public static int SetBit(this int bitmask, int pos)
        {
            return bitmask | (1 << pos);
        }

        /// <summary>
        /// Sets the bit at the given position to 0.
        /// </summary>
        /// <param name="bitmask">The bitmask to modify.</param>
        /// <param name="pos">The index of the bit to unset.</param>
        /// <returns>The bitmask with the bit at the given position set to 0.</returns>
        public static int UnsetBit(this int bitmask, int pos)
        {
            return bitmask & ~(1 << pos);
        }

        /// <summary>
        /// Is the bit at the given position set to 1?
        /// </summary>
        /// <param name="bitmask">The bitmask to check against.</param>
        /// <param name="pos">The index of the bit to check.</param>
        /// <returns>True if the bit is set to 1, false otherwise.</returns>
        public static bool IsBitSet(this int bitmask, int pos)
        {
            return (bitmask & (1 << pos)) != 0;
        }

        /// <summary>
        /// Either gets a component on the given game object or adds one.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <param name="gameObject">The game object to get or add the component to.</param>
        /// <param name="checkParents">Should we look for the component in parent objects?</param>
        /// <returns>A reference to the existing or new component.</returns>
        public static T GetOrAddComponent<T>(this GameObject gameObject, bool checkParents) where T : Component
        {
            T existing = checkParents ? gameObject.GetComponentInParent<T>() : gameObject.GetComponent<T>();
            if (existing != null)
                return existing;

            return gameObject.AddComponent<T>();
        }

        /// <summary>
        /// Gets the index of the first element matching the given predicate.
        /// </summary>
        /// <typeparam name="T">The array type.</typeparam>
        /// <param name="array">The array.</param>
        /// <param name="predicate">The predicate to match.</param>
        /// <returns>The index of the first element matching the given predicate.</returns>
        public static int IndexOf<T>(this T[] array, Predicate<T> predicate)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (predicate(array[i]))
                    return i;
            }

            return -1;
        }
    }
}

