using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class LineHandler : MonoBehaviour
{
    public GameObject LinePrefab;

    public InputHandler inputHandler;

    private List<Transform> lineTransforms = new List<Transform>();

    // Start is called before the first frame update
    void Start()
    {
        inputHandler.OnGeometryTypeChanged += OnGeometryTypeChanged;
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnGeometryTypeChanged(GeometrySelectorEventArgs args)
    {
        if (args.PointsChanged)
        {
            ClearAllLines();
        }
        
        switch (args.GeometryType)
        {
            case GeometryType.GiftWraping:
                var giftWrap = ConvexHull.GiftWrapping(inputHandler.GetPoints());
                RenderLinesBtwPoints(giftWrap);
                break;

            case GeometryType.GrahamScan:
                var grahamScan = ConvexHull.GrahamScan(inputHandler.GetPoints());
                RenderLinesBtwPoints(grahamScan);
                break;

            case GeometryType.Polygon:
                RenderLinesBtwPoints(inputHandler.GetPoints());
                break;

            case GeometryType.TriangHull:
                var hull = ConvexHull.GrahamScan(inputHandler.GetPoints());
                RenderLinesBtwPoints(hull);
                var hullDiag = Triangulation.MonotonePolygonTriangulation(hull);
                RenderLines(hullDiag);
                break;

            case GeometryType.TriangPolygon:
                var points = inputHandler.GetPoints();
                RenderLinesBtwPoints(points);
                var polyDiag = Triangulation.MonotonePolygonTriangulation(points);
                RenderLines(polyDiag);
                break;

            case GeometryType.KDTree:
                var tree = KDTree.BuildTree(inputHandler.GetPoints());
                RenderKDTree(tree, new Vector2(-10, 10), new Vector2(10, -10));
                break;

            case GeometryType.DelaunayTriang:
                var delTriangLines = DelaunayTriangulation.Triangulate(inputHandler.GetPoints());
                RenderLines(delTriangLines);
                break;

            case GeometryType.Off:
            default:
                ClearAllLines();
                break;
        }
    }


    private Transform CreateLine(Vector3 start, Vector3 end)
    {
        var line = Instantiate(LinePrefab, transform);
        var lr = line.GetComponent<LineRenderer>();
        lr.SetPositions(new Vector3[] { start, end });
        return line.transform;
    }

    private void RenderLines(List<Line> lines)
    {
        foreach (var line in lines)
        {
            var lineT = CreateLine(line.From, line.To);
            lineTransforms.Add(lineT);
        }
    }

    private void RenderLinesBtwPoints(IList<Vector2> points)
    {
        if (points.Count <= 1)
        {
            return;
        }

        var p0 = points.Last();
        for (int i = 0; i < points.Count; i++)
        {
            var p1 = points[i];
            var line = CreateLine(p0, p1);
            lineTransforms.Add(line);
            p0 = p1;
        }
    }

    private void RenderKDTree(KDTree tree, Vector2 leftUp, Vector2 rightDown)
    {
        if (tree == null)
        {
            return;
        }


        if (tree.IsVertical)
        {
            var x = tree.Point.x;
            var newLeftUp = new Vector2(x, leftUp.y);
            var newRightDown = new Vector2(x, rightDown.y);

            var lineT = CreateLine(newLeftUp, newRightDown);
            lineTransforms.Add(lineT);

            RenderKDTree(tree.Left, leftUp, newRightDown);
            RenderKDTree(tree.Right, newLeftUp, rightDown);
        }
        else
        {
            var y = tree.Point.y;
            var newLeftUp = new Vector2(leftUp.x, y);
            var newRightDown = new Vector2(rightDown.x, y);

            var lineT = CreateLine(newLeftUp, newRightDown);
            lineTransforms.Add(lineT);

            RenderKDTree(tree.Left, newLeftUp, rightDown);
            RenderKDTree(tree.Right, leftUp, newRightDown);
        }
    }

    private void ClearAllLines()
    {
        lineTransforms.ForEach(x => Destroy(x.gameObject));
        lineTransforms.Clear();
    }
}
