using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyTilesOnCollision : MonoBehaviour {
    DestructiblesBehaviour desBeh;
    public GameObject destroyParticle;

    private void Start()
    {
        desBeh = FindObjectOfType<DestructiblesBehaviour>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerController player = collision.collider.GetComponent<PlayerController>();
        if (player.isDashing == true)
        {
            Vector3 hitPosition = Vector3.zero;
            foreach (ContactPoint2D hit in collision.contacts)
            {
                hitPosition.x = hit.point.x - 0.01f * hit.normal.x;
                hitPosition.y = hit.point.y - 0.01f * hit.normal.y;
                desBeh.DestoyCell(desBeh.CountCellsPosition(hitPosition));
                GameObject.Instantiate(destroyParticle, hitPosition, Quaternion.identity);
                
                desBeh.DestoyCell(desBeh.CountCellsPosition(hitPosition) + new Vector3Int(1, 0, 0));
                GameObject.Instantiate(destroyParticle, hitPosition + new Vector3(.5f, 0f), Quaternion.identity);
                desBeh.DestoyCell(desBeh.CountCellsPosition(hitPosition) + new Vector3Int(0, -1, 0));
                GameObject.Instantiate(destroyParticle, hitPosition + new Vector3(0, -.5f), Quaternion.identity);
                desBeh.DestoyCell(desBeh.CountCellsPosition(hitPosition) + new Vector3Int(0, 1, 0));
                GameObject.Instantiate(destroyParticle, hitPosition + new Vector3(0f, .5f), Quaternion.identity);
                desBeh.DestoyCell(desBeh.CountCellsPosition(hitPosition) + new Vector3Int(1, 1, 0));
                GameObject.Instantiate(destroyParticle, hitPosition + new Vector3(.5f, .5f), Quaternion.identity);
                desBeh.DestoyCell(desBeh.CountCellsPosition(hitPosition) + new Vector3Int(1, -1, 0));
                GameObject.Instantiate(destroyParticle, hitPosition + new Vector3(.5f, -.5f), Quaternion.identity);
            }
            player.isDashing = false;
            player.dashExpireTime = 0;
            player.rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        PlayerController player = collision.collider.GetComponent<PlayerController>();
        if (player.isDashing == true)
        {
            Vector3 hitPosition = Vector3.zero;
            foreach (ContactPoint2D hit in collision.contacts)
            {
                hitPosition.x = hit.point.x - 0.01f * hit.normal.x;
                hitPosition.y = hit.point.y - 0.01f * hit.normal.y;
                desBeh.DestoyCell(desBeh.CountCellsPosition(hitPosition));
                GameObject.Instantiate(destroyParticle, hitPosition, Quaternion.identity);

                desBeh.DestoyCell(desBeh.CountCellsPosition(hitPosition) + new Vector3Int(1, 0, 0));
                GameObject.Instantiate(destroyParticle, hitPosition + new Vector3(.5f, 0f), Quaternion.identity);
                desBeh.DestoyCell(desBeh.CountCellsPosition(hitPosition) + new Vector3Int(0, -1, 0));
                GameObject.Instantiate(destroyParticle, hitPosition + new Vector3(0, -.5f), Quaternion.identity);
                desBeh.DestoyCell(desBeh.CountCellsPosition(hitPosition) + new Vector3Int(0, 1, 0));
                GameObject.Instantiate(destroyParticle, hitPosition + new Vector3(0f, .5f), Quaternion.identity);
                desBeh.DestoyCell(desBeh.CountCellsPosition(hitPosition) + new Vector3Int(1, 1, 0));
                GameObject.Instantiate(destroyParticle, hitPosition + new Vector3(.5f, .5f), Quaternion.identity);
                desBeh.DestoyCell(desBeh.CountCellsPosition(hitPosition) + new Vector3Int(1, -1, 0));
                GameObject.Instantiate(destroyParticle, hitPosition + new Vector3(.5f, -.5f), Quaternion.identity);
            }
            player.isDashing = false;
            player.dashExpireTime = 0;
            player.rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        }
    }
}
