using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class VoronoiDiagram
{
    public static List<Line> GetLines(List<Vector2> points)
    {
        var edgeList = DelaunayTriangulation.TriangulateEdgeList(points)
            .Where(x => !x.IsHelpEdge);

        var lines = new List<Line>();
        foreach (var edge in edgeList)
        {
            var (center1, _) = Utils.CircumscribedCircle(edge, edge.Next.To);
            if (edge.Twin != null)
            {
                var (center2, _) = Utils.CircumscribedCircle(edge.Twin, edge.Twin.Next.To);
                lines.Add(new Line(center1, center2));
            }
            else
            {
                var vect = edge.To - edge.From;
                var dir = new Vector2(vect.y, -vect.x).normalized * 10f;
                lines.Add(new Line(center1, center1 + dir));
            }
        }
        return lines;
    }
}

