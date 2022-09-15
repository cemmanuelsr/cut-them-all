using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Polygon {
    private List<Vector2> _vertices;
    private List<Vector2[]> _edges;

    public Polygon(List<Vector2> vertices) {
        _vertices = vertices;
        _edges = new List<Vector2[]>();
        for (int i = 0; i < vertices.Count; i++) {
            Vector2[] segment = new Vector2[2];
            segment[0] = vertices[i];
            segment[1] = i == vertices.Count - 1 ? vertices[0] : vertices[i + 1];
            _edges.Add(segment);
        }
    }

    public List<Vector2> getVertices() {
        return _vertices;
    }

    public List<Vector2[]> getEdges() {
        return _edges;
    }

    public float Area() {
        if (_vertices.Count == 3) {
            Vector2 p1 = _vertices[0], p2 = _vertices[1], p3 = _vertices[2];
            return Mathf.Abs(0.5f * (p1.x * p2.y + p2.x * p3.y + p3.x * p1.y - p1.y * p2.x - p2.y * p3.x - p3.y * p1.x));
        }

        float area = 0;
        for (int i = 0; i < _vertices.Count - 2; i++) {
            List<Vector2> vertices = new List<Vector2>();
            vertices.Add(_vertices[0]);
            vertices.Add(_vertices[i + 1]);
            vertices.Add(_vertices[i + 2]);

            Polygon p = new Polygon(vertices);
            area += p.Area();
        }
        return area;
    }
}