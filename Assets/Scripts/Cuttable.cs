using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cuttable : MonoBehaviour
{
    public Polygon polygon;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 scale = transform.localScale / 2;
        Vector3 position = transform.position;

        List<Vector2> vertices = new List<Vector2>();
        vertices.Add(new Vector2(position.x - scale.x, position.y - scale.y));
        vertices.Add(new Vector2(position.x - scale.x, position.y + scale.y));
        vertices.Add(new Vector2(position.x + scale.x, position.y + scale.y));
        vertices.Add(new Vector2(position.x + scale.x, position.y - scale.y));
        polygon = new Polygon(vertices);

        Debug.Log(polygon.Area());
    }

    public void cut(Vector2 cutOrigin, Vector2 cutEnd) {
        List<Vector2[]> edges = polygon.getEdges();
        Vector2[] segment = new Vector2[2] {cutOrigin, cutEnd};

        List<Vector2> intersectionPoints = new List<Vector2>();
        foreach (Vector2[] edge in edges)
            if (Utils.hasIntersection(segment, edge))
                intersectionPoints.Add(Utils.getIntersectionPoint(segment, edge));

        float angleOfRotation = Mathf.PI / 2f - Utils.getSegmentAngle(intersectionPoints.ToArray());
        Vector2[] rotatedSegment = new Vector2[2] {
            Utils.rotatePointByAngle(intersectionPoints[0], angleOfRotation),
            Utils.rotatePointByAngle(intersectionPoints[1], angleOfRotation)
        };

        List<Vector2> vertices = polygon.getVertices();
        List<Vector2> backVertices = new List<Vector2>();
        List<Vector2> frontVertices = new List<Vector2>();
        frontVertices.Add(intersectionPoints[0]);
        frontVertices.Add(intersectionPoints[1]);

        foreach (Vector2 vertice in vertices) {
            Vector2 rotatedVertice = Utils.rotatePointByAngle(vertice, angleOfRotation);
            int relativePosition = Utils.getPointRelativePosition(rotatedVertice, rotatedSegment);
            if (relativePosition == 0)
                backVertices.Add(vertice);
            else
                frontVertices.Add(vertice);
        }

        backVertices.Add(intersectionPoints[1]);
        backVertices.Add(intersectionPoints[0]);

        Polygon backPolygon = new Polygon(backVertices);
        Polygon frontPolygon = new Polygon(frontVertices);

        Debug.Log(backPolygon.Area() + frontPolygon.Area());
    }
}
