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

    List<Subroom> subrooms = new List<Subroom>();

    List<Room> rooms = new List<Room>();

    List<Color> randomColors = new List<Color>();

    Subroom stump = new Subroom(new Rect(0, 0, Screen.width, Screen.height));

    public bool drawDivs;

    //Big room stuff
    [Header("BigRooms")]
    public int bigRoomCount = 1;
    public int bigRoomSizeMultiplier = 4;
    private int bigRoomCounter = 0;
    [Header("1/X chance for bigRoomToSpawn")]
    public int bigRoomSpawnChance = 10;

    [Header("RoomSettings")]

    [Range(0, 1)] public float roomMinWidth = 0.3f;
    [Range(0, 1)] public float roomMinHeight = 0.3f;
    [Range(0, 1)] public float roomMaxWidth = 0.95f;
    [Range(0, 1)] public float roomMaxHeight = 0.95f;

    // Start is called before the first frame update
    void Start()
    {
        RegenerateRooms();

        foreach (Subroom div in subrooms)
        {
            Debug.Log(div.debugID + " width is " + div.divisionRect);
            //Debug.Log(div.debugID + " height is " + div.divisionRect);
        };


        //CreateTwoRooms(leafRoom1, leafRoom2);
    }

    public void RegenerateRooms()
    {
        subrooms = new List<Subroom>();
        divisions = new List<Subroom>();
        Subroom stump = new Subroom(new Rect(0, 0, baseWidth, baseHeight));
        CreateSubrooms(stump);
        AddRoomsToList(stump);
        
        //Make random colours
        for (int i = 0; i < subrooms.Count; i++)
        {
            randomColors.Add(Random.ColorHSV(0f, 1f));
        }

        CreateRoomRooms();

        Debug.Log(rooms.Count);
    }

    public void CreateRoomRooms()
    {
        rooms = new List<Room>();
        int count = 0;
        foreach (Subroom div in subrooms)
        {
            count++;
            
            int bufferW = (int)(div.divisionRect.width - (div.divisionRect.width * roomMaxWidth)); //make a buffer between the maximum width of the room and its parents width;
            int bufferH = (int)(div.divisionRect.height - (div.divisionRect.height * roomMaxHeight)); //make a buffer between the maximum height of the room and its parents height;

            //TODO Check if minimum set is less than 0/maximum is greater than the room dimensions. Because that will break the code

            int roomWidth = (int)Random.Range(div.divisionRect.width * roomMinWidth, div.divisionRect.width - bufferW); //choose width between minimum specified width. And maximum possible width;
            int roomHeight = (int)Random.Range(div.divisionRect.height * roomMinHeight, div.divisionRect.height - bufferH); //choose height between minimum specified height and maximum possible height;
            int roomX = (int)Random.Range(div.divisionRect.x, div.divisionRect.x + (div.divisionRect.width - roomWidth));
            int roomY = (int)Random.Range(div.divisionRect.y, div.divisionRect.y + (div.divisionRect.height - roomHeight));
            Debug.Log($"KEY it {div.divisionRect.x} Yeah yeah yeah and the maximum is ran {roomX} lol {roomY}");
            Rect roomRect = new(roomX, roomY, roomWidth, roomHeight);
            Room room = new Room(roomRect);
            rooms.Add(room);
            Debug.Log(room.rect);
        }
  
    }

    void Update()
    {
        if(Input.GetButtonDown("Fire1"))
        {
            debugCounter = 0;
            bigRoomCounter = 0;
            RegenerateRooms();
        }

        if(Input.GetButtonDown("Jump"))
        {
            CreateRoomRooms();
        }
    }
    
    public void AddRoomsToList(Subroom stump)
    {
        foreach (Subroom div in divisions)
        {
            if (div.IAmEndLeaf() == true)
            {
                subrooms.Add(div);
            }

            //foreach (Subroom div in rooms)
            //{
            //    Debug.Log(rooms.Count + " it is " + div.divisionRect);
            //};
        }
    }

    public void CreateSubrooms(Subroom parentRoom)
    {
         
        if(parentRoom.divisionRect.width/2 < minRoomWidth && parentRoom.divisionRect.height/2 < minRoomHeight) //Stop recursion if the next room split would make the subsequent rooms smaller than the minimum size
        {
            Debug.Log("Subroom " + parentRoom.debugID + " is a leaf!!");
            return;
        }

        //Add a random chance for big rooms up to the total of bigRooms specified by user
        if (Random.Range(1, bigRoomSpawnChance) == 1 && parentRoom.divisionRect.width/bigRoomSizeMultiplier < minRoomWidth && parentRoom.divisionRect.height/bigRoomSizeMultiplier < minRoomHeight && bigRoomCounter < bigRoomCount) 
        {
            Debug.Log("Subroom " + parentRoom.debugID + " is a leaf AND A BIG ROOM");
            bigRoomCounter++;
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
            if (splitH)
            {
                parentRoom.leftChild = new Subroom(new Rect(parentRoom.divisionRect.x, parentRoom.divisionRect.y, parentRoom.divisionRect.width / 2, parentRoom.divisionRect.height));
                parentRoom.rightChild = new Subroom(new Rect(parentRoom.divisionRect.x + parentRoom.divisionRect.width / 2, parentRoom.divisionRect.y, parentRoom.divisionRect.width / 2, parentRoom.divisionRect.height));
            }
            else if(!splitH)
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

        EditorGUI.DrawRect(new Rect(stump.divisionRect.x, stump.divisionRect.y, baseWidth, baseHeight), Color.white);
        foreach (Subroom div in subrooms)
        {
            if (drawDivs)
            {
                //Debug.Log(div);
                EditorGUI.DrawRect(new Rect(div.divisionRect.x, div.divisionRect.y, div.divisionRect.width, div.divisionRect.height), randomColors[colorIndex]);
                colorIndex++;
            }
        };

        colorIndex = 0;
        foreach(Room room in rooms)
        {
            EditorGUI.DrawRect(new Rect(room.rect.x, room.rect.y, room.rect.width, room.rect.height), randomColors[colorIndex]);
            colorIndex++;
        }

    }

    public class Room
    {
        public Rect rect;

        public Room(Rect rect)
        {
            this.rect = rect;
        }
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
