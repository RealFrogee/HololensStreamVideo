using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkController : MonoBehaviour
{
    public GameObject markPrefab;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(Input.mousePosition);

        if(Input.GetMouseButtonDown(0))
        {
            FroHoloUdp.Instance.AsyncSend(Input.mousePosition.ToString());
            
        }
    }
}
