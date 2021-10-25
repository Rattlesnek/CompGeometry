using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ConvexHull
{
    public static List<Vector2> GiftWrapping(IReadOnlyList<Vector2> points)
    {
        var availablePoints = points.ToList();
        if (availablePoints.Count <= 2)
        {
            return availablePoints;
        }

        var minY = FindMinY(availablePoints);
        var curr = minY;
        var prev = minY + new Vector2(1f, 0f); // Auxiliary point
        var next = Vector2.zero; // Default value of the next point will be replaced because there must be more than 1 point

        var convexHull = new List<Vector2>();
        do
        {
            var dir = curr - prev;

            float minAng = 360f;
            foreach (var point in availablePoints)
            {
                if (curr == point)
                {
                    continue;
                }
                var nextDir = point - curr;
                var ang = Vector2.Angle(dir, nextDir);
                if (ang < minAng)
                {
                    minAng = ang;
                    next = point;
                }
            }

            convexHull.Add(next);
            availablePoints.Remove(next);

            prev = curr;
            curr = next;
        }
        while (next != minY);

        return convexHull;
    }

    public static List<Vector2> GrahamScan(IReadOnlyList<Vector2> points)
    {
        if (points.Count <= 2)
        {
            return points.ToList();
        }
        
        var minY = FindMinY(points);

        var convexHull = new Stack<Vector2>();
        convexHull.Push(minY);
        var availablePoints = points.Except(convexHull).ToList();

        var compareDir = new Vector2(1f, 0f);

        var anglesWithPoints = new Dictionary<float, Vector2>();
        foreach (var point in availablePoints)
        {
            var vect = point - minY;
            var angle = Vector3.Angle(compareDir, vect);
            if (anglesWithPoints.ContainsKey(angle))
            {
                var included = anglesWithPoints[angle];
                if ((included - minY).magnitude < vect.magnitude)
                {
                    anglesWithPoints[angle] = point;
                }
            }
            else
            {
                anglesWithPoints.Add(angle, point);
            }
        }

        var keys = anglesWithPoints.Keys.ToList();
        keys.Sort();

        var first = anglesWithPoints[keys[0]];
        convexHull.Push(first);

        int i = 1;
        while (i < keys.Count)
        {
            var next = anglesWithPoints[keys[i]];
            var top = convexHull.Pop();

            bool isLeft = IsLeftTurn(convexHull.Peek(), top, next);
            if (isLeft)
            {
                convexHull.Push(top);
                convexHull.Push(next);
                i++;
            }
        }

        return convexHull.ToList();
    }

    private static bool IsLeftTurn(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        var v1 = p2 - p1;
        var v2 = p3 - p1;
        float res = v1.x * v2.y - v1.y * v2.x;
        return res >= 0f;
    }

    private static Vector2 FindMinY(IEnumerable<Vector2> points)
    {
        var minY = points.First();
        foreach (var point in points)
        {
            if (point.y < minY.y)
            {
                minY = point;
            }
        }
        return minY;
    }
}
