using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DestructiblesBehaviour : MonoBehaviour {

    public Grid m_Grid;
    public Tilemap m_Destructible;
    private GridInformation m_GridInfo;

	// Use this for initialization
	void Start () {
        m_Grid = GetComponent<Grid>();
        m_GridInfo = GetComponent<GridInformation>();
        if (m_Destructible == null)
        {
            Debug.Log("Destructibles Tilemap is not assigned.");
            m_Destructible = GameObject.Find("Destructibles").GetComponent<Tilemap>();
        }
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
            Debug.Log("Destructibles Tilemap is not assigned.");
            return Vector3Int.zero;
        }
    }
    public void DestoyCell(Vector3Int position, GameObject particle)
    {
        if (m_Destructible.GetTile(position) != null)
        {

            m_Destructible.SetTile(position, null);
            GameObject.Instantiate(particle, m_Grid.GetCellCenterWorld(position), Quaternion.identity);
        }
    }

}
