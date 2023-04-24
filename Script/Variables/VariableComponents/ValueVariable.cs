using System;

namespace PlayBox
{
	[Serializable]
	public abstract class ValueVariable<T> : Variable
	{
		Func<T> getterFunction;
		Action<T> setterFunction;

		public T Value
		{
			get => getterFunction == null ? default : getterFunction.Invoke();
			set => setterFunction?.Invoke(value);
		}

		internal override Type ValueType => typeof(T);
	}
}