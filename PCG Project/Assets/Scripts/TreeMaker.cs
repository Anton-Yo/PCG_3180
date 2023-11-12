using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using TMPro;

public class TreeMaker : MonoBehaviour
{

    private bool splitH = true;
    static private int debugCounter;

    List<Subroom> divisions = new List<Subroom>();

    List<Subroom> subrooms = new List<Subroom>(); //rename this thingo

    List<Room> rooms = new List<Room>();
    List<Rect> paths = new List<Rect>();

    List<Color> randomColors = new List<Color>();

    Subroom stump;

    [Header("This is a generator script that uses a modified binary partitioning algorithm to create a dungeon layout.")]
    [Space(-5), Header("Tooltips are only visible before running the program, but everything can be changed during runtime by regenerating")]
    [Space(-5), Header("Use <Left-Click> to regenerate the layout, and press <Space> to regenerate just the rooms and paths")]
    [Space(-5), Header("Other controls: <WASD or Arrow Keys> to move around. <Mouse Wheel> to scroll")]

    [Header("Base settings")]
    [Tooltip("X position of the rect containing all of the generated content")] public int startingX = 0;
    [Tooltip("Y position of the rect containing all of the generated content")] public int startingY = 0;
    [Tooltip("Width of the rect containing all of the generated content. Larger value results in more rooms (if minimum division size is the same)")] public int baseWidth = Screen.width;

    [Tooltip("Height of the rect containing all of the generated content. Larger value results in more rooms (if minimum division size is the same)")] public int baseHeight = Screen.height;

    [Tooltip("Colour of the base (background)")] public Color baseColour;

    [Tooltip("Adjust camera move/zoom speed based on width and height of base")] public bool scaleCameraSpeedByBaseSize;
   

    [Header("Generation Settings")]
    
    [Header("Minimum division size. A room is generated based on the division sizes")]
  
    [Tooltip("All divisions and therefore the rooms inside them, will be greater than this width")] public int minDivisionWidth = 100;

    [Tooltip("All divisions and therefore the rooms inside them, will be greater than this height")] public int minDivisionHeight = 100;


    [Header("Room Settings"), Header("Room will fill a random range between min% and max% of the divisions width and height."), Space(-5), Header("E.g with min values of 40 and max values of 80, a room will occupy 40%-80% of the division's space")]
    [Header("Max will always be enforced, it overrules minimum values")]

    [Tooltip("Room will always be bigger than X% of the division's width. As long as min < max")]
    [Range(0, 100)] public float roomMinWidth = 30;

    [Tooltip("Room will always be bigger than X% of the division's height. As long as min < max")]
    [Range(0, 100)] public float roomMinHeight = 30;

    [Tooltip("Room will always be smaller than X% of the division's width.")]
    [Range(0, 100)] public float roomMaxWidth = 95;

    [Tooltip("Room will always be smaller than X% of the division's height.")]
    [Range(0, 100)] public float roomMaxHeight = 95;

    [Header("Corridor Settings")]
    [Tooltip("Width of the corridor")] public float corridorWidth;

    [Tooltip("Colour of the corridors")] public Color pathColour;

    public enum BigRoomGenerationType
    {
        ByCount,
        ByChance,
    };

    [Header("Big room settings: A.k.a Double the size of a normal room. NOTE: This will break if big room count > room count/2")]
    [Tooltip("Choose whether the generator goes with manual number of big rooms, or % chance for it to spawn.")] public BigRoomGenerationType bigRoomGenerationType;
    [Tooltip("Number of big rooms. A big room combines two child rooms into one room. The sizes may vary based on the size of the children")] public int bigRoomCount = 1;

    [Tooltip("Chance for a big room to spawn. A big room combines two child rooms into one room. The sizes may vary based on the size of the children")] [Range(0,100)] public float bigRoomSpawnChance = 0.1f;


    [Header("Dungeon drawing settings")]
    [Tooltip("Draws the divisions over the top of everything. Mostly for debug purposes")] public bool drawDivisions;

    [Tooltip("Draw order for 4 dungeon components. Default is divisions, base, paths, rooms")] public List<string> drawOrder = new List<string>();


    [Header("DebugUI settings")]
    [Tooltip("Draw Debug UI")] public bool drawDebugUI = false;

    [Tooltip("Will show the divisions a.k.a the room's parent rather than the rooms")] public bool drawDivs;

