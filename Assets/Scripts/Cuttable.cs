using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cuttable : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private PolygonCollider2D polygonCollider;

    public Vector2 scale;
    public List<Vector2> polygonVertices;
    public Polygon polygon;
    public bool canCut = true;

    // Start is called before the first frame update
    void Start()
    {
        meshFilter = gameObject.GetComponent<MeshFilter>();
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        polygonCollider = gameObject.GetComponent<PolygonCollider2D>();

        polygon = new Polygon(polygonVertices);
        initPolygon();
    }

    public void initPolygon() {
        polygonCollider.pathCount = 1;
        polygonCollider.points = polygon.getVertices().ToArray();
        polygonCollider.isTrigger = true;

        Mesh polygonMesh = polygon.getMesh();
        meshFilter.mesh = polygonMesh;

        GameObject childTrigger = transform.GetChild(0).gameObject;
        childTrigger.GetComponent<PolygonCollider2D>().pathCount = 1;
        childTrigger.GetComponent<PolygonCollider2D>().points = polygon.getVertices().ToArray();
    }

    public void cut(Vector2 cutOrigin, Vector2 cutEnd) {
        if (!canCut) return;

        List<Vector2[]> edges = polygon.getEdges();
        Vector2[] segment = new Vector2[2] {cutOrigin, cutEnd};

        List<Vector2> intersectionPoints = new List<Vector2>();
        foreach (Vector2[] edge in edges)
            if (Utils.hasIntersection(edge, segment))
                intersectionPoints.Add(Utils.getIntersectionPoint(edge, segment));
            
        // if (intersectionPoints[0].y > intersectionPoints[1].y) {
        //     Vector2 temp = intersectionPoints[0];
        //     intersectionPoints[0] = intersectionPoints[1];
        //     intersectionPoints[1] = temp;
        // }

        float segmentAngle = Utils.getSegmentAngle(intersectionPoints.ToArray());
        if (segmentAngle > Mathf.PI / 2f)
            segmentAngle = Mathf.PI - segmentAngle;
        float angleOfRotation = segmentAngle;
        Vector2[] rotatedSegment = new Vector2[2] {
            Utils.rotatePointByAngle(intersectionPoints[0], angleOfRotation),
            Utils.rotatePointByAngle(intersectionPoints[1], angleOfRotation)
        };

        List<Vector2> vertices = polygon.getVertices();
        List<Vector2> backVertices = new List<Vector2>();
        List<Vector2> frontVertices = new List<Vector2>();

        int start, end;

        if (intersectionPoints[1].x > intersectionPoints[0].x) {
            if (intersectionPoints[1].y > intersectionPoints[0].y) {
                frontVertices.Add(intersectionPoints[1]);
                backVertices.Add(intersectionPoints[0]);
                start = 0;
                end = vertices.Count;
            } else {
                frontVertices.Add(intersectionPoints[0]);
                backVertices.Add(intersectionPoints[1]);
                start = vertices.Count - 1;
                end = -1;
            }
        } else {
            if (intersectionPoints[1].y > intersectionPoints[0].y) {
                frontVertices.Add(intersectionPoints[0]);
                backVertices.Add(intersectionPoints[1]);
                start = vertices.Count - 1;
                end = -1;
            } else {
                frontVertices.Add(intersectionPoints[1]);
                backVertices.Add(intersectionPoints[0]);
                start = 0;
                end = vertices.Count;
            }
        }
        
        for (int i = start; i != end; i += (start < end ? 1 : -1)) {
            Vector2 vertice = vertices[i];
            Vector2 rotatedVertice = Utils.rotatePointByAngle(vertice, angleOfRotation);
            int relativePosition = Utils.getPointRelativePosition(rotatedVertice, rotatedSegment);
            if (relativePosition == 0)
                backVertices.Add(vertice);
            else
                frontVertices.Add(vertice);
        }

        if (intersectionPoints[1].x > intersectionPoints[0].x) {
            if (intersectionPoints[1].y > intersectionPoints[0].y) {
                frontVertices.Add(intersectionPoints[0]);
                backVertices.Add(intersectionPoints[1]);
            } else {
                frontVertices.Add(intersectionPoints[1]);
                backVertices.Add(intersectionPoints[0]);
            }
        } else {
            if (intersectionPoints[1].y > intersectionPoints[0].y) {
                frontVertices.Add(intersectionPoints[1]);
                backVertices.Add(intersectionPoints[0]);
            } else {
                frontVertices.Add(intersectionPoints[0]);
                backVertices.Add(intersectionPoints[1]);
            }
        }

        Polygon backPolygon = new Polygon(backVertices);
        Polygon frontPolygon = new Polygon(frontVertices);

        Polygon minorPolygon;

        if (backPolygon.Area() < frontPolygon.Area()) {
            polygon = frontPolygon;
            minorPolygon = backPolygon;
        }
        else {
            polygon = backPolygon;
            minorPolygon = frontPolygon;
        }

        canCut = false;
        transform.GetChild(0).gameObject.layer = 0;

        // Create new polygon
        GameObject minorObject = Instantiate(gameObject);
        Rigidbody2D minorBody = minorObject.AddComponent<Rigidbody2D>();
        minorBody.mass = minorPolygon.Area();

        Cuttable minorCuttable = minorObject.GetComponent<Cuttable>();
        minorCuttable.polygonVertices = minorPolygon.getVertices();

        initPolygon();
        minorCuttable.initPolygon();
    }
}
