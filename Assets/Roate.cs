using UnityEngine;

public class Roate : MonoBehaviour
{
    public float speed = 10f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MeshRenderer meshf = GetComponent<MeshRenderer>();
        meshf.material.color = Random.ColorHSV();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 rotate = new Vector3(0, speed * Time.deltaTime * Random.value, 0);
        transform.Rotate(rotate, Space.World);
    }
}
