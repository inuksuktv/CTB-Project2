using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    Transform[] heroMarkers = new Transform[4];

    private float animationSpeed = 10f;

    void Start()
    {
        for (int i = 0; i < 4; i++) {
            heroMarkers[i] = transform.GetChild(i);
        }
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        StartCombat(collider);
    }

    public void StartCombat(Collider2D collider)
    {
        Debug.Log("Started combat with " + collider.gameObject.name);
        collider.gameObject.GetComponent<PlayerController>().enabled = false;
        TakeYourMarks();
    }

    public void StartCombat(Collision2D collision)
    {
        Debug.Log("Started combat with " + collision.gameObject.name);
        collision.gameObject.GetComponent<PlayerController>().enabled = false;
        TakeYourMarks();
    }

    private void TakeYourMarks()
    {
        int index = 0;
        foreach (GameObject hero in GameManager.Instance.heroes) {
            Transform mark = heroMarkers[index];
            IEnumerator coroutine = MoveToTarget(hero, mark);
            StartCoroutine(coroutine);
            index++;
        }
    }

    private IEnumerator MoveToTarget(GameObject unit, Transform target)
    {
        while (MoveTick(unit, target)) { yield return null; }
    }

    private bool MoveTick(GameObject unit, Transform target)
    {
        unit.transform.position = Vector2.MoveTowards(unit.transform.position, target.position, animationSpeed * Time.deltaTime);
        return !(Mathf.Approximately(unit.transform.position.x - target.transform.position.x, 0) && Mathf.Approximately(unit.transform.position.y - target.transform.position.y, 0));
    }
}
