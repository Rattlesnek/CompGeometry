using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Triangulation
{   
    public static List<Line> MonotonePolygonTriangulation(List<Vector2> points)
    {
        if (points.Count <= 3)
        {
            return new List<Line>();
        }

        // Find indexes of min and max points acc. to lexicographic ordering.
        int maxIdx = 0;
        int minIdx = 0;
        int i = 0;
        for (i = 0; i < points.Count; i++)
        {
            var currP = points[i];
            var maxP = points[maxIdx];
            var minP = points[minIdx];
            if (currP.y > maxP.y || (currP.y == maxP.y && currP.x < maxP.x))
            {
                maxIdx = i;
            }
            if (currP.y < minP.y || (currP.y == minP.y && currP.x > minP.x))
            {
                minIdx = i;
            }
        }

        // Decide which path from max is right and which is left.
        var max = points[maxIdx];
        var min = points[minIdx];
        var dirNext = GetValueCyclic(points, maxIdx + 1) - max;
        var dirPrev = GetValueCyclic(points, maxIdx - 1) - max;
        var isNextRightSide = (Vector2.Angle(Vector2.right, dirNext) < Vector2.Angle(Vector2.right, dirPrev));

        var next = new List<(Vector2, bool)>();
        var prev = new List<(Vector2, bool)>();
        int j = maxIdx;
        while (GetValueCyclic(points, j) != min)
        {
            next.Add((GetValueCyclic(points, j), isNextRightSide));
            j++;
        }
        
        j = maxIdx;
        do
        {
            j--;
            prev.Add((GetValueCyclic(points, j), !isNextRightSide));
        }
        while (GetValueCyclic(points, j) != min);

        // Merge left and right paths into single lexicographically ordered list.
        i = 0;
        j = 0;
        var sorted = new List<(Vector2, bool)>();
        while (true)
        {            
            if (i == next.Count)
            {
                sorted.AddRange(prev.GetRange(j, prev.Count - j));
                break;
            }
            if (j == prev.Count)
            {
                sorted.AddRange(next.GetRange(i, next.Count - i));
                break;
            }

            var nextP = next[i].Item1;
            var prevP = prev[j].Item1;
            if (nextP.y > prevP.y || (nextP.y == prevP.y && nextP.x < prevP.x))
            {
                sorted.Add(next[i]);
                i++;
            }
            else
            {
                sorted.Add(prev[j]);
                j++;
            }
        }

        var diagonals = new List<Line>();

        var stack = new Stack<(Vector2, bool)>();
        stack.Push(sorted[0]);
        stack.Push(sorted[1]);

        for (i = 2; i < sorted.Count - 1; i++)
        {
            var current = sorted[i];
            var top = stack.Peek();
            if (current.Item2 != top.Item2)
            {
                var stackCnt = stack.Count;
                for (int k = 0; k < stackCnt - 1; k++)
                {
                    var popped = stack.Pop();
                    diagonals.Add(new Line(popped.Item1, current.Item1));
                }
                stack.Pop(); // Pop last one without adding diagonal.
                stack.Push(sorted[i - 1]);
                stack.Push(current);
            }
            else
            {
                var lastPop = stack.Pop();
                var lastLine = new Line(current.Item1, lastPop.Item1);
                while (stack.Count != 0)
                {
                    var tmp = stack.Pop();
                    bool isLeftTurn = Utils.IsLeftTurn(lastLine.From, lastLine.To, tmp.Item1);
                    if (current.Item2 && isLeftTurn ||
                        !current.Item2 && !isLeftTurn)
                    {
                        lastLine = new Line(current.Item1, tmp.Item1);
                        diagonals.Add(lastLine);
                        lastPop = tmp;
                    }
                    else
                    {
                        stack.Push(tmp);
                        break;
                    }
                }
                stack.Push(lastPop);
                stack.Push(current);                               
            }
        }

        stack.Pop();
        var bottomPoint = sorted[sorted.Count - 1].Item1;
        while (stack.Count != 1)
        {
            var tmp = stack.Pop();
            diagonals.Add(new Line(bottomPoint, tmp.Item1));
        }

        return diagonals;       
    }

    public static Vector2 GetValueCyclic(IList<Vector2> list, int index)
    {
        if (index >= 0)
        {
            return list[index % list.Count];
        }
        else
        {
            return list[list.Count - 1 - (Mathf.Abs(index) - 1) % list.Count];
        }
    }
}
