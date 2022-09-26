using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Utils {
    public static bool isEqual(float a, float b) {
        if (Mathf.Abs(a - b) < Mathf.Epsilon) return true;
        else return false;
    }

    public static bool isBetween(float x, float a, float b) {
        return (x >= a && x <= b) || (x >= b && x <= b);
    }

    public static Vector3 GetNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        // Find vectors corresponding to two of the sides of the triangle.
        Vector3 side1 = b - a;
        Vector3 side2 = c - a;

        // Cross the vectors to get a perpendicular vector, then normalize it.
        return Vector3.Cross(side1, side2).normalized;
    }

    public static bool isColinear(Vector2 P, Vector2 A, Vector2 B) {
        return isEqual( (P.y - A.y) / (P.x - A.x) , (B.y - A.y) / (B.x - A.x) );
    }

    public static bool isPointBetween(Vector2 P, Vector2 A, Vector2 B) {
        return isColinear(P, A, B) && isBetween(P.x, A.x, B.x) && isBetween(P.y, A.y, B.y);
    }

    public static int getPointRelativePosition(Vector2 P, Vector2[] line) {

        // return 0 if point is before the line
        // return 1 if point is after the line
        // return 2 if point is on line
        // return 3 when has no intersection

        Vector2 minPoint, maxPoint;
        if (line[0].y > line[1].y) {
            maxPoint = line[0];
            minPoint = line[1];
        } else {
            maxPoint = line[1];
            minPoint = line[0];
        }

        P.y += (isEqual(P.y, maxPoint.y) || isEqual(P.y, minPoint.y)) ? Mathf.Epsilon : 0f;

        if (isEqual(maxPoint.x, minPoint.x)) {
            if (P.y > maxPoint.y || P.y < minPoint.y) return 3;

            if (P.x < maxPoint.x) return 0;
            else if(P.x > maxPoint.x) return 1;
            else return 2;
        }

        if (isEqual(maxPoint.y, minPoint.y)) {
            // if (P.x > maxPoint.x || P.x < minPoint.x) return 3;

            if (P.y < maxPoint.y) return 0;
            else if (P.y > maxPoint.y) return 1;
            else return 2;
        }

        float x_projection = maxPoint.x - (maxPoint.y - P.y) / (maxPoint.y - minPoint.y) * (maxPoint.x - minPoint.x);

        if (P.x < x_projection) return 0;
        else if(P.x > x_projection) return 1;
        else return 2;
    }

    public static bool hasIntersection(Vector2[] r, Vector2[] s) {
        int firstPointRelativePosition = getPointRelativePosition(r[0], s);
        int secondPointRelativePosition = getPointRelativePosition(r[1], s);

        return firstPointRelativePosition != secondPointRelativePosition;
    }

    public static Vector2 getIntersectionPoint(Vector2[] r, Vector2[] s) {
        float a_r = (r[1].y - r[0].y) / (r[1].x - r[0].x);
        float b_r = -1;
        float c_r = (r[0].y - r[0].x * a_r);

        float a_s = (s[1].y - s[0].y) / (s[1].x - s[0].x);
        float b_s = -1;
        float c_s = (s[0].y - s[0].x * a_s);

        Vector2 intersectionPoint = new Vector2();

        intersectionPoint.x = (b_r * c_s - b_s * c_r) / (a_r * b_s - a_s * b_r);
        intersectionPoint.y = (c_r * a_s - c_s * a_r) / (a_r * b_s - a_s * b_r);

        return intersectionPoint;
    }

    public static float getSegmentAngle(Vector2[] segment) {
        return Mathf.Atan2(segment[1].y - segment[0].y, segment[1].x - segment[0].x);
    }

    public static Vector2 rotatePointByAngle(Vector2 P, float theta) {
        Vector2 P_rotate = new Vector2();
        P_rotate.x = P.x * Mathf.Cos(theta) - P.y * Mathf.Sin(theta);
        P_rotate.y = P.x * Mathf.Sin(theta) + P.y * Mathf.Cos(theta);

        return P_rotate;
    }
}