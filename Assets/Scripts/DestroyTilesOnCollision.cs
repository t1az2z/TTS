using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DestroyTilesOnCollision : MonoBehaviour {
    DestructiblesBehaviour desBeh;
    public GameObject destroyParticle;
    [SerializeField] float disabledTime = .3f;
    Vector3Int[] coordinates = new Vector3Int[6]{new Vector3Int(0, 0, 0),
                                                 new Vector3Int(0, 1, 0),
                                                 new Vector3Int(0, -1, 0),
                                                 new Vector3Int(1, 1, 0),
                                                 new Vector3Int(1, 0, 0),
                                                 new Vector3Int(1, -1, 0) };
    private void Start()
    {
        desBeh = FindObjectOfType<DestructiblesBehaviour>();

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerController player = collision.collider.GetComponent<PlayerController>();


        if (player.currentState == PlayerState.WallBreak)
        {
            int xDirection = player.isFacingLeft ? -1 : 1;
            Vector3 hitPosition = Vector3.zero;

            foreach (ContactPoint2D hit in collision.contacts)
            {

                hitPosition.x = (hit.point.x - 0.01f * hit.normal.x) + xDirection *(desBeh.m_Grid.cellSize.x / 2);
                hitPosition.y = hit.point.y - 0.01f * hit.normal.y;
                desBeh.DestoyCell(desBeh.CountCellsPosition(hitPosition), destroyParticle);

                foreach (Vector3Int addition in coordinates)
                {
                    desBeh.DestoyCell(desBeh.CountCellsPosition(hitPosition) + addition, destroyParticle);
                }
            }
            player.controllsDisabledTimer = disabledTime;
            //player.currentState = PlayerState.WallBreak;
            player.dashExpireTime = 0;
            player.rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            player.impulse.GenerateImpulse(new Vector3(0, 1, 0));

        }
    }

 
    private void OnCollisionStay2D(Collision2D collision)
    {
        PlayerController player = collision.collider.GetComponent<PlayerController>();
        if (player.currentState == PlayerState.WallBreak)
        {
            int xDirection = player.isFacingLeft ? -1 : 1;
            Vector3 hitPosition = Vector3.zero;

            foreach (ContactPoint2D hit in collision.contacts)
            {

                hitPosition.x = (hit.point.x - 0.01f * hit.normal.x) + xDirection * (desBeh.m_Grid.cellSize.x / 2);
                hitPosition.y = hit.point.y - 0.01f * hit.normal.y;
                desBeh.DestoyCell(desBeh.CountCellsPosition(hitPosition), destroyParticle);

                foreach (Vector3Int addition in coordinates)
                {
                    desBeh.DestoyCell(desBeh.CountCellsPosition(hitPosition) + addition, destroyParticle);
                }
            }
            player.controllsDisabledTimer = disabledTime;
            //player.currentState = PlayerState.WallBreak;
            player.dashExpireTime = 0;
            player.rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            player.impulse.GenerateImpulse(new Vector3(0, 1, 0));

        }
    }
}
