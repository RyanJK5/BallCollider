using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncingBall.Models.Physics;

public struct Circle(Point center, float radius)
{
    public Point Center = center;

    public float Radius = radius;

    public readonly float Diameter => Radius * 2;

    public readonly float Circumference => MathF.PI * Diameter;

    public readonly bool Contains(Point p) => Center.DistanceFrom(p) <= Radius;

    public readonly bool Intersects(Circle circle)
    {
        Vector2 vector = circle.Center - Center;
        return Vector2.Dot(vector, vector) <= MathF.Pow(Radius + circle.Radius, 2);
    }

    public Circle(float x, float y, float radius) : this(new Point(x, y), radius) { }

    public readonly override bool Equals(object? obj) => obj is Circle circ && Center == circ.Center && Radius == circ.Radius;

    public readonly override int GetHashCode() => HashCode.Combine(Center, Radius);
}
