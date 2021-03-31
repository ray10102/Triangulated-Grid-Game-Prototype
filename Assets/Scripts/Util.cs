using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class Util
{
    public static Vector3 ElevationToVec3(int elevation) {
        return Vector3.up * elevation * GridMetrics.elevationStep;
    }

    public static float FindIntercept2D(float slope, Vector2 point) {
        return point.y - slope * point.x;
    }
    
    public static Vector2 XZ(Vector3 vec3) {
        return new Vector2(vec3.x, vec3.z);
    }

    public static Vector2 FindIntersectionOfLines(float m1, float m2, float b1, float b2) {
        if (m1 == m2) {
            throw new ArgumentException();
        }
        float x = (b2 - b1) / (m1 - m2);
        float y = m1 * x + b1;
        return new Vector2(x, y);
    }
}
