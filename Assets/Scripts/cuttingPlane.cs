using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cuttingPlane : MonoBehaviour
{
    public Material mat1;
    float   delta_x=0.1f, delta_y=0.1f, delta_rot_x=0.0f, delta_rot_y=0.0f, delta_rot_z=0.0f;
    string mode = "x";
    Quaternion rot;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // move();
        mat1.SetVector("_planePosition", transform.position);
        mat1.SetVector("_planeNormal", transform.up);        
    }

    void move()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            mode    = "x";
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)){
            mode    = "y";
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3)){
            mode    = "z";
        }
        if (Input.GetKey(KeyCode.UpArrow)){
            translation(true);
        }
        if (Input.GetKey(KeyCode.DownArrow)){
            translation(false);
        }
        if (Input.GetKey(KeyCode.RightArrow)){
            rotation(1.0f);
        }
        else if (Input.GetKey(KeyCode.LeftArrow)){
            rotation(-1.0f);
        }
    }
    void translation(bool dir)
    {
        // transform.position = transform.position + new Vector3(0, delta_x, delta_y);
        Debug.Log(Vector3.forward);
        if (dir){
            transform.position += transform.TransformDirection(new Vector3(0.0f,1.0f,0.0f));
        }
        else{
            transform.position -= transform.TransformDirection(new Vector3(0.0f,1.0f,0.0f));
        }
    }
    void rotation(float dir)
    {
        if (mode=="x"){
            delta_rot_x = delta_rot_x + dir*0.1f;
            rot = Quaternion.Euler(delta_rot_x, delta_rot_y, delta_rot_z);
        }
        else if (mode=="y"){
            delta_rot_y = delta_rot_y - dir*0.1f;
            rot = Quaternion.Euler(delta_rot_x, delta_rot_y, delta_rot_z);
        }
        else if (mode=="z"){
            delta_rot_z = delta_rot_z - dir*0.1f;
            rot = Quaternion.Euler(delta_rot_x, delta_rot_y, delta_rot_z);
        }
        transform.rotation = rot;
    }
}
