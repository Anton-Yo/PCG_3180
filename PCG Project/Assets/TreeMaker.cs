using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TreeMaker : MonoBehaviour
{

    private bool splitH = true;
    public int baseWidth = Screen.width;
    public int baseHeight = Screen.height;
    private Rect baseRoom;
    private int splitCount;

    [Header("Generation Settings")]
    
    public int minRoomWidth = 100;

    public int minRoomHeight = 100;

    public int maxRoomWidth = 100;

    public int maxRoomHeight = 100;

    static public int debugCounter;

    List<Subroom> divisions = new List<Subroom>();

    List<Subroom> rooms = new List<Subroom>();

    List<Color> randomColors = new List<Color>();

    // Start is called before the first frame update
    void Start()
    {
        RegenerateRooms();

        foreach (Subroom div in rooms)
        {
            Debug.Log(div.debugID + " width is " + div.divisionRect);
            //Debug.Log(div.debugID + " height is " + div.divisionRect);
        };


        //CreateTwoRooms(leafRoom1, leafRoom2);
    }

    public void RegenerateRooms()
    {
        Subroom stump = new Subroom(new Rect(0, 0, baseWidth, baseHeight));
        CreateSubrooms(stump);
        AddRoomsToList(stump);

        //Make random colours
        for (int i = 0; i < rooms.Count; i++)
        {
            randomColors.Add(Random.ColorHSV(0f, 1f));
        }
    }

    void Update()
    {
        if(Input.GetButtonDown("Fire1"))
        {
            debugCounter = 0;
            RegenerateRooms();


        }
    }
    
    public void AddRoomsToList(Subroom stump)
    {
        foreach (Subroom div in divisions)
        {
            if (div.IAmEndLeaf() == true)
            {
                rooms.Add(div);
            }

            //foreach (Subroom div in rooms)
            //{
            //    Debug.Log(rooms.Count + " it is " + div.divisionRect);
            //};
        }
    }

    public void CreateSubrooms(Subroom parentRoom)
    {
        
        if(parentRoom.divisionRect.width/2 < maxRoomWidth && parentRoom.divisionRect.height/2 < maxRoomHeight)
        {
            Debug.Log("Subroom " + parentRoom.debugID + "is a leaf!!");
            return;
        }
       
        //splitH = Random.Range(0f, 1f) > 0.5f;
        if (parentRoom.divisionRect.width / parentRoom.divisionRect.height >= 1.25f) //1.25f is ratio between sides so that they are less that 1.25x size apart. 
        {
            splitH = true;
        }
        else if(parentRoom.divisionRect.height / parentRoom.divisionRect.width >= 1.25f)
        {
            splitH = false;
        }
        else //make it a random choice if the room is basically a square already
        {
            splitH = Random.Range(0f, 1f) > 0.5f;
        }
        //Debug.Log("COoking");

            //Create the subrooms
            if (splitH && parentRoom.divisionRect.width > maxRoomWidth)
            {
                parentRoom.leftChild = new Subroom(new Rect(parentRoom.divisionRect.x, parentRoom.divisionRect.y, parentRoom.divisionRect.width / 2, parentRoom.divisionRect.height));
                parentRoom.rightChild = new Subroom(new Rect(parentRoom.divisionRect.x + parentRoom.divisionRect.width / 2, parentRoom.divisionRect.y, parentRoom.divisionRect.width / 2, parentRoom.divisionRect.height));
            }
            else if(!splitH && parentRoom.divisionRect.height > maxRoomHeight)
            {
                parentRoom.leftChild = new Subroom(new Rect(parentRoom.divisionRect.x, parentRoom.divisionRect.y, parentRoom.divisionRect.width, parentRoom.divisionRect.height/2));
                parentRoom.rightChild = new Subroom(new Rect(parentRoom.divisionRect.x, parentRoom.divisionRect.y + parentRoom.divisionRect.height / 2, parentRoom.divisionRect.width, parentRoom.divisionRect.height/2));
            }
            else
            {
                return;
            }

            Debug.Log("The left child of " + parentRoom.divisionRect + " is " + parentRoom.leftChild.divisionRect);
            Debug.Log("The right child of " + parentRoom.divisionRect + " is " + parentRoom.rightChild.divisionRect);
            splitCount++;
            divisions.Add(parentRoom.leftChild);
            divisions.Add(parentRoom.rightChild);

            CreateSubrooms(parentRoom.leftChild);
            CreateSubrooms(parentRoom.rightChild);
    }

    public void CreateTwoRooms(Rect rect1, Rect rect2)
    {
        Rect[] rectArr = new Rect[2];
        rectArr[0] = rect1;
        rectArr[1] = rect2;
        for(int i = 0; i < 2; i++) //do twice rn. Probs gonna need to use nodes/recursion tho
        {
           // divisions.Add(new Rect(rectArr[i].x, rectArr[i].y, rectArr[i].width/2, rectArr[i].height));
            //divisions.Add(new Rect(rectArr[i].x + rectArr[i].width / 2, rectArr[i].y, rectArr[i].width/2, rectArr[i].height));
        }
        
        return;
    }

    public void Split(Subroom subroom)
    {
        Debug.Log("Splitting " + subroom.debugID + " and the rect is " + subroom.divisionRect);
    }

    void OnGUI()
    {

        //Make extra rooms
        int colorIndex = 0;
        foreach (Subroom div in rooms)
        {
            //Debug.Log(div);
            EditorGUI.DrawRect(new Rect(div.divisionRect.x, div.divisionRect.y, div.divisionRect.width, div.divisionRect.height), randomColors[colorIndex]);
            colorIndex++;
        };

        // EditorGUI.DrawRect(new Rect(leafRoom11.x, leafRoom11.y, leafRoom11.width, leafRoom11.height), Color.red);
        // EditorGUI.DrawRect(new Rect(leafRoom12.x, leafRoom12.y, leafRoom11.width, leafRoom12.height), Color.green);
        // EditorGUI.DrawRect(new Rect(leafRoom21.x, leafRoom21.y, leafRoom21.width, leafRoom21.height), Color.blue);
        // EditorGUI.DrawRect(new Rect(leafRoom22.x, leafRoom22.y, leafRoom22.width, leafRoom22.height), Color.yellow);

    }

    public class Subroom
    {
        public Subroom leftChild;
        public Subroom rightChild;
        public int debugID;

        public Rect divisionRect;

        public Subroom(Rect baseRect)
        {
            divisionRect = baseRect;
            debugID = debugCounter;
            debugCounter++;
        }

        public bool IAmEndLeaf()
        {
            return leftChild == null && rightChild == null;
        }
    }
}
