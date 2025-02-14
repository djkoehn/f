namespace F.UI.Animations;

public static class Easing
{
    public static float Linear(float t)
    {
        return t;
    }

    public static float InQuad(float t)
    {
        return t * t;
    }

    public static float OutQuad(float t)
    {
        return 1 - InQuad(1 - t);
    }

    public static float InOutQuad(float t)
    {
        if (t < 0.5) return InQuad(t * 2) / 2;
        return 1 - InQuad((1 - t) * 2) / 2;
    }

    public static float InCubic(float t)
    {
        return t * t * t;
    }

    public static float OutCubic(float t)
    {
        return 1 - InCubic(1 - t);
    }

    public static float InOutCubic(float t)
    {
        if (t < 0.5) return InCubic(t * 2) / 2;
        return 1 - InCubic((1 - t) * 2) / 2;
    }

    public static float InQuart(float t)
    {
        return t * t * t * t;
    }

    public static float OutQuart(float t)
    {
        return 1 - InQuart(1 - t);
    }

    public static float InOutQuart(float t)
    {
        if (t < 0.5) return InQuart(t * 2) / 2;
        return 1 - InQuart((1 - t) * 2) / 2;
    }

    public static float InQuint(float t)
    {
        return t * t * t * t * t;
    }

    public static float OutQuint(float t)
    {
        return 1 - InQuint(1 - t);
    }

    public static float InOutQuint(float t)
    {
        if (t < 0.5) return InQuint(t * 2) / 2;
        return 1 - InQuint((1 - t) * 2) / 2;
    }

    public static float InSine(float t)
    {
        return 1 - Mathf.Cos(t * Mathf.Pi / 2);
    }

    public static float OutSine(float t)
    {
        return Mathf.Sin(t * Mathf.Pi / 2);
    }

    public static float InOutSine(float t)
    {
        return (Mathf.Cos(t * Mathf.Pi) - 1) / -2;
    }

    public static float InExpo(float t)
    {
        return Mathf.Pow(2, 10 * (t - 1));
    }

    public static float OutExpo(float t)
    {
        return 1 - InExpo(1 - t);
    }

    public static float InOutExpo(float t)
    {
        if (t < 0.5) return InExpo(t * 2) / 2;
        return 1 - InExpo((1 - t) * 2) / 2;
    }

    public static float InCirc(float t)
    {
        return -(Mathf.Sqrt(1 - t * t) - 1);
    }

    public static float OutCirc(float t)
    {
        return 1 - InCirc(1 - t);
    }

    public static float InOutCirc(float t)
    {
        if (t < 0.5) return InCirc(t * 2) / 2;
        return 1 - InCirc((1 - t) * 2) / 2;
    }

    public static float InElastic(float t)
    {
        return 1 - OutElastic(1 - t);
    }

    public static float OutElastic(float t)
    {
        var p = 0.3f;
        return Mathf.Pow(2, -10 * t) * Mathf.Sin((t - p / 4) * (2 * Mathf.Pi) / p) + 1;
    }

    public static float InOutElastic(float t)
    {
        if (t < 0.5) return InElastic(t * 2) / 2;
        return 1 - InElastic((1 - t) * 2) / 2;
    }

    public static float InBack(float t)
    {
        var s = 1.70158f;
        return t * t * ((s + 1) * t - s);
    }

    public static float OutBack(float t)
    {
        var c1 = 1.70158f;
        return 1 + --t * t * ((c1 + 1) * t + c1);
    }

    // Add overload with bounciness parameter
    public static float OutBack(float t, float bounciness)
    {
        return 1 + --t * t * ((bounciness + 1) * t + bounciness);
    }

    public static float InOutBack(float t)
    {
        if (t < 0.5) return InBack(t * 2) / 2;
        return 1 - InBack((1 - t) * 2) / 2;
    }

    public static float InBounce(float t)
    {
        return 1 - OutBounce(1 - t);
    }

    public static float OutBounce(float t)
    {
        var div = 2.75f;
        var mult = 7.5625f;

        if (t < 1 / div) return mult * t * t;

        if (t < 2 / div)
        {
            t -= 1.5f / div;
            return mult * t * t + 0.75f;
        }

        if (t < 2.5 / div)
        {
            t -= 2.25f / div;
            return mult * t * t + 0.9375f;
        }

        t -= 2.625f / div;
        return mult * t * t + 0.984375f;
    }

    public static float InOutBounce(float t)
    {
        if (t < 0.5) return InBounce(t * 2) / 2;
        return 1 - InBounce((1 - t) * 2) / 2;
    }
}