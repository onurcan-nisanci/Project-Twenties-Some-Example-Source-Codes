using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackedEntityBehaviour : MonoBehaviour
{
    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.tag.Equals("Coverable Object") || collision.gameObject.tag.Equals("Shell"))
            Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag.Equals("Foreground"))
            Destroy(gameObject);
    }
}
