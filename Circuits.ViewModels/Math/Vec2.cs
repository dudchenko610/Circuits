namespace Circuits.ViewModels.Math;

public class Vec2
{
    public float X { get; set; }
    public float Y { get; set; }

    public Vec2()
    {
        X = 0;
        Y = 0;
    }

    public Vec2(float x, float y)
    {
        X = x;
        Y = y;
    }

    public Vec2(double x, double y)
    {
        X = (float)x;
        Y = (float)y;
    }

    public Vec2 Set(Vec2 other)
    {
        this.X = other.X;
        this.Y = other.Y;

        return this;
    }

    public Vec2 Set(double x, double y)
    {
        this.X = (float)x;
        this.Y = (float)y;

        return this;
    }

    public Vec2 Set(float x, float y)
    {
        this.X = x;
        this.Y = y;

        return this;
    }

    public Vec2 Add(double x, double y)
    {
        this.X += (float)x;
        this.Y += (float)y;

        return this;
    }

    public Vec2 Add(float x, float y)
    {
        this.X += x;
        this.Y += y;

        return this;
    }

    public Vec2 Add(Vec2 other)
    {
        this.X += other.X;
        this.Y += other.Y;

        return this;
    }

    public Vec2 Multiply(float scale)
    {
        this.X *= scale;
        this.Y *= scale;

        return this;
    }

    public string GetXStr()
    {
        return X.ToString().Replace(",", ".");
    }

    public string GetYStr()
    {
        return Y.ToString().Replace(",", ".");
    }
}