    [Tooltip("Paths are visible over rooms and other debug tools")] public bool drawPathsOverRooms;



    [Header("Unity Scripting Stuff. CAN IGNORE")]
    public GameObject rectanglePrefab;

    public GameObject baseParent;
    public GameObject divParent;
    public GameObject pathsParent;
    public GameObject roomsParent;
    public TMP_Text roomCount;



    // Start is called before the first frame update
    void Start()
    {       
        RegenerateRooms();
    }

    public void RegenerateRooms()
    {
        debugCounter = 0;
        subrooms = new List<Subroom>();
        divisions = new List<Subroom>();
        randomColors = new List<Color>();
        paths = new List<Rect>();
        
        stump = new Subroom(new Rect(startingX, startingY, baseWidth, baseHeight));
        Debug.Log("Creating base for generator");

        divisions.Add(stump);
        CreateDivisions(stump);
        Debug.Log("Finished creating divisions");

        AddRoomsToList(stump);
        MakeBigRooms();
        Debug.Log("Big divisions created (if any)");

        CreateRoomInSubrooms();
        Debug.Log("Created rooms in all the divisions that are leaves");
     
        foreach(Subroom div in divisions)
        {
            //Debug.Log(div.divisionRect);
            // if(div.IAmEndLeaf())
            // {
            //     Debug.Log($"DebugID is {div.debugID}    End leaf: {div.IAmEndLeaf()}");
            // }
            // else
            // {
            //     Debug.Log($"DebugID is {div.debugID}    End leaf: {div.IAmEndLeaf()}     /n left child {div.leftChild}  with id of {div.leftChild.debugID}   /n  right child {div.rightChild} with id of {div.rightChild.debugID}");
            // }
            
            // if(div.containedRoom == null)
            // {
            //     Debug.LogError("contained room is null");
            // }
            // else
            // {
            //     Debug.Log($"The contained room is {div.containedRoom.rect}");
            // }
           
            if(!div.IAmEndLeaf())
            {
                CreatePathwayBetween(div.leftChild, div.rightChild);
            }
           
        }
        Debug.Log("Pathways between rooms created");

        for (int i = 0; i < subrooms.Count; i++)   //Make random colours
        {
            randomColors.Add(Random.ColorHSV(0f, 1f));
        }
        Debug.Log("Random colours generated for rooms");

        DrawEveryRectangle();
        Debug.Log("Rectangles were instantiated for everything, paths/base/rooms etc");
    }

    public void CreateRoomInSubrooms()
    {
        rooms = new List<Room>();
        int count = 0;
        foreach (Subroom div in subrooms)
        {
            count++;
            
            int bufferW = (int)(div.divisionRect.width - (div.divisionRect.width * roomMaxWidth/100)); //make a buffer between the maximum width of the room and its parents width;
            int bufferH = (int)(div.divisionRect.height - (div.divisionRect.height * roomMaxHeight/100)); //make a buffer between the maximum height of the room and its parents height;

            //TODO Check if minimum set is less than 0/maximum is greater than the room dimensions. Because that will break the code

            int roomWidth = (int)Random.Range(div.divisionRect.width * roomMinWidth/100, div.divisionRect.width - bufferW); //choose width between minimum specified width. And maximum possible width;
            int roomHeight = (int)Random.Range(div.divisionRect.height * roomMinHeight/100, div.divisionRect.height - bufferH); //choose height between minimum specified height and maximum possible height;

            if(roomWidth > roomMaxWidth/100 * div.divisionRect.width) //if the room width is greater than the max width. Make it the max width
            {
                roomWidth = (int) (roomMaxWidth/100 * div.divisionRect.width);
            }

            if(roomWidth > roomMaxHeight/100 * div.divisionRect.height) //if the room height is greater than the max height. Make it the max height
            {
                roomWidth = (int) (roomMaxHeight/100 * div.divisionRect.height);
            }
            int roomX = (int)Random.Range(div.divisionRect.x, div.divisionRect.x + (div.divisionRect.width - roomWidth));
            int roomY = (int)Random.Range(div.divisionRect.y, div.divisionRect.y + (div.divisionRect.height - roomHeight));
            Rect roomRect = new(roomX, roomY, roomWidth, roomHeight);
            Room room = new Room(roomRect);
            div.containedRoom = room; //make the subroom know what room it has
            rooms.Add(room);
            //Debug.Log(room.rect);
        }
  
    }

