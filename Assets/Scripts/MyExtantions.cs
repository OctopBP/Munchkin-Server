﻿using System.Collections.Generic;
using UnityEngine;

public static class MyExtensions {
    public static void Shaffle<T>(this List<T> list) {
		if (list.Count < 2)
			return;
		
        for (int i = list.Count - 1; i >= 0; i--) {
            int r = Random.Range(0, i);
            list.Swap(i, r);
        }
    }

    public static void Swap<T>(this List<T> list, int i, int j) {
        T item = list[i];
        list[i] = list[j];
        list[j] = item;
    }

	public static bool Contain<T>(this T[] array, T value) {
		foreach (T temp in array) {
			if (temp.Equals(value))
				return true;
		}
		return false;
	}
}