using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DestructiblesBehaviour : MonoBehaviour {

    public Grid m_Grid;
    private Tilemap m_Destructible;
    private GridInformation m_GridInfo;

	// Use this for initialization
	void Start () {
        m_Grid = GetComponent<Grid>();
        m_GridInfo = GetComponent<GridInformation>();
        m_Destructible = GameObject.Find("Destructibles").GetComponent<Tilemap>();
	}
	
    public Vector3Int CountCellsPosition(Vector3 pos)
    {
        if (m_Grid)
        {
            Vector3Int gridPos = m_Grid.WorldToCell(pos);
            return gridPos;
        }
        else
        {
            Debug.Log("No Destructibles Tilemap");
            return Vector3Int.zero;
        }
    }
    public void DestoyCell(Vector3Int position)
    {
        if (m_Destructible.GetTile(position) != null)
        {
            m_Destructible.SetTile(position, null);
        }
    }

}
