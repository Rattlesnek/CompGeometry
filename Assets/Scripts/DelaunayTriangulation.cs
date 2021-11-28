using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DelaunayTriangulation
{
    public static List<Line> TriangulateLines(List<Vector2> points)
    {
        var edgeList = TriangulateEdgeList(points);
        return new List<Line>(edgeList.Where(x => !x.IsHelpEdge));
    }

    public static List<HalfEdge> TriangulateEdgeList(List<Vector2> points)
    {
        if (points.Count <= 2)
        {
            return new List<HalfEdge>();
        }

        var finalEdges = new List<HalfEdge>();
        var edgeQueue = new List<HalfEdge>();

        var p1 = Utils.FindMinY(points);
        var p2 = ClosestPoint(p1, points);

        var startEdge = new HalfEdge(p1, p2);
        var minP = GetPointMinDelaunayDistance(startEdge, points);
        if (!minP.HasValue)
        {
            var twinEdge = startEdge.CreateTwin();
            minP = GetPointMinDelaunayDistance(twinEdge, points);

            var (e1, e2) = CreateTriangle(twinEdge, minP.Value);
            finalEdges.Add(twinEdge);
            finalEdges.Add(e1);
            finalEdges.Add(e2);
            edgeQueue.Add(e1);
            edgeQueue.Add(e2);
        }
        else
        {
            var (e1, e2) = CreateTriangle(startEdge, minP.Value);
            finalEdges.Add(startEdge);
            finalEdges.Add(e1);
            finalEdges.Add(e2);
            edgeQueue.Add(startEdge);
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
                // newEdge is not on convex hull -- therefore bind with twin
                newEdge.Twin = usedEdge;
                newEdge.IsHelpEdge = true;
                usedEdge.Twin = newEdge;

                var (e1, e2) = CreateTriangle(newEdge, minP.Value);

                finalEdges.Add(newEdge);
                // Check whether e1 and e2 are not already computed
                AddEdgeToLists(e1, finalEdges, edgeQueue);
                AddEdgeToLists(e2, finalEdges, edgeQueue);
            }
        }

        return finalEdges;
    }

    private static void AddEdgeToLists(HalfEdge newEdge, List<HalfEdge> finalEdges, List<HalfEdge> edgeQueue)
    {
        int idx = edgeQueue.FindIndex(x => x.From == newEdge.To && x.To == newEdge.From);
        if (idx == -1)
        {
            edgeQueue.Add(newEdge);
        }
        else
        {
            var twinEdge = edgeQueue[idx];
            newEdge.Twin = twinEdge;
            newEdge.IsHelpEdge = true;
            twinEdge.Twin = newEdge;
            edgeQueue.RemoveAt(idx);
        }
        finalEdges.Add(newEdge);
    }

    private static (HalfEdge, HalfEdge) CreateTriangle(HalfEdge edge, Vector2 newPoint)
    {
        var e1 = new HalfEdge(edge.To, newPoint);
        var e2 = new HalfEdge(newPoint, edge.From);
        edge.Next = e1;
        e1.Next = e2;
        e2.Next = edge;
        return (e1, e2);
    }

    private static Vector2? GetPointMinDelaunayDistance(HalfEdge edge, List<Vector2> points)
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

    private static IEnumerable<Vector2> GetPointsToLeft(HalfEdge edge, List<Vector2> points)
    {
        return points.Where(x => IsToLeft(edge, x));
    }

    private static float DelaunayDistance(HalfEdge edge, Vector2 point)
    {
        var (_, radius) = Utils.CircumscribedCircle(edge, point);

        var a = edge.From - point;
        var b = edge.To - point;

        var angle = Vector2.Angle(a, b);
        return (angle > 90) ? -radius : radius;
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

    private static bool IsToLeft(HalfEdge edge, Vector2 point)
    {
        var v1 = edge.To - edge.From;
        var v2 = point - edge.From;
        float res = v1.x * v2.y - v1.y * v2.x;
        return res > 0f;
    }
}
