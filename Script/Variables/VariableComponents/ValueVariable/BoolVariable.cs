using System.Linq;

namespace PlayBox
{
public class BoolVariable : ValueVariable<bool>
{
    [PlayBoxFunction]
    static bool And(params bool[] values) => values.All(value => value);
    
    [PlayBoxFunction]
    static bool Or(params bool[] values) => values.Any(value => value);
}
}