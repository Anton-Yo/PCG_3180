using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TreeMaker : MonoBehaviour
{

    public bool splitH = true;
    public int width;
    public int height;
    private Rect baseRoom;
    private Rect prevRoom;
    private Rect leafRoom1;
    private Rect leafRoom2;

    private Rect leafRoom11;
    private Rect leafRoom12;
    private Rect leafRoom21;
    private Rect leafRoom22;

    public int splitXTimes;
    public int leafID;

    List<Rect> divisions = new List<Rect>();

    // Start is called before the first frame update
    void Start()
    {
        baseRoom = new Rect(0, 0, Screen.width, Screen.height);
        divisions.Add(baseRoom);
        prevRoom = baseRoom;
        //leafRoom1 = new Rect(Screen.width / 2, 0, Screen.width / 2, Screen.height);
        
        if (splitH)
        {
            leafRoom1 = new Rect(0, 0, baseRoom.width / 2, baseRoom.height);
            leafRoom2 = new Rect(baseRoom.width / 2, 0, baseRoom.width / 2, baseRoom.height);

            leafRoom11 = new Rect(leafRoom1.x, leafRoom1.y, leafRoom1.width / 2, leafRoom1.height);
            leafRoom12 = new Rect(leafRoom1.x + leafRoom1.width/2, leafRoom1.y, leafRoom1.width / 2, leafRoom1.height);

            leafRoom21 = new Rect(leafRoom2.x, leafRoom2.y, leafRoom2.width / 2, leafRoom2.height);
            leafRoom22 = new Rect(leafRoom2.x + leafRoom2.width/2, leafRoom2.y, leafRoom2.width / 2, leafRoom2.height);

            Debug.Log(leafRoom1);
            Debug.Log(leafRoom2);
        }
        else
        {

        }
        //Debug.Log(baseRoom);
        //Debug.Log(leafRoom1);
        //Debug.Log(baseRoom2);
        CreateTwoRooms();
    }

    // Update is called once per frame
    void Update()
    {
       //Debug.Log(baseRoom.size);
       //Debug.Log(baseRoom);
    }

    public void CreateTwoRooms()
    {
        return;
    }

    void OnGUI()
    {
      
       // EditorGUI.DrawRect(new Rect(leafRoom1.x, leafRoom1.y, leafRoom1.width, leafRoom1.height), Color.green);
       // EditorGUI.DrawRect(new Rect(leafRoom2.x, leafRoom2.y, leafRoom2.width, leafRoom2.height), Color.yellow);

        //Make extra rooms
        //divisions.ForEach({
       //     Editor.GUI.DrawRect(new Rect( ))
        //})

        EditorGUI.DrawRect(new Rect(leafRoom11.x, leafRoom11.y, leafRoom11.width, leafRoom11.height), Color.red);
        EditorGUI.DrawRect(new Rect(leafRoom12.x, leafRoom12.y, leafRoom11.width, leafRoom12.height), Color.green);
        EditorGUI.DrawRect(new Rect(leafRoom21.x, leafRoom21.y, leafRoom21.width, leafRoom21.height), Color.blue);
        EditorGUI.DrawRect(new Rect(leafRoom22.x, leafRoom22.y, leafRoom22.width, leafRoom22.height), Color.yellow);

    }
}
