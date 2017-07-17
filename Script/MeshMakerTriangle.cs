using UnityEngine;
using System;

[Serializable]
public class MeshMakerTriangle : MeshMakerBase
{
    private int _id;
    public int ID
    {
        get{
            return _id;
        }
        set {
            _id = value;
        }
    }

    private MeshMakerVertex _vertex1;
    public MeshMakerVertex Vertex1
    {
        get {
            return _vertex1;
        }
        set {
            _vertex1 = value;
        }
    }

    private MeshMakerVertex _vertex2;
    public MeshMakerVertex Vertex2
    {
        get
        {
            return _vertex2;
        }
        set
        {
            _vertex2 = value;
        }
    }

    private MeshMakerVertex _vertex3;
    public MeshMakerVertex Vertex3
    {
        get
        {
            return _vertex3;
        }
        set
        {
            _vertex3 = value;
        }
    }

    private MeshMakerEdge _edge1;
    public MeshMakerEdge Edge1
    {
        get
        {
            return _edge1;
        }
        set
        {
            _edge1 = value;
        }
    }

    private MeshMakerEdge _edge2;
    public MeshMakerEdge Edge2
    {
        get
        {
            return _edge2;
        }
        set
        {
            _edge2 = value;
        }
    }

    private MeshMakerEdge _edge3;
    public MeshMakerEdge Edge3
    {
        get
        {
            return _edge3;
        }
        set
        {
            _edge3 = value;
        }
    }

    public MeshMakerTriangle(int id, MeshMakerVertex vertex1, MeshMakerVertex vertex2, MeshMakerVertex vertex3)
    {
        _id = id;
        Vertex1 = vertex1;
        Vertex2 = vertex2;
        Vertex3 = vertex3;
        RefreshEdge();
    }

    public Vector3 TriangleCenter()
    {
        Vector3 triangle = new Vector3((Vertex1.Vertex.x + Vertex2.Vertex.x + Vertex3.Vertex.x) / 3,
           (Vertex1.Vertex.y + Vertex2.Vertex.y + Vertex3.Vertex.y) / 3,
           (Vertex1.Vertex.z + Vertex2.Vertex.z + Vertex3.Vertex.z) / 3);

        return triangle;
    }

    public void RefreshEdge()
    {
        Edge1 = new MeshMakerEdge(Vertex1, Vertex2);
        Edge2 = new MeshMakerEdge(Vertex2, Vertex3);
        Edge3 = new MeshMakerEdge(Vertex3, Vertex1);
    }
}
