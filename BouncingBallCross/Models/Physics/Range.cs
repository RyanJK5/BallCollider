using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncingBall.Models.Physics;

public readonly struct Range<T>(T min, T max) where T : IComparable<T>
{
    public T Min { get; } = min;

    public T Max { get; } = max;
}
