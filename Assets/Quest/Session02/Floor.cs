using UnityEngine;

public class Floor : MonoBehaviour
{
    public GameObject prefab;
    void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            Instantiate(prefab, new Vector3(Random.Range(-50, 50), 0.25f, Random.Range(-50, 50)), Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
