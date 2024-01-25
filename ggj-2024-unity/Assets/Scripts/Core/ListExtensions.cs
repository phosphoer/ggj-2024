using System.Collections.Generic;

public static class ListExtensions
{
  public static bool Contains<T>(this IReadOnlyList<T> list, T value)
  {
    for (int i = 0; i < list.Count; ++i)
    {
      if (Comparer<T>.Equals(value, list[i]))
        return true;
    }

    return false;
  }

  public static int IndexOf<T>(this IReadOnlyList<T> list, T value)
  {
    for (int i = 0; i < list.Count; ++i)
    {
      if (Comparer<T>.Equals(value, list[i]))
        return i;
    }

    return -1;
  }

  public static bool IsIndexValid<T>(this IReadOnlyList<T> list, int index)
  {
    return index >= 0 && index < list.Count;
  }

  public static int ClampIndex<T>(this IReadOnlyList<T> list, int index)
  {
    index = System.Math.Min(list.Count - 1, index);
    index = System.Math.Max(0, index);
    return index;
  }

  public static int WrapIndex<T>(this IReadOnlyList<T> list, int index)
  {
    return index % list.Count;
  }

  private static System.Random rng = new System.Random();
  public static void Shuffle<T>(this IList<T> list, System.Random rand = null)
  {
    if (rand == null)
      rand = rng;

    int n = list.Count;
    while (n > 1)
    {
      n--;
      int k = rng.Next(n + 1);
      T value = list[k];
      list[k] = list[n];
      list[n] = value;
    }
  }
}