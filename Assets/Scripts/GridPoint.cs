using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A GridPoint is a container for all Points with the same coordinates. 
/// Points are stored in order from lowest to highest.
/// </summary>
public class GridPoint
{
    private List<Point> points;

    public Point GetPoint(int index) {
        if (index > points.Count - 1) {
            throw new ArgumentOutOfRangeException();
        }
        return points[index];
    }
}
