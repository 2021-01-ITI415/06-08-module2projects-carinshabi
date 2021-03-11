using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetSortOrder : MonoBehaviour
{
    private void OnEnable()
    {
        GetComponent<SpriteRenderer>().sortingOrder = 0;
    }
}
