using System.Numerics;

public static class ColorManager
{
    public static Vector4 userColor = RGBA(255f, 255f, 255f, 195f);

    public static Vector4 vip0Color = RGBA(0f, 192f, 45f, 255f);

    public static Vector4 vip1Color = RGBA(0f, 164f, 246f, 255f);

    public static Vector4 vip2Color = RGBA(231f, 37f, 143f, 255f);

    public static Vector4 tutorColor = RGBA(112f, 63f, 157f, 255f);

    public static Vector4 gmColor = RGBA(129f, 152f, 224f, 255f);

    public static Vector4 cmColor = RGBA(228f, 63f, 79f, 255f);

    public static Vector4 FromRGBA(float r, float g, float b, float a) => new(r, g, b, a);

    private static Vector4 RGBA(float r, float g, float b, float a) => new(r / 255f, g / 255f, b / 255f, a / 255f);
}