    void Update()
    {
        if(Input.GetButtonDown("Fire1"))
        {
            debugCounter = 0;
            RegenerateRooms();
        }

        if(Input.GetButtonDown("Jump"))
        {
            CreateRoomInSubrooms();
            paths = new List<Rect>();
            foreach(Subroom div in divisions) //create paths again
            {
                if(!div.IAmEndLeaf())
                {
                    CreatePathwayBetween(div.leftChild, div.rightChild);
                }
           
            }
            DrawEveryRectangle(); //draw the new rooms
     
        }
    }

    public void MakeBigRooms()
    {
        //grab amount of pairs in end rooms
        //grab end leaf count
        //divide by 2 to get number of pairs
        //Go back up by 1 step to get the big room necessary
        //Loop thru the divisions array and find the ID of the parent of the subrooms... easier said than done

        List<Subroom> roomsMadeBig = new List<Subroom>();
        int numPairs = subrooms.Count/2;

        switch(bigRoomGenerationType)
        {
                case BigRoomGenerationType.ByChance:

                    Debug.Log("Doing By Chance");
                        for(int j = 0; j < divisions.Count; j++)
                        {   
                                if (!divisions[j].IAmEndLeaf())
                                {
                                    if (divisions[j].leftChild.IAmEndLeaf() && divisions[j].rightChild.IAmEndLeaf() && bigRoomSpawnChance > Random.Range(0, 100)) //if division is the parent of the selected rooms to be combined
                                    {
                                        //remove the childs from both divisions and subrooms list, and reinstate parent division as the subroom
                                        divisions.Remove(divisions[j].leftChild);
                                        divisions.Remove(divisions[j].rightChild);
                                        subrooms.Remove(divisions[j].leftChild);
                                        subrooms.Remove(divisions[j].rightChild);
                                        roomsMadeBig.Add(divisions[j]);

                                        Debug.Log($"leftID = {divisions[j].leftChild.debugID} and right ID = {divisions[j].rightChild.debugID} and subroom to add is {divisions[j].debugID}");
                                        //make the childs null so the path drawer won't draw paths because it won't pass the child null check
                                        divisions[j].leftChild = null;
                                        divisions[j].rightChild = null;

                                    }
                                }
                        }

                    foreach(Subroom bigRoom in roomsMadeBig) //add them on at the end so subrooms is fine
                    {
                        subrooms.Add(bigRoom);
                    }

                break;

                case BigRoomGenerationType.ByCount:

                    for (int i = 0; i < bigRoomCount; i++)
                    {
                        //bool canProceed = false;
                        
                        numPairs = subrooms.Count/2;
                        int pairToChange = Random.Range(0, numPairs);
                        int idOfLeft = subrooms[pairToChange * 2].debugID;
                        int idOfRight = subrooms[pairToChange * 2 + 1].debugID;

                        //Debug.Log(idOfLeft);
                        //Debug.Log(idOfRight);
                        
                    
                        for (int j = 0; j < divisions.Count; j++)
                        {   
                            if (!divisions[j].IAmEndLeaf())
                            {   
                                if (divisions[j].leftChild.debugID == idOfLeft && divisions[j].rightChild.debugID == idOfRight) //if division is the parent of the selected rooms to be combined
                                {
                                    //remove the childs from both divisions and subrooms list, and reinstate parent division as the subroom
                                    divisions.Remove(divisions[j].leftChild);
                                    divisions.Remove(divisions[j].rightChild);
                                    subrooms.Remove(divisions[j].leftChild);
                                    subrooms.Remove(divisions[j].rightChild);
                                    roomsMadeBig.Add(divisions[j]);

                                    Debug.Log($"leftID = {divisions[j].leftChild.debugID} and right ID = {divisions[j].rightChild.debugID} and subroom to add is {divisions[j].debugID}");

                                    //make the childs null so the path drawer won't draw paths because it won't pass the child null check
                                    divisions[j].leftChild = null;
                                    divisions[j].rightChild = null;
                                }
                            } 
                        }
            
                    }
                
                    foreach(Subroom bigRoom in roomsMadeBig) //add them on at the end so subrooms is fine
                    {
                        subrooms.Add(bigRoom);
                    }

                break;
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
        }
    }

