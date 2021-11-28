using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Utils
{
    public static Vector2 FindMinY(IEnumerable<Vector2> points)
    {
        var minY = points.First();
        foreach (var point in points)
        {
            if (point.y < minY.y || (point.y == minY.y && point.x < minY.x))
            {
                minY = point;
            }
        }
        return minY;
    }

    public static (Vector2 center, float radius) CircumscribedCircle(HalfEdge edge, Vector2 point)
    {
        var v1 = point - edge.From;
        var v2 = point - edge.To;

        var midP1 = edge.From + v1 / 2f;
        var midP2 = edge.To + v2 / 2f;

        float a1 = v1.x;
        float b1 = v1.y;
        float c1 = a1 * midP1.x + b1 * midP1.y;

        float a2 = v2.x;
        float b2 = v2.y;
        float c2 = a2 * midP2.x + b2 * midP2.y;

        float D = a1 * b2 - a2 * b1;
        float Dx = c1 * b2 - c2 * b1;
        float Dy = a1 * c2 - a2 * c1;

        float x = Dx / D;
        float y = Dy / D;

        var center = new Vector2(x, y);
        var radius = Vector2.Distance(center, point);

        return (center, radius);
    }


}
