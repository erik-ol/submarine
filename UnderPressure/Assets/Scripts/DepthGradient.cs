using UnityEngine;

public class DepthGradient : MonoBehaviour
{

    public Camera Cam;
    public Transform sub; //unused for now


    public float Y_min = 0.0f;
    public float Y_max = 15.0f;

    public Color TopCol = new Color(0.3f, 0.3f, 0.3f);
    public Color BotCol = new Color(0.3f, 0.3f, 0.9f);

    Color col;

    Material mat;
    float t; // color
    float dis; // y-distance
    float height; //height of quad
    float width; //width of quad

    void Start(){
        //Debug.LogError("START RAN");
        mat = GetComponent<Renderer>().material;
        dis = Cam.farClipPlane - 1.0f;
        FitCamera();
    }

    void FitCamera(){
        transform.position = Cam.transform.position + Cam.transform.forward * dis;
        height = 2 * dis * Mathf.Tan(Cam.fieldOfView/2*Mathf.Deg2Rad);
        width = height * Cam.aspect;
        transform.localScale = new Vector3(width, height, 1.0f);
    }

    void Update(){
        t = Mathf.InverseLerp(Y_min,Y_max,Cam.transform.position.y);
        col = Color.Lerp(TopCol,BotCol,t);
        mat.SetColor("_TopColor", col);
        mat.SetColor("_BottomColor", col);
        //Debug.LogError("TEST: " + Cam.transform.position.y);
    }
}
