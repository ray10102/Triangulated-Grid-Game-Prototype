using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A GridPoint is a container for all Points with the same coordinates. 
/// Points are stored in order from lowest to highest.
/// </summary>
public class GridPoint
{
    public VertexCoordinates coordinates;
    private List<Point> points;

    public GridPoint(Point point, VertexCoordinates coordinates) {
        this.coordinates = coordinates;
        points = new List<Point>();
        point.gridPoint = this;
        points.Add(point);
    }

    public Point GetPoint(int index) {
        if (index > points.Count - 1) {
            throw new ArgumentOutOfRangeException();
        }
        return points[index];
    }

    public Point GetPointAtY(float y) {
        foreach(Point point in points) {
            float marginOfError = 0.1f; // To account for float precision
            Vector2 extents = point.ElevationExtents;
            if (extents.x * GridMetrics.elevationStep - marginOfError <= y && y <= extents.y * GridMetrics.elevationStep + marginOfError) {
                return point;
            }
        }
        throw new ArgumentException("Couldn't get point for " + coordinates.ToString() + " at y " + y.ToString());
    }
}
