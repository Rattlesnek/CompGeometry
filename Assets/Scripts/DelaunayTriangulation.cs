using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DelaunayTriangulation
{
    public static List<Line> Triangulate(List<Vector2> points)
    {
        if (points.Count <= 2)
        {
            return new List<Line>();
        }

        var finalEdges = new List<Line>();
        var edgeQueue = new List<Line>();

        var p1 = FindMinY(points);
        var p2 = ClosestPoint(p1, points);

        var startEdge = new Line(p1, p2);
        var minP = GetPointMinDelaunayDistance(startEdge, points);
        if (!minP.HasValue)
        {
            var twinEdge = startEdge.CreateTwin();
            minP = GetPointMinDelaunayDistance(twinEdge, points);

            finalEdges.Add(twinEdge);
            var (e1, e2) = CreateTriangle(twinEdge, minP.Value);
            edgeQueue.Add(e1);
            edgeQueue.Add(e2);
        }
        else
        {
            edgeQueue.Add(startEdge);
            var (e1, e2) = CreateTriangle(startEdge, minP.Value);
            edgeQueue.Add(e1);
            edgeQueue.Add(e2);
        }

        while (edgeQueue.Count != 0)
        {
            var usedEdge = edgeQueue[0];
            edgeQueue.RemoveAt(0);

            var newEdge = usedEdge.CreateTwin();

            minP = GetPointMinDelaunayDistance(newEdge, points);
            if (minP.HasValue)
            {
                var (e1, e2) = CreateTriangle(newEdge, minP.Value);

                // Check whether e1 and e2 are not already computed
                AddEdgeToLists(e1, finalEdges, edgeQueue);
                AddEdgeToLists(e2, finalEdges, edgeQueue);
            }

            finalEdges.Add(newEdge);
        }

        return finalEdges;
    }

    private static void AddEdgeToLists(Line newEdge, List<Line> finalEdges, List<Line> edgeQueue)
    {
        int idx = edgeQueue.FindIndex(x => x.From == newEdge.To && x.To == newEdge.From);
        if (idx == -1)
        {
            edgeQueue.Add(newEdge);
        }
        else
        {
            edgeQueue.RemoveAt(idx);
            finalEdges.Add(newEdge);
        }
    }


    private static (Line, Line) CreateTriangle(Line edge, Vector2 newPoint)
    {
        var e1 = new Line(edge.To, newPoint);
        var e2 = new Line(newPoint, edge.From);
        return (e1, e2);
    }

    private static Vector2? GetPointMinDelaunayDistance(Line edge, List<Vector2> points)
    {
        Vector2? minP = null;
        float minDist = float.MaxValue;
        foreach (var point in GetPointsToLeft(edge, points))
        {
            var dist = DelaunayDistance(edge, point);
            if (dist < minDist)
            {
                minP = point;
                minDist = dist;
            }
        }
        return minP;
    }

    private static IEnumerable<Vector2> GetPointsToLeft(Line edge, List<Vector2> points)
    {
        return points.Where(x => IsToLeft(edge, x));
    }

    private static float DelaunayDistance(Line edge, Vector2 point)
    {
        var (center, radius) = CircumscribedCircle(edge, point);

        var a = edge.From - point;
        var b = edge.To - point;

        var angle = Vector2.Angle(a, b);
        return (angle > 90) ? -radius : radius;
    }

    private static (Vector2 center, float radius) CircumscribedCircle(Line edge, Vector2 point)
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

    private static Vector2 ClosestPoint(Vector2 refPoint, List<Vector2> points)
    {
        Vector2? closestPoint = null;
        var closestDist = float.MaxValue;
        foreach (var otherPoint in points)
        {
            if (otherPoint == refPoint)
            {
                continue;
            }

            var dist = Vector2.Distance(refPoint, otherPoint);
            if (dist < closestDist)
            {
                closestPoint = otherPoint;
                closestDist = dist;
            }
        }
        return closestPoint.Value;
    }

    private static Vector2 FindMinY(IEnumerable<Vector2> points)
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

    private static bool IsToLeft(Line edge, Vector2 point)
    {
        var v1 = edge.To - edge.From;
        var v2 = point - edge.From;
        float res = v1.x * v2.y - v1.y * v2.x;
        return res > 0f;
    }
}
