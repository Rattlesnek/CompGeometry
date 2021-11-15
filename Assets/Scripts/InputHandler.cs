using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public enum GeometryType
{
    GiftWraping,
    GrahamScan,
    Polygon,
    TriangHull,
    TriangPolygon,
    KDTree,
    DelaunayTriang,
    Off
}


public class InputHandler : MonoBehaviour
{
    // Public
    public GameObject PointPrefab;


    // Events
    public delegate void GeometryTypeChangedEvent(GeometrySelectorEventArgs args);

    public event GeometryTypeChangedEvent OnGeometryTypeChanged;


    // Private
    private List<Transform> points = new List<Transform>();

    private GeometryType geometryType = GeometryType.Off;

    private Vector3 pointRadius;

    private Transform clickedPoint = null;


    // Start is called before the first frame update
    public void Start()
    {
        pointRadius = PointPrefab.transform.localScale / 2f;
    }

    // Update is called once per frame
    public void Update()
    {
        var pointsChanged = false;
        var newGeometryType = geometryType;
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        // Mouse
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            // Select point
            clickedPoint = GetClickedPoint(mousePos);
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            if (clickedPoint == null)
            {
                // Add point
                var point = CreatePoint(mousePos);
                points.Add(point);
            }
            else
            {
                // Remove point
                points.Remove(clickedPoint);
                Destroy(clickedPoint.gameObject);
            }
            pointsChanged = true;
        }

        if (Input.GetMouseButton(1) && clickedPoint != null)
        {
            // Drag point
            clickedPoint.position = mousePos;
            pointsChanged = true;
        }

        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            // Deselect point
            clickedPoint = null;
        }


        // Keyboard
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearAllPoints();
            newGeometryType = GeometryType.Off;
            pointsChanged = true;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            var maxSize = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0f));
            for (int i = 0; i < 5; i++)
            {
                var x = UnityEngine.Random.Range(-maxSize.x, maxSize.x);
                var y = UnityEngine.Random.Range(-maxSize.y, maxSize.y);
                var point = CreatePoint(new Vector3(x, y, 0f));
                points.Add(point);
            }
            pointsChanged = true;
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            newGeometryType = swapGeometryTypes(GeometryType.GiftWraping);
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            newGeometryType = swapGeometryTypes(GeometryType.GrahamScan);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            newGeometryType = swapGeometryTypes(GeometryType.TriangPolygon);
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            newGeometryType = swapGeometryTypes(GeometryType.TriangHull);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            newGeometryType = swapGeometryTypes(GeometryType.KDTree);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            newGeometryType = swapGeometryTypes(GeometryType.DelaunayTriang);
        }

        if (pointsChanged || newGeometryType != geometryType)
        {
            geometryType = newGeometryType;
            OnGeometryTypeChanged?.Invoke(new GeometrySelectorEventArgs(pointsChanged, geometryType));
        }
    }

    public List<Vector2> GetPoints()
    {
        return points.Select(t => new Vector2(t.position.x, t.position.y)).ToList();
    }

    private GeometryType swapGeometryTypes(GeometryType definition)
    {
        return (geometryType != definition) ? definition : GeometryType.Off;
    } 

    private Transform CreatePoint(Vector3 position)
    {
        var pointObj = Instantiate(
            PointPrefab,
            position, 
            Quaternion.identity, 
            transform);
        return pointObj.transform;
    }

    private Transform GetClickedPoint(Vector3 mousePosition)
    {
        var clickedPoint = points.SingleOrDefault(x =>
        {
            var diff = x.position - mousePosition;
            return Mathf.Abs(diff.x) <= pointRadius.x && Mathf.Abs(diff.y) <= pointRadius.y;
        });
        return clickedPoint;
    }

    private void ClearAllPoints()
    {
        points.ForEach(x => Destroy(x.gameObject));
        points.Clear();
    }
}


public class GeometrySelectorEventArgs : EventArgs
{
    public bool PointsChanged { get; set; }

    public GeometryType GeometryType { get; set; }

    public GeometrySelectorEventArgs(bool pointsChanged, GeometryType geometryType)
    {
        PointsChanged = pointsChanged;
        GeometryType = geometryType;
    }
}
