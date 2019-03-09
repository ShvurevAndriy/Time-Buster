using UnityEngine;

public static class JumpPhysics {
    public const float g = 9.8f;

    public static Vector2 CalculateNextJumpVelocities(float heightToJump, float g, float angel) {
        float velocity = HeightToJumpForce(heightToJump, g, angel);
        return new Vector2(velocity * Mathf.Cos(angel * Mathf.Deg2Rad), velocity * Mathf.Sin(angel * Mathf.Deg2Rad));
    }

    private static float HeightToJumpForce(float height, float g, float angel) {
        return Mathf.Sqrt(height * 2 * g) / Mathf.Sin(Mathf.Deg2Rad * angel);
    }

    public static float CalculateNextJumpHeight(float nextHeightBoost, float minJumpHeight, float maxJumpHeight) {
        return Mathf.Clamp(nextHeightBoost * 2, minJumpHeight, maxJumpHeight);
    }

    public static float CalculateBoostJumpHeight(float topPosition, float touchPosition) {
        float boostHeight = topPosition - touchPosition;
        return boostHeight < 0 ? 0 : boostHeight;
    }

    public static Vector3 CalculatePositionAtTime(float yVelocity, float currentAngel, float radius, float time, float g, Vector3 startPos) {
        return new Vector3(
            Mathf.Cos(Mathf.Deg2Rad * currentAngel) * radius,
            g * time * time * 0.5f + yVelocity * time + startPos.y,
            Mathf.Sin(Mathf.Deg2Rad * currentAngel) * radius);
    }

    public static float AngularToLinearVelocity(float angularVelocity, float radius) {
        return angularVelocity * radius / Mathf.Rad2Deg;
    }
    public static float LinearToAngularVelocity(float linearVelocity, float radius) {
        return linearVelocity * Mathf.Rad2Deg / radius;
    }
}
