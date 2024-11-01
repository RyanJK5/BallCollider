namespace BouncingBall.Models.Physics;

using System;
using System.Data;

public class Ball
{

    private Circle _bounds = new();
    public Circle Bounds => _bounds;

    public Vector2 Velocity;

    public int Number;

    public Color? Color;

    public float Radius
    {
        get => _bounds.Radius;
        set => _bounds.Radius = value;
    }

    public Point Center
    {
        get => _bounds.Center;
        set => _bounds.Center = value;
    }

    public Point TopLeft
    {
        get => new(Center.X - Radius, Center.Y - Radius);
    }

    public void Update(Random random, float gravity, float entropy)
    {
        Velocity.Y += Globals.TimerDelaySeconds * gravity;

        if (Velocity.X == 0 && gravity > 0 && entropy > 0)
        {
            int sign = random.NextDouble() > 0.5 ? 1 : -1;

            float oldY = Velocity.Y;
            float maxEntropy = Simulation.Rule.GetDefaultRange(Simulation.RuleType.Entropy).Max;
            Velocity.Y *= (maxEntropy - entropy) / maxEntropy;
            Velocity.X += sign * (oldY - Velocity.Y);
        }
        Center += (Point) Velocity;
    }

    public bool CollidesWith(Ball ball) => ball != this && (ball.Center - Center).Length() <= ball.Radius + Radius;

    public bool CollidesWithOuter(Circle outerCircle) => (Center - outerCircle.Center).Length() >= outerCircle.Radius - Radius;


    public static void OnCollision(Ball a, Ball b)
    {
        if (a.Center == b.Center)
        {
            a.Center += new Point(a.Radius + b.Radius, 0);
        }
        ReflectVelocities(a, b);

        Vector2 dist = b.Center - a.Center;
        float scalar = (a.Radius + b.Radius) - dist.Length();
        a.Center -= (scalar + 1) * dist.Normalized();
    }

    public void OnCollision(Circle outerCircle, out Point collisionPoint)
    {

        collisionPoint = (Center - outerCircle.Center).Normalized() * outerCircle.Radius;
        collisionPoint.X += outerCircle.Center.X;
        collisionPoint.Y += outerCircle.Center.Y;


        Center -= (Center - outerCircle.Center).Normalized() * Velocity.Length();

        Vector2 radiusLine = Center - outerCircle.Center;
        float scalar = radiusLine.Length() + Radius - outerCircle.Radius;
        if (scalar > 0)
        {
            Center -= scalar * radiusLine.Normalized();
        }

        ReflectVelocityOffOuter(outerCircle, Center);
    }

    private static void ReflectVelocities(Ball a, Ball b)
    {
        Vector2 _oldVelocity = a.Velocity;

        a.Velocity -= Vector2.Dot(a.Velocity - b.Velocity, a.Center - b.Center) / (a.Center - b.Center).LengthSquared()
            * (a.Center - b.Center);
        b.Velocity -= Vector2.Dot(b.Velocity - _oldVelocity, b.Center - a.Center) / (b.Center - a.Center).LengthSquared()
            * (b.Center - a.Center);
    }

    private void ReflectVelocityOffOuter(Circle outer, Vector2 collisionPoint)
    {
        Vector2 normal = collisionPoint - (Vector2) outer.Center;
        normal.Normalize();
        Vector2 tangent = normal.PerpendicularCounterClockwise();
        float normalSpeed = -Vector2.Dot(normal, Velocity);
        float tangentSpeed = Vector2.Dot(tangent, Velocity);
        Velocity = new Vector2(
            normalSpeed * normal.X + tangentSpeed * tangent.X,
            normalSpeed * normal.Y + tangentSpeed * tangent.Y
        );
    }
}