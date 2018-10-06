using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastLight : MonoBehaviour
{
    public float lightRadius;
    [Range(0, 360)]
    public float lightAngle;

    public LayerMask obstaclesMask;

    public float meshResolution;
    public MeshFilter lightMashFilter;
    private Mesh lightMesh;
    public int edgeResolveIterations;
    public float edgeDistanceThreshold;

    private void Start()
    {
        lightMesh = new Mesh();
        lightMesh.name = "Light Mesh";
        lightMashFilter.mesh = lightMesh;
    }
    /*void FindCollidingObstakles(bool isDynamic)
    {
        if (isDynamic)

        {
            Collider2D[] collidingObstacles = Physics2D.OverlapCircleAll(transform.position, lightRadius, obstaclesMask);

            for (int i = 0; i < collidingObstacles.Length; i++)
            {
                //Figure out logic for static and dynamic light sources
            }
        }
    }*/

    private void LateUpdate()
    {
        DrawLightField();
    }

    void DrawLightField()
    {
        int stepCount = Mathf.RoundToInt(lightAngle * meshResolution);
        float stepAngleSize = lightAngle / stepCount;
        List<Vector3> lightPoints = new List<Vector3>();
        LightCastInfo oldLightCast = new LightCastInfo();
        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.z - lightAngle / 2 + stepAngleSize * i;
            LightCastInfo newLightCast = LightCast(angle);

            if (i>0)
            {
                bool edgeDistanceThresholdExceeded = Mathf.Abs(oldLightCast.dist - newLightCast.dist) > edgeDistanceThreshold;
                if(oldLightCast.hit != newLightCast.hit || oldLightCast.hit && newLightCast.hit && edgeDistanceThresholdExceeded)
                {
                    EdgeInfo edge = FindEdge(oldLightCast, newLightCast);
                    if (edge.pointA != Vector3.zero)
                    {
                        lightPoints.Add(edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero)
                    {
                        lightPoints.Add(edge.pointB);
                    }
                }
            }

            lightPoints.Add(newLightCast.point);
            oldLightCast = newLightCast;
        }
        int vertexCount = lightPoints.Count+1;
        Vector3[] vertecies = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertecies.Length];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertecies[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertecies[i + 1] = transform.InverseTransformPoint(lightPoints[i]);
            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(0.5f + (vertecies[i].x) / (2 * lightRadius), 0.5f + (vertecies[i].y) / (2 * lightRadius));
        }

        lightMesh.Clear();
        lightMesh.vertices = vertecies;
        lightMesh.triangles = triangles;
        lightMesh.uv = uvs;
        lightMesh.RecalculateNormals();
    }
    EdgeInfo FindEdge(LightCastInfo minLightCast, LightCastInfo maxLightCast)
    {
        float minAgle = minLightCast.angle;
        float maxAngle = maxLightCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAgle + maxAngle) / 2;
            LightCastInfo newLightCast = LightCast(angle);

            bool edgeDistanceThresholdExceeded = Mathf.Abs(minLightCast.dist - newLightCast.dist) > edgeDistanceThreshold;
            if (newLightCast.hit == minLightCast.hit && !edgeDistanceThresholdExceeded)
            {
                minAgle = angle;
                minPoint = newLightCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newLightCast.point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }

    LightCastInfo LightCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, lightRadius, (int)obstaclesMask);
        if (hit.collider != null)
        {
            return new LightCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new LightCastInfo(false, transform.position+dir*lightRadius, lightRadius, globalAngle);

        }
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if(!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.z;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), 0);
    }


    public struct LightCastInfo
    {
        public bool hit;
        public Vector2 point;
        public float dist;
        public float angle;

        public LightCastInfo(bool _hit, Vector2 _point, float _dist, float _angle)
        {
            hit = _hit;
            point = _point;
            dist = _dist;
            angle = _angle;
        }
    }

    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireMesh(lightMesh, transform.position);
    }
}

