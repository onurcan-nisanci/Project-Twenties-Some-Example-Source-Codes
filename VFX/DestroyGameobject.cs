using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyGameobject : MonoBehaviour
{
  [SerializeField] float Time = 0f;
  [SerializeField] bool OnStart;

    private void Start()
    {
        if (OnStart)
            DestroyGO();
    }

    void DestroyGO()
    {
        Destroy(gameObject, Time);
    }
}
