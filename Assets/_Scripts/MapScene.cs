using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MapScene : MonoBehaviour
{
    public float accuracy;
    public float maxX;
    public float maxY;
    public float minX ;
    public float minY ;
    public List<Vector3> MappedPoints;

    [SerializeField]
    public List<Node> Bitches_here = new List<Node>();
    private int layerMask;

    public GameObject StartPoint,EndPoint;

    private Node GetPath(Node Current_Node)
    {
        if (Current_Node == null)
        {
            Debug.Log("Its Null");
            return null;
        } else
        {
            Debug.DrawRay(Current_Node.position, Vector3.up * 10, Color.black, 100f);
            Bitches_here.Add(Current_Node);
            if(Current_Node.parent == null)
            {
                return null;
            } else
            {
                Current_Node = Current_Node.parent;
                return GetPath(Current_Node.parent);
            }   
        }
    }

    public class Node {
        public Node parent = null;

        public float g;
        public float h;
        public float f;
        
        public Vector3 position;
        public Node(){
            this.f = 0;
        }
        public Node(Vector3 pos, Node parent = null){
            this.f = 0;
            this.h = 0;
            this.g = 0;
            this.position = pos;
            this.parent = parent;
        }	
    }

    void Start()
    {
        layerMask = LayerMask.NameToLayer("Ground");
        Map();
        FindPathBetween(StartPoint,EndPoint);
    }

    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {

            Map();
            FindPathBetween(StartPoint, EndPoint);
        }
    }

    public void Map(){        
        MappedPoints.Clear();
        for(float x=minX;x<maxX;){
           for(float y=minY;y<maxY;){
                //Do raycasy at pos x,y(alias for actual z) and position down
                Vector3 position = new Vector3(x,2,y);

                if(checkSurrounding(position)){
                    var newPos = new Vector3(position.x,0f, position.z);
                    MappedPoints.Add(newPos);

                }

                //Increment Y
                y += accuracy;
                
           }

            //increment X 
            x += accuracy;
           
        }
    }
    
    public bool checkSurrounding(Vector3 position){
        Vector3 Start_Point = new Vector3(position.x, 3f, position.z);
        var colliding = Physics.CheckCapsule(Start_Point, new Vector3(position.x,2,position.z), 1.5f);
        return !colliding;
    }

    public void FindPathBetween(GameObject Start,GameObject End){
        if(Start == null || End == null){
            Debug.LogError("Start or End point are not set");
            return;
        }
        
        Vector3 Start_Position = Start.transform.position;
        Start_Position.y = 0;

        Vector3 End_Position = End.transform.position;
        End_Position.y = 0;

        //convert all Points into 
        Node Start_Point = new Node(Start_Position, null);
        Node End_Point = new Node(End_Position, null);

        List<Node> Open_Nodes = new List<Node>();
        List<Node> Closed_Nodes = new List<Node>();

        Open_Nodes.Add(Start_Point);
        Node Current_Node = Open_Nodes.First();

        int index = 0;
        while (Open_Nodes.Count() > 0){

            Current_Node = Open_Nodes.First();
            foreach (var node in Open_Nodes)
            {
                if (node.f < Current_Node.f)
                {
                    Current_Node = node;
                }
            }
            

            Open_Nodes.Remove(Current_Node);
            Closed_Nodes.Add(Current_Node);
            
            //check goal condition //use approx
            if(Vector3.Distance(Current_Node.position,End_Point.position) <= accuracy)
            {
                Debug.Log("NIGGA WE MADE IT");
                break;
            }

            //get points within {range} of Current_Node
            List<Node> Near_Current = new List<Node>();

            var search_distance = Mathf.Sqrt((accuracy * accuracy)*2);
            foreach (var point in MappedPoints.Where(n => Vector3.Distance(n, Current_Node.position) <= search_distance).ToList())
            {
                Near_Current.Add(new Node(point, Current_Node));         
            }

            foreach (Node node in Near_Current) {

                if (Closed_Nodes.Any(n => n.position == node.position)) {
                    continue;
                }            
            
                node.g = Current_Node.g + 1;
                node.h = Vector3.Distance(node.position, End_Position);
                node.f = node.g + node.h;                

                if (Open_Nodes.Any(x => x.position == node.position && node.g > x.g))
                {
                    continue;
                }

                Open_Nodes.RemoveAll(x => x.position == node.position);

                Open_Nodes.Add(node);
                index++;
                if (index > 1000) break;
            }

        }
        //End
        Debug.Log(Open_Nodes.Count() + " - " + Closed_Nodes.Count());
        Debug.Log(Current_Node.position);
        GetPath(Current_Node);
    }

}
