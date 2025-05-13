using UnityEngine;

public static class Vector3Extensions
{
    public static Vector3 WithX(this Vector3 value, float x)
    {
        value.x = x;
        return value;
    }

    public static Vector3 WithY(this Vector3 value, float y)
    {
        value.y = y;
        return value;
    }

    public static Vector3 WithZ(this Vector3 value, float z)
    {
        value.z = z;
        return value;
    }

    public static Vector3 AddX(this Vector3 value, float x)
    {
        value.x += x;
        return value;
    }

    public static Vector3 AddY(this Vector3 value, float y)
    {
        value.y += y;
        return value;
    }

    public static Vector3 AddZ(this Vector3 value, float z)
    {
        value.z += z;
        return value;
    }

    public static Vector3 ToVector3(this Vector3Int a)
    {
        return a;
    }

    public static Vector3 ToVector3XZ(this Vector2 a)
    {
        return new Vector3(a.x, 0, a.y);
    }
    
    public static Vector3 ToVector3XY(this Vector2 a)
    {
        return new Vector3(a.x, a.y, 0);
    }
}