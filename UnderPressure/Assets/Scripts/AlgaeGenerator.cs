using UnityEngine;
using System.Collections.Generic;
//should use System.Collections.Generic

public class AlgaeGenerator : MonoBehaviour
{
    private string algae;
    private string axiom = "B";
    [SerializeField] private int iterations;
    Stack<AlgaeTransformHelper> stack = new Stack<AlgaeTransformHelper>();
    private AlgaeTransformHelper helper;
    [SerializeField] private float length = 0.6f; 
    [SerializeField] private float angle = 30; //This should be what they used in the paper
    private Vector3 initialPosition; //not needed anymore?
    private List<List<Vector3>> LineList = new List<List<Vector3>>(); //for gizmos not needed now probably

    //prefabs for algae components
    [SerializeField] private GameObject branch;
    [SerializeField] private GameObject filament;
    [SerializeField] private GameObject stolon;



    void Start()
    {
        algae = axiom;
        Debug.Log("Starting algae");
        ExpandAlgaeString();
        CreateMesh();
    }

    private void OnDrawGizmos()
    {
        foreach(List<Vector3> line in LineList)
        {
            Gizmos.DrawLine(line[0], line[1]);
        }
    }

    
    void ExpandAlgaeString()
    {
        string expandedAlgae;  
        for (int i = 0; i < iterations; i++)
        {
            expandedAlgae = "";
            foreach (char j in algae)
            {
                float weightFactor = Random.Range(0.1f, 1.0f);
                switch(j)
                {
                    case 'B': 
                        if (weightFactor < 0.45f)
                            expandedAlgae += "B[-S]<[[F]/[F]]+"; 
                        else
                            expandedAlgae += "B[B]>[B]";
                        break;
                    case 'S': 
                        if (weightFactor<0.45f)
                            expandedAlgae += "[S]<[S]>[S]"; 
                        else
                            expandedAlgae += "[-S][-S]>[-S]";
                        break;
                    case 'F':
                        if (weightFactor < 0.4f)
                            expandedAlgae += "[F][B]";
                        else if (weightFactor >= 0.4f && weightFactor <0.9)
                            expandedAlgae += "[–B[F]-[F]-»[F]-\\[F]-/ / [F]-»[F]]";
                        else
                            expandedAlgae += "[B[F]-[F]-»[F]++\\[F]–[F]+-//[F]–[F]]";
                        break;
                    default: 
                        expandedAlgae += j.ToString(); 
                        break;
                }

            }
            algae = expandedAlgae;
            Debug.Log("Algae at iteration " + i + "is " + algae);
        }
    }
    void CreateMesh()
    {
        //Parent of algae components
        GameObject Algae = new GameObject(name: "Algae");
        
        //Using turtle geometry for moving
        GameObject turtle = new GameObject(name: "turtle");
        turtle.transform.position = transform.position;
        turtle.transform.rotation = transform.rotation;

        //moved this to top Vector3 initialPosition; 9:29
        foreach (char j in algae)
        {
            switch(j)
            {
                case 'B':
                    //turtle.transform.Translate(Vector3.up*length); 
                    GameObject Branch = Instantiate(branch, turtle.transform.position, turtle.transform.rotation, Algae.transform);
                    GameObject bBottom = Branch.transform.Find("bottom").gameObject;
                    // Calculate offset between prefab origin and bottom
                    Vector3 bOffset = Branch.transform.position - bBottom.transform.position;
                    // Move the entire prefab so its bottom matches the turtle position
                    Branch.transform.position += bOffset;

                    //make sure the turtle go to the top of the branch for next placement
                    GameObject top = Branch.transform.Find("Top").gameObject;
                    turtle.transform.position = top.transform.position;
                    break;

                case 'S': 
                    //turtle.transform.Translate(Vector3.up*length); 
                    GameObject Stolon = Instantiate(stolon, turtle.transform.position, turtle.transform.rotation, Algae.transform);
                    //prefabs pivot point is in the center so we need to fint the bottom
                    GameObject sBottom = Stolon.transform.Find("bottom").gameObject;
                    // Calculate offset between prefab origin and bottom
                    Vector3 sOffset = Stolon.transform.position - sBottom.transform.position;
                    // Move the entire prefab so its bottom matches the turtle position
                    Stolon.transform.position += sOffset;
                    //sBottom.transform.position = turtle.transform.position;
                    //turtle.transform.position = bottom.transform.position;
                    //nothing is placed on filaments or stolons top, so no need to change the turtles position in this case
                    break;

                case 'F':
                    //turtle.transform.Translate(Vector3.up*length); 
                    GameObject Filament = Instantiate(filament,turtle.transform.position, turtle.transform.rotation, Algae.transform);
                     //prefabs pivot point is in the center so we need to fint the bottom
                    GameObject fBottom = Filament.transform.Find("bottom").gameObject;
                    // Calculate offset between prefab origin and bottom
                    Vector3 fOffset = Filament.transform.position - fBottom.transform.position;
                    // Move the entire prefab so its bottom matches the turtle position
                    Filament.transform.position += fOffset;
                
                    break;

                    //Vector3.right - around x.axis, Vector3.up - Y axis Vector3.forward - Z axis.

                    //Have to check if paper has same axis as unity or if Y and Z should be switched
                case '+': //positive in z-axis
                    turtle.transform.Rotate(Vector3.forward, angle);
            
                    break;
                case '-': //negative in z-axis
                    turtle.transform.Rotate(Vector3.back, angle);

                    break;
                case '\\': //positive in x-axis changed from \ to \\ due to 
                    turtle.transform.Rotate(Vector3.right, angle);
                    break;
                case '/': //negative in x-axis
                    turtle.transform.Rotate(Vector3.left, angle);

                    break;
                case '<': //positive in y-axis
                    turtle.transform.Rotate(Vector3.up, angle);
                
                    break;
                case '>': //negative in y-azis
                    turtle.transform.Rotate(Vector3.down, angle);

                    break;
                case '[': //push (Create a new state)
                    stack.Push(new AlgaeTransformHelper()
                    {
                        position = turtle.transform.position,
                        rotation = turtle.transform.rotation
                    });
                
                    break;
                case ']': //Pop (return to the previous state)
                    AlgaeTransformHelper helper = stack.Pop();
                    turtle.transform.position = helper.position;
                    turtle.transform.rotation = helper.rotation;
                    break;

                Debug.Log("Nothing fit for char: " + j);

            }
        }
    }
}
