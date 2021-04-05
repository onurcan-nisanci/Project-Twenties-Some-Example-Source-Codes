using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StairBehaviour : MonoBehaviour
{
    private CapsuleCollider2D _topCollider;

    // Start is called before the first frame update
    void Start()
    {
        _topCollider = transform.GetChild(0).GetComponent<CapsuleCollider2D>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.gameObject.tag.Equals("Player"))
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                _topCollider.enabled = true;
            else if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                _topCollider.enabled = false;
        }
    }
}
