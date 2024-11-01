using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncingBall.Models.Physics;

public struct Vector2(float x, float y)
{
    public float X = x;

    public float Y = y;

    public readonly override bool Equals([NotNullWhen(true)] object? obj) => (obj is Point pt && X == pt.X && Y == pt.Y) || (obj is Vector2 v && X == v.X && Y == v.Y);

    public readonly override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();

    public readonly float Length() => MathF.Sqrt(X * X + Y * Y);

    public readonly float LengthSquared() => X * X + Y * Y;

    public static Vector2 Zero => new(0, 0);

    public static Vector2 One => new(1, 1);

    public static Vector2 UnitX => new(1, 0);

    public static Vector2 UnitY => new(0, 1);

    public Vector2(float v1alue) : this(v1alue, v1alue) { }

    public static bool operator ==(Vector2 v1, Vector2 v2) => v1.Equals(v2);

    public static bool operator !=(Vector2 v1, Vector2 v2) => !v1.Equals(v2);

    public static Vector2 operator +(Vector2 v1, Vector2 v2) => new(v1.X + v2.X, v1.Y + v2.Y);

    public static Vector2 operator -(Vector2 v1, Vector2 v2) => v1 + -v2;

    public static Vector2 operator -(Vector2 v) => new(-v.X, -v.Y);

    public static Vector2 operator *(Vector2 v1, Vector2 v2) => new(v1.X * v2.X, v1.Y * v2.Y);

    public static Vector2 operator *(Vector2 v1, float scalar) => new(v1.X * scalar, v1.Y * scalar);

    public static Vector2 operator *(float scalar, Vector2 v1) => new(v1.X * scalar, v1.Y * scalar);

    public static Vector2 operator /(Vector2 v1, Vector2 v2) => new(v1.X / v2.X, v1.Y/ v2.Y);

    public static Vector2 operator /(Vector2 v1, float scalar) => new(v1.X / scalar, v1.Y / scalar);

    public static float Dot(Vector2 v1, Vector2 v2) => v1.X * v2.X + v1.Y * v2.Y;

    public static implicit operator Vector2(Point p) => new(p.X, p.Y);

    public void Normalize()
    {
        float length = MathF.Sqrt(X * X + Y * Y);
        if (length == 0)
        {
            return;
        }
        float num = 1f / length;
        X *= num;
        Y *= num;
    }

    public readonly Vector2 Normalized() {
        Vector2 result = new(X, Y);
        result.Normalize();
        return result;
    }

    public readonly Vector2 PerpendicularCounterClockwise() => new(-Y, X);
}
