using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallBloodSortingLayerAdjuster : MonoBehaviour
{
    void DestroyAnimator()
    {
        Destroy(GetComponent<Animator>());
        Destroy(GetComponent<BoxCollider2D>());
        Destroy(this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Mirror")
        {
            GetComponent<SpriteRenderer>().sortingOrder = 5;
            Color spriteColor = GetComponent<SpriteRenderer>().color;
            spriteColor.a = 0.5f;
            GetComponent<SpriteRenderer>().color = spriteColor;
        }

        if(collision.tag == "Environment")
        {
            GetComponent<SpriteRenderer>().sortingOrder = 5;
            Color spriteColor = GetComponent<SpriteRenderer>().color;
            spriteColor.a = 0.6f;
            GetComponent<SpriteRenderer>().color = spriteColor;
        }
    }

}
