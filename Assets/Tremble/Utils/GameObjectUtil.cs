//
// This file is part of the Tremble package by Tiny Goose.
// Copyright (c) 2024-2025 TinyGoose Ltd., All Rights Reserved.
//

using System;
using UnityEngine;

namespace TinyGoose.Tremble
{
	public static class GameObjectUtil
	{
		public static GameObject Find_Recursive(this GameObject go, string findName)
		{
			if (go.name.EqualsInvariant(findName, caseSensitive: true))
				return go;

			for (int i = 0; i < go.transform.childCount; i++)
			{
				GameObject inner = Find_Recursive(go.transform.GetChild(i).gameObject, findName);
				if (inner)
					return inner;
			}

			return null;
		}
		
		public static bool TryGetComponentInChildren<T>(this GameObject gameObject, out T component) where T : class
		{
			component = null;
			if (gameObject == null)
				return false;
			component = gameObject.GetComponentInChildren<T>();

			return component != null;
		}
		
		public static bool TryGetComponentInParent<T>(this GameObject gameObject, out T component) where T : class
		{
			component = null;
			if (gameObject == null)
				return false;
			component = gameObject.GetComponentInParent<T>(true);

			return component != null;
		}
		
		public static bool TryGetComponentInParent<T>(this Component c, out T component) where T : class
		{
			component = c.GetComponentInParent<T>(true);

			return component != null;
		}
	}
}