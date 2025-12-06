using System;

namespace NPTP.ReferenceableScriptables.Utilities
{
    public static class ArrayExtensions
    {
        public static T[] WithElementAdded<T>(this T[] array, T element)
        {
            T[] newArrayWithElementAdded = new T[array.Length + 1];
            Array.Copy(array, newArrayWithElementAdded, array.Length);
            newArrayWithElementAdded[^1] = element;
            return newArrayWithElementAdded;
        }

        public static T[] WithRemovedAt<T>(this T[] array, int index)
        {
            if (index < 0 || index >= array.Length)
            {
                return array;
            }

            T[] newArrayWithElementRemoved = new T[array.Length - 1];

            for (int i = 0; i < array.Length; i++)
            {
                if (i == index)
                {
                    continue;
                }

                int j = i > index ? i - 1 : i;
                newArrayWithElementRemoved[j] = array[i];
            }

            return newArrayWithElementRemoved;
        }
    }
}
