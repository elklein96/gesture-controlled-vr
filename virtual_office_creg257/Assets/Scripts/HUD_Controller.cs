using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUD_Controller : MonoBehaviour {

	// Use this for initialization
	public float width;
	public float height;
    public int Display_Option; 
    public float smooth;
    List<GameObject> HUD = new List<GameObject>();
	

void Start() {  
	width = 1;
	height = 1;
    smooth = .5f;
    Display_Option = 2;
    //INIT Variables 
    Vector3[] vertices = new Vector3[4]; 
    vertices[0] = new Vector3(0, 0, 0);
    vertices[1] = new Vector3(width, 0, 0);
    vertices[2] = new Vector3(0, height, 0);
    vertices[3] = new Vector3(width, height, 0);
    
    int[] tri = new int[6];
    tri[0] = 0;
    tri[1] = 2;
    tri[2] = 1;
    tri[3] = 2;
    tri[4] = 3;
    tri[5] = 1;
    
    Vector3[] normals = new Vector3[4];
    normals[0] = -Vector3.forward;
    normals[1] = -Vector3.forward;
    normals[2] = -Vector3.forward;
    normals[3] = -Vector3.forward;
    
    Vector2[] uv = new Vector2[4];
    uv[0] = new Vector2(0, 0);
    uv[1] = new Vector2(1, 0);
    uv[2] = new Vector2(0, 1);
    uv[3] = new Vector2(1, 1);

    //INIT HUD OBJECT
	for(int i = 0; i < Display_Option; i++){
            HUD.Add(Instantiate(Resources.Load("DISPLAY", typeof(GameObject))) as GameObject); //load display prefab
	}
    if(HUD == null){
        Debug.Log("[ERROR]  DIsplay not initialized");
    }
    else{
        foreach(GameObject dsply in HUD){
            dsply.GetComponent<MeshFilter>().mesh.triangles = tri;
            dsply.GetComponent<MeshFilter>().mesh.vertices = vertices;
            dsply.GetComponent<MeshFilter>().mesh.normals = normals;
            dsply.GetComponent<MeshFilter>().mesh.uv = uv;
        }
    }

    
    
}
	
	// Update is called once per frame
	void Update () {

        float tiltAroundZ = 0;
        float tiltAroundY = 0;
		float tiltAroundX = 0;
        float x_pos = 0;
        float y_pos = 1;
        float z_pos = 1; 
        int display_count = HUD.Count;
        int count = 0;
        float incr = ( 180f / display_count); 
        foreach(GameObject dsply in HUD){
            tiltAroundY = tiltAroundY + incr+count + 90;
            dsply.transform.position= RandomCircle(this.transform.position, 2f, count, display_count);
            Quaternion target = Quaternion.Euler(tiltAroundX, tiltAroundY, tiltAroundZ);
            dsply.transform.rotation = Quaternion.Slerp(dsply.transform.rotation, target, Time.deltaTime * smooth);
            count ++;
        }
		

		//Quaternion target = Quaternion.Euler(tiltAroundX, tiltAroundY, tiltAroundZ);

		//this.transform.rotation = Quaternion.Slerp(transform.rotation, target,  Time.deltaTime * smooth);
	}

    Vector3 RandomCircle(Vector3 center, float radius, int index, int parts){ 
        // create random angle between 0 to 360 degrees 
        float ang = (360f/parts) * index; 
        Vector3 pos; 
        pos.z = center.z + radius * Mathf.Sin(ang * Mathf.Deg2Rad); 
        pos.x = center.x + radius * Mathf.Cos(ang * Mathf.Deg2Rad); 
        pos.y = center.y; 
        return pos; 
        
        }
}