    public void CreateDivisions(Subroom parentRoom)
    {
        
        if(parentRoom.divisionRect.width/2 <= minDivisionWidth || parentRoom.divisionRect.height/2 <= minDivisionHeight) //Stop recursion if the next room split would make the subsequent rooms smaller than the minimum size
        {
           // Debug.Log("Subroom " + parentRoom.debugID + " is a leaf!!");


            return;
        }

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
    

            //Create the subrooms
            if (splitH)
            {
                parentRoom.leftChild = new Subroom(new Rect(parentRoom.divisionRect.x, parentRoom.divisionRect.y, parentRoom.divisionRect.width / 2, parentRoom.divisionRect.height));
                parentRoom.rightChild = new Subroom(new Rect(parentRoom.divisionRect.x + parentRoom.divisionRect.width / 2, parentRoom.divisionRect.y, parentRoom.divisionRect.width / 2, parentRoom.divisionRect.height));
                parentRoom.leftChild.splitH = true;
                parentRoom.rightChild.splitH = true;
            }
            else if(!splitH)
            {
                parentRoom.leftChild = new Subroom(new Rect(parentRoom.divisionRect.x, parentRoom.divisionRect.y, parentRoom.divisionRect.width, parentRoom.divisionRect.height/2));
                parentRoom.rightChild = new Subroom(new Rect(parentRoom.divisionRect.x, parentRoom.divisionRect.y + parentRoom.divisionRect.height / 2, parentRoom.divisionRect.width, parentRoom.divisionRect.height/2));
                parentRoom.leftChild.splitH = false;
                parentRoom.rightChild.splitH = false;
            }
            else
            {
                return;
            }

            //Debug.Log("The left child of " + parentRoom.divisionRect + " is " + parentRoom.leftChild.divisionRect);
            //Debug.Log("The right child of " + parentRoom.divisionRect + " is " + parentRoom.rightChild.divisionRect);
            divisions.Add(parentRoom.leftChild);
            divisions.Add(parentRoom.rightChild);

            CreateDivisions(parentRoom.leftChild);
            CreateDivisions(parentRoom.rightChild);
    }

    public void CreatePathwayBetween(Subroom left, Subroom right) 
    {

        //
        // TODO
        // Randomise path placement a little bit?
        //
        float distance = 0;
        Vector2 startingPoint = Vector2.zero;
        Vector2 lPoint;
        Vector2 rPoint;

        lPoint = new Vector2(left.divisionRect.x + left.divisionRect.width/2, left.divisionRect.y + left.divisionRect.height / 2);
        rPoint = new Vector2(right.divisionRect.x + right.divisionRect.width/2, right.divisionRect.y + right.divisionRect.height /2);

        //if the two rooms are subleafs connect directly, otherwise connect the divisions

        if(left.IAmEndLeaf() && right.IAmEndLeaf())
        {
            lPoint = new Vector2(left.containedRoom.rect.x + left.containedRoom.rect.width/2, left.containedRoom.rect.y + left.containedRoom.rect.height/2);
            rPoint = new Vector2(right.containedRoom.rect.x + right.containedRoom.rect.width/2, right.containedRoom.rect.y + right.containedRoom.rect.height/2);
        }
        else
        {   
            Rect leftMostChildRect = left.GetRoom();
            Rect rightMostChildRect = right.GetRoom();
            lPoint = new Vector2(leftMostChildRect.x + leftMostChildRect.width/2, leftMostChildRect.y + leftMostChildRect.height / 2);
            rPoint = new Vector2(rightMostChildRect.x + rightMostChildRect.width/2, rightMostChildRect.y + rightMostChildRect.height /2);
        }


        if(lPoint.x > rPoint.x && splitH)
        {
            Vector2 temp = lPoint;
            lPoint = rPoint;
            rPoint = temp;
        }
        
        if(lPoint.y > rPoint.y && !splitH)
        {
            Vector2 temp = lPoint;
            lPoint = rPoint;
            rPoint = temp;
        }

        float pathDistance = 0; 

       // Debug.Log($"L Point is {lPoint} and right point is {rPoint} //// pathDistance is {pathDistance} and corridorWidth is {corridorWidth}");
        Rect pathPt1 = new Rect(0,0,0,0);
        Rect pathPt2 = new Rect(0,0,0,0);

        if(left.splitH) //Make a horizontal path
        {
            pathDistance = rPoint.x - lPoint.x + corridorWidth/2;
            pathPt1 = new Rect(lPoint.x, lPoint.y - corridorWidth/2, pathDistance, corridorWidth); //directly from midpoint to midpoint

                if(lPoint.y < rPoint.y) //if the path is above the connecting room
                {
                    distance = lPoint.y - rPoint.y;
                    pathPt2 = new Rect(rPoint.x - corridorWidth/2, lPoint.y - distance - corridorWidth/2, corridorWidth, distance); 

                }
                else //if the path is below the connecting room
                {
                    distance = rPoint.y - lPoint.y;
                    pathPt2 = new Rect(rPoint.x - corridorWidth/2, rPoint.y - distance + corridorWidth/2, corridorWidth, distance);
                }
            
            //do nothing if the path is spot on already
        }
        else //Make a vertical path
        {   
            pathDistance = rPoint.y - lPoint.y + corridorWidth/2;
            pathPt1 = new Rect(lPoint.x - corridorWidth/2, lPoint.y, corridorWidth, pathDistance); //directly from midpoint to midpoint
            //Debug.Log(lPoint);
            //Debug.Log(rPoint);
            
            //Do a conjoining path just in case;
                if(lPoint.x < rPoint.x) //if end of vertical path is to the left of the rPoint
                {
                    //make second path
                    distance = rPoint.x - lPoint.x;
                    pathPt2 = new Rect(lPoint.x - corridorWidth/2, rPoint.y - corridorWidth/2, distance, corridorWidth);
                }
                else if(lPoint.x > rPoint.x) //if end of vertical path is to the right of the rPoint;
                {   
                    //make second path   
                    distance = lPoint.x - rPoint.x;

                    pathPt2 = new Rect(rPoint.x, rPoint.y - corridorWidth/2, distance + corridorWidth/2, corridorWidth);
                }
            
            //do nothing if the path is spot on already
        }
      
        paths.Add(pathPt1);
        if(pathPt2.height >= corridorWidth || pathPt2.width >= corridorWidth) //only add the path if the path is of a big enough size to matter
        { 
            paths.Add(pathPt2);
        }
       
        
    }

