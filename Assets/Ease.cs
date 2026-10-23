using UnityEngine;

// If your Lecture 05 project already has an Ease.cs, delete this file and use yours.
// It must contain a static OutBack(float t).
public static class Ease
{
    public static float OutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        t = Mathf.Clamp01(t);
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}