using OpenTK.Mathematics;
using System;

public class MathUtil {

    private const float MULT_TO_RAD = MathF.PI / 180;

    public static Vector3 toRadians(Vector3 vec) {
        return vec * MULT_TO_RAD;
    }

    public static Vector3 toDegrees(Vector3 vec) {
        return vec / MULT_TO_RAD;
    }

    public static int Mod(int x, int y) {
        return (x % y + y) % y;
    }

}