    void DrawEveryRectangle()
    {
        //draw base first
        //then draw paths
        //then draw rooms

        //delete all objects under parents;

        foreach(Transform child in baseParent.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        foreach(Transform child in pathsParent.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        foreach(Transform child in roomsParent.transform)
        {
            GameObject.Destroy(child.gameObject);
        }


        GameObject background = Instantiate(rectanglePrefab, baseParent.transform);
        background.transform.localScale = new Vector3(baseWidth, baseHeight, 1);
        background.GetComponent<SpriteRenderer>().color = baseColour;
        background.GetComponent<SpriteRenderer>().sortingOrder = drawOrder.IndexOf("base");
        background.name = "base rectangle";
        if(scaleCameraSpeedByBaseSize)
        {
            GameObject.Find("Main Camera").GetComponent<CameraMove>().baseGenerated((int)(baseWidth + baseHeight)/2);
        }
      
        Vector3 BGPos = background.transform.position;
        BGPos.x += baseWidth/2;
        BGPos.y -= baseHeight/2;
        background.transform.position = BGPos;

        //drawing paths

        for(int i = 0; i < paths.Count; i++)
        {
            GameObject path = Instantiate(rectanglePrefab, pathsParent.transform);
            path.transform.localScale = new Vector3(paths[i].width, paths[i].height, 1);
            path.GetComponent<SpriteRenderer>().color = pathColour;
            path.GetComponent<SpriteRenderer>().sortingOrder = drawOrder.IndexOf("paths");
            Vector3 pathPos = path.transform.localPosition;
            pathPos.x = paths[i].x + paths[i].width/2 - startingX;
            pathPos.y = -paths[i].y - paths[i].height/2 + startingY;
            path.transform.localPosition = pathPos;
        }  

        int count = 0;
        for(int j = 0; j < rooms.Count; j++)
        {
            //Debug.Log(rooms[j].divisionRect);
            GameObject room = Instantiate(rectanglePrefab, roomsParent.transform);
            room.transform.localScale = new Vector3(rooms[j].rect.width, rooms[j].rect.height, 1);
            room.name = "Room " + count;
            count++;
            room.GetComponent<SpriteRenderer>().color = randomColors[j];
            room.GetComponent<SpriteRenderer>().sortingOrder = drawOrder.IndexOf("rooms");
            Vector3 roomPos = room.transform.localPosition;
            roomPos.x = rooms[j].rect.x + rooms[j].rect.width/2 - startingX;
            roomPos.y = -rooms[j].rect.y - rooms[j].rect.height/2 + startingY;
            room.transform.localPosition = roomPos;

        }
        roomCount.text = "Number Of Rooms: " + count;

        count = 0;
        if(drawDivisions)
        {
            for(int k = 0; k < subrooms.Count; k++)
            {
                //Debug.Log(subrooms[k].divisionRect);
                GameObject subroom = Instantiate(rectanglePrefab, roomsParent.transform);
                subroom.transform.localScale = new Vector3(subrooms[k].divisionRect.width, subrooms[k].divisionRect.height, 1);
                subroom.name = "Division " + count;
                count++;
                subroom.GetComponent<SpriteRenderer>().color = randomColors[k];
                subroom.GetComponent<SpriteRenderer>().sortingOrder = drawOrder.IndexOf("divisions");
                Vector3 roomPos = subroom.transform.localPosition;
                roomPos.x = subrooms[k].divisionRect.x + subrooms[k].divisionRect.width / 2 - startingX;
                roomPos.y = -subrooms[k].divisionRect.y - subrooms[k].divisionRect.height / 2 + startingY;
                subroom.transform.localPosition = roomPos;

            }
        }
        

    }

    void OnGUI()
    {
        //Draw the background
       if(drawDebugUI)
       {
          
       
        int colorIndex = 0;
        if(stump != null)
        {
            EditorGUI.DrawRect(new Rect(stump.divisionRect.x, stump.divisionRect.y, stump.divisionRect.width, stump.divisionRect.height), baseColour);
        }

        if(!drawPathsOverRooms) //Draw normally, unless debug draw mode is on
        {
            foreach(Rect path in paths)
            {
                EditorGUI.DrawRect(new Rect(path.x, path.y, path.width, path.height), pathColour);
            }
            colorIndex = 0;

           
            if(drawDivs){
                foreach (Subroom div in subrooms)
                {
                    if(div != null)
                    {
                        EditorGUI.DrawRect(new Rect(div.divisionRect.x, div.divisionRect.y, div.divisionRect.width, div.divisionRect.height), randomColors[colorIndex]);
                        colorIndex++;
                    }
                  
                };
            }

            colorIndex = 0;
            foreach(Room room in rooms)
            {
                if (room != null && randomColors[colorIndex] != null)
                {
                    EditorGUI.DrawRect(new Rect(room.rect.x, room.rect.y, room.rect.width, room.rect.height), randomColors[colorIndex]);
                    colorIndex++;
                }
            }
        }
        else //draw paths over everything
        {
            colorIndex = 0;
            if(drawDivs){
                foreach (Subroom div in subrooms)
                {
                    EditorGUI.DrawRect(new Rect(div.divisionRect.x, div.divisionRect.y, div.divisionRect.width, div.divisionRect.height), randomColors[colorIndex]);
                    colorIndex++; 
                };
            }

            colorIndex = 0;
            foreach(Room room in rooms)
            {
                if(randomColors[colorIndex] != null)
                {
                    EditorGUI.DrawRect(new Rect(room.rect.x, room.rect.y, room.rect.width, room.rect.height), randomColors[colorIndex]);
                    colorIndex++;
                }
            }

            foreach(Rect path in paths)
            {
                EditorGUI.DrawRect(new Rect(path.x, path.y, path.width, path.height), pathColour);
            }
        }
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

        public Room containedRoom;

        public Rect divisionRect;

        public bool splitH;

        public Subroom(Rect baseRect)
        {
            divisionRect = baseRect;
            debugID = debugCounter;
            debugCounter++;
        }

        public Rect GetRoom()
        {
            if(IAmEndLeaf())
            {
                return containedRoom.rect;
            }

            if(leftChild != null)
            {
                Rect lroom = leftChild.GetRoom();
                return lroom;
            }

            if(rightChild != null)
            {
                Rect rroom = rightChild.GetRoom();
                return rroom; 
            }
            return new Rect(-1, -1, 0, 0);
        }

        public bool IAmEndLeaf()
        {
            return leftChild == null && rightChild == null;
        }

        public bool ContainsARoom()
        {
            return containedRoom != null;
        }
    }
}
