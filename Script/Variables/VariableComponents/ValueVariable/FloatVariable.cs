using System.Linq;

namespace PlayBox
{
	public class FloatVariable : ValueVariable<float>
	{
		[PlayBoxFunction(uniqName: null, displayName: null, "Sum")]
		static float Summa(params float[] values) => values.Sum();
	}
}