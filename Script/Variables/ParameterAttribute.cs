using System;
using UnityEngine;

namespace PlayBox
{
	[AttributeUsage(AttributeTargets.Field)]
	public class ParameterAttribute : PropertyAttribute
	{
		public string defaultName;

		public ParameterAttribute(string defaultName)
		{
			this.defaultName = defaultName;
		}

		public ParameterAttribute()
		{
		}
	}
}