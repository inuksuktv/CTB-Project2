using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusTextAnimation : MonoBehaviour
{
    private float animationSpeed = 50f;
    private RectTransform myRT;
    private Vector3 myPosition;

    void Start()
    {
        Destroy(gameObject, 2f);
        myRT = GetComponent<RectTransform>();
        myPosition = myRT.localPosition;
        myPosition += 128 * Vector3.up;
        myRT.localPosition = myPosition;
    }

    void Update()
    {
        Vector3 myScale = myRT.localScale;
        myScale += animationSpeed * Time.deltaTime * Vector3.one;
        myRT.localScale = myScale;
    }
}
