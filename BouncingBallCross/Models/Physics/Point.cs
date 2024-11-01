using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncingBall.Models.Physics;

public struct Point(float x, float y)
{
    public float X = x;

    public float Y = y;

    public readonly override bool Equals([NotNullWhen(true)] object? obj) => obj is Point pt && X == pt.X && Y == pt.Y;

    public readonly override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();

    public static bool operator ==(Point pt1, Point pt2) => pt1.Equals(pt2);
    
    public static bool operator !=(Point pt1, Point pt2) => !pt1.Equals(pt2);

    public static Point operator +(Point pt1, Point pt2) => new(pt1.X + pt2.X, pt1.Y + pt2.Y);

    public static Vector2 operator -(Point pt1, Point pt2) => pt1 + -pt2;

    public static Point operator -(Point pt, Vector2 v) => pt - (Point)v;

    public static Point operator +(Point pt, Vector2 v) => pt + (Point)v;


    public static Point operator -(Point pt) => new(-pt.X, -pt.Y);

    public static Point operator *(float scalar, Point pt) => new(pt.X * scalar, pt.Y * scalar);

    public static implicit operator Point(Vector2 v) => new(v.X, v.Y);

    public readonly float DistanceFrom(Point pt) => MathF.Sqrt(MathF.Pow(pt.X - X, 2) + MathF.Pow(pt.Y - Y, 2));

    public static float Dist(Point p1, Point p2) => p1.DistanceFrom(p2);
}
