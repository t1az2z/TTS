using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class RaycastLight : MonoBehaviour
{
    public bool gizmoEnable = false;
    private bool isMoved = false;
    private Vector2 oldPos = Vector3.zero;
    [Header("Mesh Parameters")]
    public bool isDynamic = false;
    private Color prevColor;
    public float lightRadius;
    [Range(0, 360)]
    public float lightAngle;
    [Range(0, .5f)] public float meshResolution;
    public float edgeDistanceThreshold;
    public int edgeResolveIterations;
    public LayerMask obstaclesMask;
    public MeshFilter lightMeshFilter;
    MeshRenderer mr;
    private Mesh lightMesh;

    [Header("Additional  Mesh Parameters")]
    public bool lightOverlap = false;
    [Range(0, .1f)] public float lightMultiplierRange = .07f;
    private float lightMultiplier = 1;

    /*[Header("Light Parameters (shader)")]
    public Material material;
    private Material tempMaterial;
    public Color color = new Color(1, 1, 1, 1);
    public float contrastFactor = 1.15f;
    public float colorFactor = .25f;
    public float rotationSpeed = 2f;*/

    private void Start()
    {
        lightMesh = new Mesh();
        lightMesh.name = "Light Mesh";
        lightMeshFilter.mesh = lightMesh;
        mr = GetComponent<MeshRenderer>();
        /*mr.material = material;
        tempMaterial = new Material(mr.sharedMaterial);*/
        mr.sortingLayerName = "Middleground";
        mr.sortingOrder = 0;

        //SetShaderParameters();

        DrawLightField();
    }

    /*private void SetShaderParameters()
    {
        tempMaterial.color = color;
        tempMaterial.SetFloat("_ContrastFactor", contrastFactor);
        tempMaterial.SetFloat("_ColorFactor", colorFactor);
        tempMaterial.SetFloat("_RotationSpeed", rotationSpeed);
        mr.sharedMaterial = tempMaterial;
    }*/

    private void Update()
    {
        //SetShaderParameters();
        isMoved = CheckForMovement();
        if (lightOverlap)
            lightMultiplier = 1 + lightMultiplierRange;
        else
            lightMultiplier = 1 / (1 + lightMultiplierRange);

    }

    private bool CheckForMovement()
    {
        Vector2 curPos = transform.position;
        if (curPos == oldPos)
            return false;
        else
        {
            oldPos = curPos;
            return true;
        }

    }

    private void LateUpdate()
    {
        if (isDynamic && Physics2D.OverlapCircle(transform.position, lightRadius) != null && isMoved)
        {
            DrawLightField();
        }
    }

    void DrawLightField()
    {
        int stepCount = Mathf.RoundToInt(lightAngle*meshResolution);
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
            var vert = transform.InverseTransformPoint(lightPoints[i])*lightMultiplier;

            vertecies[i + 1] = vert;
            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(0.5f + (vertecies[i].x) / (2 * (lightRadius * lightMultiplier)), 0.5f + (vertecies[i].y) / (2 * (lightRadius * lightMultiplier)));
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
        if (gizmoEnable)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireMesh(lightMesh, transform.position);
        }
    }
}

