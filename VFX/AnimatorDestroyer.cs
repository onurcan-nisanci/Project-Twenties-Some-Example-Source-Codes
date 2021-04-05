using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorDestroyer : MonoBehaviour
{
    void DestroyAnimator()
    {
        Destroy(GetComponent<Animator>());
    }
}
