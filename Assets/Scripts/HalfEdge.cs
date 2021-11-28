using UnityEngine;

public class HalfEdge : Line
{
    // Help Edges are not rendered (one of the two twin edges is labeled as help edge)
    public bool IsHelpEdge = false;
    
    public HalfEdge Twin = null;

    public HalfEdge Next = null;

    public HalfEdge(Vector2 from, Vector2 to) 
        : base(from, to)
    {
    }

    public HalfEdge CreateTwin()
    {
        var twinEdge = new HalfEdge(To, From);
        return twinEdge;
    }
}
