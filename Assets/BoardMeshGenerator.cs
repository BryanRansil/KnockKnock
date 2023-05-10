using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class BoardMeshGenerator : MonoBehaviour
{
    public float Height
    {
        get { return _height; }
        set {
            _height = value;
            UpdateMesh();
        }
    }

    Mesh mesh;

    UnityEngine.Vector3[] _vertices;
    int[] _triangles;
    float _height;
    int _num_vertices_set;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        _vertices = new UnityEngine.Vector3[]
        {
            new UnityEngine.Vector3(0, 0, 0),
            new UnityEngine.Vector3(0, 0, 0),
            new UnityEngine.Vector3(0, 0, 0),
            new UnityEngine.Vector3(0, 0, 0)
        };

        _triangles = new int[]
        {
            0, 1, 2,
            2, 3, 0
        };
    }

    void UpdateMesh()
    {
        Debug.Log("Bryan, Updating Mesh!");
        mesh.Clear();

        mesh.vertices = _vertices;
        mesh.triangles = _triangles;
    }

    public void SetVertex(UnityEngine.Vector3 vertex)
    {
        Debug.Log("Bryan, in Set Vector!");
        if (_num_vertices_set >= _vertices.Length)
        {
            return;
        }

        if (_num_vertices_set == 0)
        {
            Height = vertex.y;
        }

        for (int i = _num_vertices_set; i < _vertices.Length; i++)
        {
            Debug.Log("Bryan, setting vector " + i);
            _vertices[i].x = vertex.x;
            _vertices[i].y = Height;
            _vertices[i].z = vertex.z;
        }
        foreach (UnityEngine.Vector3 vector in _vertices)
        {
            Debug.Log("Bryan, vertex is now " + vector);
        }
        _num_vertices_set++;
        UpdateMesh();
    }
}
