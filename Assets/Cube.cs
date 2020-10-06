using UnityEngine;

public class Cube : MonoBehaviour
{

    public string Address;
    private GameObject _cube;
    public float X,Y,Z;
    public float speed = 1.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        _cube = GameObject.Find("NetworkMan");
    }

    public void deleteCube()
    {
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        X = this.transform.position.x;
        Y = this.transform.position.y;
        Z = this.transform.position.z;
        if (_cube.GetComponent<NetworkMan>().myAddress == Address)
        {
            _cube.GetComponent<NetworkMan>().cubeX = X;
            _cube.GetComponent<NetworkMan>().cubeY = Y;
            _cube.GetComponent<NetworkMan>().cubeZ = Z;
            if (Input.GetKey(KeyCode.UpArrow))
            {
                //Debug.Log("Pressed");
                transform.Translate(Vector3.forward* Time.deltaTime * speed);
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                //Debug.Log("Pressed");

                transform.Translate(-Vector3.forward * Time.deltaTime * speed);
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
               // Debug.Log("Pressed");

                transform.Translate(Vector3.left * Time.deltaTime * speed);
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
               //Debug.Log("Pressed");

                transform.Translate(-Vector3.left * Time.deltaTime * speed);
            }
        }
    }
}
