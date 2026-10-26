using UnityEngine;
using System.Collections.Generic; //should use System.Collections.Generic

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
                expandedAlgae += j switch
                {
                'B' => "B-[S]-[F]",
                'S' => "S",
                'F' => "BF", 
                _ => j.ToString()
                };
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
                    turtle.transform.Translate(Vector3.up*length); 
                    Instantiate(branch, turtle.transform.position, turtle.transform.rotation, Algae.transform);

                    break;

                case 'S': 
                    turtle.transform.Translate(Vector3.up*length); 
                    Instantiate(stolon, turtle.transform.position, turtle.transform.rotation, Algae.transform);
                    break;

                case 'F':
                    turtle.transform.Translate(Vector3.up*length); 
                    Instantiate(filament,turtle.transform.position, turtle.transform.rotation, Algae.transform);
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

            }
        }
    }
}
