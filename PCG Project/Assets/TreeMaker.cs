using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using Unity.Jobs;
using System.IO.Compression;

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

    [Header("Base settings")]
    [Tooltip("X position of the rect containing all of the generated content")] public int startingX = 0;
    [Tooltip("Y position of the rect containing all of the generated content")] public int startingY = 0;
    [Tooltip("Width of the rect containing all of the generated content. Larger value results in more rooms (if minimum division size is the same)")] public int baseWidth = Screen.width;

    [Tooltip("Height of the rect containing all of the generated content. Larger value results in more rooms (if minimum division size is the same)")]
    public int baseHeight = Screen.height;

    [Header("Generation Settings")]
    
    [Header("Minimum division size. A room is generated based on the division sizes")]

    [Tooltip("Will show the divisions a.k.a the room's parent rather than the rooms")] public bool drawDivs;
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
    [Tooltip("Width of the corridor")] public int corridorWidth;

    [Tooltip("Colour of the corridors")] public Color corridorColour;

    [Tooltip("Paths are visible over rooms and other debug tools")] public bool drawPathsOverRooms;

    public enum BigRoomGenerationType
    {
        ByCount,
        ByChance,
    };

    [Header("Big room settings: A.k.a Double the size of a normal room")]
    [Tooltip("Choose whether the generator goes with manual number of big rooms, or % chance for it to spawn.")] public BigRoomGenerationType bigRoomGenerationType;
    [Tooltip("Number of big rooms. A big room combines two child rooms into one room. The sizes may vary based on the size of the children")] public int bigRoomCount = 1;

    private int bigRoomCounter = 0;
    [Tooltip("Chance for a big room to spawn. A big room combines two child rooms into one room. The sizes may vary based on the size of the children")] [Range(0,100)] public float bigRoomSpawnChance = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        //BigRoomGenerationType bigRoomGenType;

        RegenerateRooms();

       
        foreach (Subroom div in subrooms)
        {
            //Debug.Log(div.debugID + " width is " + div.divisionRect);
            //Debug.Log(div.debugID + " height is " + div.divisionRect);
        };


        //CreateTwoRooms(leafRoom1, leafRoom2);
    }

    public void RegenerateRooms()
    {
        debugCounter = 0;
        subrooms = new List<Subroom>();
        divisions = new List<Subroom>();
        randomColors = new List<Color>();
        paths = new List<Rect>();
        
        stump = new Subroom(new Rect(startingX, startingY, baseWidth, baseHeight));
        divisions.Add(stump);
        CreateDivisions(stump);
     
        //generate endleaf list and edit that to make big rooms
        AddRoomsToList(stump);
        MakeBigRooms();

        CreateRoomInSubrooms();
        //Make random colours
        for (int i = 0; i < rooms.Count; i++)
        {
            randomColors.Add(Random.ColorHSV(0f, 1f));
        }
        //Debug.Log("");
        //CreatePathwayBetween();
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
       // Debug.Log("");
        //Debug.Log(paths.Count);
        //CreatePathwayBetween(stump.leftChild, stump.rightChild);

       // Debug.Log(rooms.Count);
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
            //Debug.Log($"KEY it {div.divisionRect.x} Yeah yeah yeah and the maximum is ran {roomX} lol {roomY}");
            Rect roomRect = new(roomX, roomY, roomWidth, roomHeight);
            Room room = new Room(roomRect);
            //Debug.Log($"Inputs {roomRect}");
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
            bigRoomCounter = 0;
            RegenerateRooms();
        }

        if(Input.GetButtonDown("Jump"))
        {
            CreateRoomInSubrooms();
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
            
        Debug.Log(roomsMadeBig.Count);
        Debug.Log("Subroom count is " + subrooms.Count);
        Debug.Log("");
        
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

    public void CreateDivisions(Subroom parentRoom)
    {
         
        if(parentRoom.divisionRect.width/2 < minDivisionWidth || parentRoom.divisionRect.height/2 < minDivisionHeight) //Stop recursion if the next room split would make the subsequent rooms smaller than the minimum size
        {
           // Debug.Log("Subroom " + parentRoom.debugID + " is a leaf!!");


            return;
        }

        //Add a random chance for big rooms up to the total of bigRooms specified by user
        //if (Random.Range(0, 1) <= bigRoomSpawnChance && parentRoom.divisionRect.width/bigRoomSizeMultiplier < minDivisionWidth && parentRoom.divisionRect.height/bigRoomSizeMultiplier < minDivisionHeight && bigRoomCounter < bigRoomCount) 
        //{
            //Debug.Log("Subroom " + parentRoom.debugID + " is a leaf AND A BIG ROOM");
            //bigRoomCounter++;
            //return;
       // }
        //else
        //{
            //if(bigRoomSpawnChance < 1 && bigRoomCounter < bigRoomCount) //if its not big room increase spawn chance for next one so the big room count is equal to amount specified
            //{
             //   bigRoomSpawnChance += bigRoomIncrease;
            //    Debug.Log("BIG " + bigRoomSpawnChance);
            //}
        
        //}
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
           // Debug.Log("The right child of " + parentRoom.divisionRect + " is " + parentRoom.rightChild.divisionRect);
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

        //make the paths travel based off the division rect rather than the room rect. Just cos it broken lol
        // if(left.containedRoom != null) 
        // {
           
            // lPoint = new Vector2(left.containedRoom.rect.x + left.containedRoom.rect.width/2, left.containedRoom.rect.y + left.containedRoom.rect.height / 2);
            // rPoint = new Vector2(right.containedRoom.rect.x + right.containedRoom.rect.width / 2, right.containedRoom.rect.y + right.containedRoom.rect.height / 2);
        // }
        // else    //if the parent doesn't have a pair of leafs as kids 
        // {
            
           
        // }

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
            pathDistance = rPoint.x - lPoint.x;
            pathPt1 = new Rect(lPoint.x, lPoint.y + corridorWidth/2, pathDistance, corridorWidth); //directly from midpoint to midpoint

                 if(lPoint.y < rPoint.y) //if the path is above the connecting room
                 {
                     Vector2 temp = new Vector2(lPoint.x + pathDistance, lPoint.y - corridorWidth/2); 
                     distance = Vector2.Distance(temp, rPoint);
                     //Debug.Log("Key + " + distance + "   ////   " + temp);
                     startingPoint = new Vector2(temp.x - corridorWidth, lPoint.y + corridorWidth/2);
                     //Debug.Log("Key " + startingPoint);
                     pathPt2 = new Rect(startingPoint.x, startingPoint.y, corridorWidth, distance);

                 }
                 else //if the path is below the connecting room
                 {
                     Vector2 temp = new Vector2(lPoint.x + pathDistance, lPoint.y + corridorWidth/2);
                     distance = Vector2.Distance(temp, rPoint);
                     startingPoint = new Vector2(temp.x - corridorWidth, lPoint.y - distance + corridorWidth/2); //got lost somewhere along the way with the maths I was doing. 3*corridorWidth/2 just seems to fix it.
                     pathPt2 = new Rect(startingPoint.x, startingPoint.y, corridorWidth, distance);
                 }
            
            //do nothing if the path is spot on already
        }
        else //Make a vertical path
        {   
            pathDistance = rPoint.y - lPoint.y;
            pathPt1 = new Rect(lPoint.x - corridorWidth/2, lPoint.y + corridorWidth/2, corridorWidth, pathDistance); //directly from midpoint to midpoint
            
            
            //Do a conjoining path just in case;
                 if(lPoint.x < rPoint.x) //if end of vertical path is to the left of the rPoint
                 {
                     //make second path
                     Vector2 temp = new Vector2(lPoint.x - corridorWidth/2, lPoint.y + pathDistance);
                    distance = Vector2.Distance(temp, rPoint);
                     startingPoint = new Vector2(temp.x, temp.y - corridorWidth); 
                    
                    pathPt2 = new Rect(startingPoint.x, startingPoint.y, distance, corridorWidth);
                 }
                 else if(lPoint.x > rPoint.x) //if end of vertical path is to the right of the rPoint;
                 {
                     //make second path
                     Vector2 temp = new Vector2(lPoint.x + corridorWidth/2, lPoint.y + pathDistance);
                     distance = Vector2.Distance(temp, rPoint);
                     startingPoint = new Vector2(temp.x - distance - corridorWidth, temp.y - corridorWidth); 
                    
                     pathPt2 = new Rect(startingPoint.x, startingPoint.y, distance, corridorWidth);
                 }
            
            //do nothing if the path is spot on already
        }
      
        paths.Add(pathPt1);
        if(pathPt2.height >= corridorWidth || pathPt2.width >= corridorWidth) //only add the path if the path is of a big enough size to matter
        { 
            paths.Add(pathPt2);
        }
       
        
    }

    void OnGUI()
    {
        //Draw the background
       
        int colorIndex = 0;
        if(stump != null)
        {
            EditorGUI.DrawRect(new Rect(stump.divisionRect.x, stump.divisionRect.y, stump.divisionRect.width, stump.divisionRect.height), Color.white);
        }

        if(!drawPathsOverRooms) //Draw normally, unless debug draw mode is on
        {
            foreach(Rect path in paths)
            {
                EditorGUI.DrawRect(new Rect(path.x, path.y, path.width, path.height), corridorColour);
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
                if (room != null)
                {
                    EditorGUI.DrawRect(new Rect(room.rect.x, room.rect.y, room.rect.width, room.rect.height), Color.red);
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
                EditorGUI.DrawRect(new Rect(room.rect.x, room.rect.y, room.rect.width, room.rect.height), randomColors[colorIndex]);
                colorIndex++;
            }

            foreach(Rect path in paths)
            {
                EditorGUI.DrawRect(new Rect(path.x, path.y, path.width, path.height), corridorColour);
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
