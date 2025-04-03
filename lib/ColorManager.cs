using System.Numerics;

namespace event_planner_mupvp.lib;
public static class ColorManager
{
    public static Vector4 userColor = RGBA(255f, 255f, 255f, 195f);

    public static Vector4 vip0Color = RGBA(0f, 192f, 45f, 255f);

    public static Vector4 vip1Color = RGBA(0f, 164f, 246f, 255f);

    public static Vector4 vip2Color = RGBA(231f, 37f, 143f, 255f);

    public static Vector4 tutorColor = RGBA(112f, 63f, 157f, 255f);

    public static Vector4 gmColor = RGBA(129f, 152f, 224f, 255f);

    public static Vector4 cmColor = RGBA(228f, 63f, 79f, 255f);

    public static Vector4 FromRGBA(float r, float g, float b, float a)
    {
        return new Vector4(r, g, b, a);
    }

    private static Vector4 RGBA(float r, float g, float b, float a)
    {
        return new Vector4(r / 255f, g / 255f, b / 255f, a / 255f);
    }

    public static Vector4 GetUserColor(User user, float dynamicRed, float dynamicGreen, float dynamicBlue)
    {
        return user.UserType switch
        {
            UserTypes.USUARIO => userColor,
            UserTypes.VIP0 => vip0Color,
            UserTypes.VIP1 => vip1Color,
            UserTypes.VIP2 => vip2Color,
            UserTypes.TUTOR => tutorColor,
            UserTypes.GAMEMANAGER => gmColor,
            UserTypes.COMMUNITYMANAGER => cmColor,
            UserTypes.ADMINISTRADOR => FromRGBA(dynamicRed, dynamicGreen, dynamicBlue, 1f),
            UserTypes.CREADOR => FromRGBA(dynamicRed, dynamicGreen, dynamicBlue, 1f),
            _ => userColor
        };
    }
}