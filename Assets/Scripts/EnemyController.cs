using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        LayerMask layerMask = LayerMask.GetMask("BattleManager");
        int mask = layerMask.value;

        Collider2D hitCollider = Physics2D.OverlapCircle(transform.position, 15f, mask);

        if (hitCollider != null) {
            hitCollider.gameObject.GetComponent<BattleManager>().StartCombat(collision);
        }
    }
}
