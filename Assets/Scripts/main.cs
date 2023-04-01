using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class main : MonoBehaviour
{
    string translation_mode = "xy";
    float   delta=0.1f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        move();
    }

    void move()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            translation_mode    = "x";
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)){
            translation_mode    = "y";
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3)){
            translation_mode    = "z";
        }
        if (Input.GetKey(KeyCode.UpArrow)){
            translation(delta);
        }
        if (Input.GetKey(KeyCode.DownArrow)){
            translation(-delta);
        }
    }

    void translation(float delta)
    {
        if (translation_mode == "x"){
            transform.position = transform.position + new Vector3(delta, 0, 0);
        }
        else if (translation_mode == "y"){
            transform.position = transform.position + new Vector3(0, delta, 0);
        }
        else if (translation_mode == "z"){
            transform.position = transform.position + new Vector3(0, 0, delta);
        }
    }
}
