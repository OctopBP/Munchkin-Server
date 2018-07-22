using System;
using UnityEngine;

public static class JsonReader {
	
	public static T[] ReadJson<T>(string fileName) {
		try {
			string filePath = "Data/Cards/" + fileName;

			TextAsset targetFile = Resources.Load<TextAsset>(filePath);
			Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(targetFile.text);

			return wrapper.cards;
		}
		catch {
			Debug.LogError("Cannot load JSON data!");
			return null;
		}
	}

	[Serializable]
	class Wrapper<T> {
		public T[] cards;
	}

}