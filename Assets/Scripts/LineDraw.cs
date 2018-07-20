using UnityEngine;
[ExecuteInEditMode]
public class LineDraw : MonoBehaviour {
    public Transform point1;
    public Transform point2;

    private LineRenderer line;
    [SerializeField] float lineWidth = .05f;

    private void Awake()
    {

    }

    private void Start()
    {
        line = gameObject.AddComponent<LineRenderer>();
        //line.material = new Material(Shader.Find("Sprites/Default"));
        //line.startWidth = lineWidth;
        //line.endWidth = lineWidth;
        line.positionCount = 2;
    }

    private void Update()
    {
        if (point1 && point2)
        {
            line.SetPosition(0, point1.position);
            line.SetPosition(1, point2.position);
        }
    }
}
