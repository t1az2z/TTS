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
        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.z - lightAngle / 2 + stepAngleSize * i;
            LightCastInfo newLightCast = LightCast(angle);
            lightPoints.Add(newLightCast.point);
        }
        int vertexCount = lightPoints.Count+1;
        Vector3[] vertecies = new Vector3[vertexCount];
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
        lightMesh.Clear();
        lightMesh.vertices = vertecies;
        lightMesh.triangles = triangles;
        lightMesh.RecalculateNormals();
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireMesh(lightMesh, transform.position);
    }
}

