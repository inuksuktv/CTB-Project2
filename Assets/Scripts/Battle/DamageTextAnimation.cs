using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTextAnimation : MonoBehaviour
{
    private float animationSpeed = 50f;
    private RectTransform myRT;
    private Vector3 myPosition;

    void Start()
    {
        Destroy(gameObject, 2f);
        myRT = GetComponent<RectTransform>();
        myPosition = myRT.localPosition;
        myPosition += 64 * Vector3.up;
        myRT.localPosition = myPosition;
    }

    void Update()
    {
        Vector3 myPosition = myRT.localPosition;
        myPosition += animationSpeed * Time.deltaTime * Vector3.up;
        myRT.localPosition = myPosition;
    }
}
