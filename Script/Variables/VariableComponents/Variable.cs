using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PlayBox
{
	public abstract class Variable
	{
		public bool isGUISettingEnabled = true;
		public List<Object> otherObjectChangingWithVariable;
		public MonoBehaviour sourceComponent;

		public GameObject GameObject => sourceComponent?.gameObject;

		public string NiceName;
		public string IDName;
		public string ElementName;

		internal IEnumerable<Object> ChangingObjects
		{
			get
			{
				yield return sourceComponent;
				if (otherObjectChangingWithVariable != null)
					foreach (Object otherObject in otherObjectChangingWithVariable)
						yield return otherObject;
			}
		}

		internal abstract Type ValueType { get; }
		public List<string> _path = new();
		public IReadOnlyList<string> Path => _path;
	}
}