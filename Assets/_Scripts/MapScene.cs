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
        if (Current_Node.parent == null)
        {
            Debug.Log("Its Null");
            return null;
        } else
        {
            Debug.Log("Parent Found!");
            Debug.DrawRay(Current_Node.position, Vector3.up * 10, Color.red, 100f);
            Bitches_here.Add(Current_Node);
            Current_Node = Current_Node.parent;
            return GetPath(Current_Node.parent);
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
        }	
    }

    void Start()
    {
        layerMask = LayerMask.NameToLayer("Ground");
        Map();
        FindPathBetween(StartPoint,EndPoint);
    }

    public void Map(){        
        MappedPoints.Clear();
        for(float x=minX;x<maxX;){
           for(float y=minY;y<maxY;){
                //Do raycasy at pos x,y(alias for actual z) and position down
                Vector3 position = new Vector3(x,10,y);
                bool registered = false;
                RaycastHit hit;
                if (Physics.Raycast(position,Vector3.down, out hit, Mathf.Infinity))
                {                    
                    if (hit.transform.gameObject.layer == layerMask) {
                        if(checkSurrounding(position)){
                            var newPos = new Vector3(hit.point.x,0f,hit.point.z);
                            MappedPoints.Add(newPos);
                            registered = true;
                            //Debug.DrawRay(position,Vector3.down * hit.distance, Color.red,10f);
                        }
                    }                  
                }

                //Increment Y
                if(registered){
                    y+= accuracy;
                } else {
                    y+= accuracy/5f;
                }
                           
           }

           //increment X 
           x+= accuracy;
           
        }
    }
    
    public bool checkSurrounding(Vector3 position){
                
        var colliding = Physics.CheckCapsule(new Vector3(position.x,0.5f,position.z), new Vector3(position.x,2,position.z), accuracy,9);
        //Debug.DrawRay(position, Vector3.up * 15, Color.blue, 100f);
        return !colliding;
    }

    
    void Update(){
        if(Input.GetButtonDown("Jump")){

            Map();
            FindPathBetween(StartPoint,EndPoint);
        }
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
        Node Current_Node = Start_Point;

        int index = 0;
        while (Open_Nodes.Count() > 0){

            Current_Node = Open_Nodes.OrderBy(node => node.f < Current_Node.f).First();
     
            Open_Nodes.Remove(Current_Node);
            Closed_Nodes.Add(Current_Node);
            
            //check goal condition //use approx
            if(Current_Node == End_Point){
                Debug.Log("NIGGA WE MADE IT");
                return;
            }

            //get points within {range} of Current_Node
            List<Node> Near_Current = new List<Node>();
           
            foreach (var point in MappedPoints.Where(n => Vector3.Distance(new Vector3(n.x, 0, n.z), Current_Node.position) < accuracy * 1.3f).ToList())
            {
                Near_Current.Add(new Node(point, Current_Node));

            }

            foreach (Node node in Near_Current){

                node.parent = Current_Node;
                if (Closed_Nodes.Any(n=>n.position == node.position)){
                    continue;
                }

                node.g = Current_Node.g + 1;
                node.h = Vector3.Distance(node.position, End_Position);
                node.f = node.g + node.h;

                if (Open_Nodes.Any(x => x == node && x.g > node.g)){
                    continue;
                }
                              
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
