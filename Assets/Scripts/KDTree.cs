using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KDTree
{
    public Vector2 Point;

    public bool IsVertical;

    public KDTree Left;

    public KDTree Right;

    public KDTree(Vector2 point, bool isVertical, KDTree left, KDTree right)
    {
        Point = point;
        IsVertical = isVertical;
        Left = left;
        Right = right;
    }

    public static KDTree BuildTree(List<Vector2> points, int depth = 0)
    {
        if (points == null || points.Count == 0)
        {
            return null;
        }

        bool isVertical = depth % 2 == 0;
        if (points.Count == 1)
        {
            return new KDTree(points.First(), isVertical, null, null);
        }

        if (isVertical)
        {
            points.Sort(new XCoordComparer());
        }
        else
        {
            points.Sort(new YCoordComparer());
        }

        int half = points.Count / 2;
        var lesser = points.GetRange(0, half);
        var median = points[half];
        var greater = (half + 1 >= points.Count) ? null : points.GetRange(half + 1, points.Count - half - 1);

        var left = BuildTree(lesser, depth + 1);
        var right = BuildTree(greater, depth + 1);

        return new KDTree(median, isVertical, left, right);
    }
}

public class XCoordComparer : IComparer<Vector2>
{
    public int Compare(Vector2 a, Vector2 b)
    {
        if (a.x == b.x && a.y == b.y)
        {
            return 0;
        }
        if (a.x < b.x || (a.x == b.x && a.y < b.y))
        {
            return -1;
        }
        return 1;
    }
}

public class YCoordComparer : IComparer<Vector2>
{
    public int Compare(Vector2 a, Vector2 b)
    {
        if (a.x == b.x && a.y == b.y)
        {
            return 0;
        }
        if (a.y < b.y || (a.y == b.y && a.x < b.x))
        {
            return -1;
        }
        return 1;
    }
}

