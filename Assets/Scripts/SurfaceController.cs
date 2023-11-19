using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceController : MonoBehaviour
{

    public float maxHeight = 10;
    public float scale = 0.1f;

    public Vector2Int mapSize = new Vector2Int(10,10);
    public Vector2 cliffSize = new Vector2(2,2);

    public Transform prefab;

    public Transform meshTransform;
    public Transform cliffParent;

    float perlinX, perlinY;
    Dictionary<Vector2Int, Transform> cliffs = new Dictionary<Vector2Int, Transform>();

    void Start()
    {
        perlinX = Random.value * scale;
        perlinY = Random.value * scale;

        for (int x = 0; x < mapSize.x; x++)
            for (int y = 0; y < mapSize.y; y++)
            {
                Vector3 sc = Vector3.one;
                sc.y = Mathf.Clamp(Mathf.PerlinNoise(perlinX + ((float)x / mapSize.x) * scale, perlinY + ((float)y / mapSize.y) * scale) * maxHeight, 0.01f, maxHeight);

                Transform cliff = Instantiate(prefab, cliffParent);
                cliff.localPosition = new Vector3(cliffSize.x * x, 0, cliffSize.y * y);
                cliff.localScale = sc;
                cliff.gameObject.SetActive(true);

                int angle = Random.Range(0, 4);
                cliff.Find("Top").eulerAngles = new Vector3(0, angle * 90, 0);

                cliffs.Add(new Vector2Int(x, y), cliff);
            }


        //TODO make plato

        int px = Random.Range(0 + 20, mapSize.x - 10);
        int py = Random.Range(0 + 20, mapSize.y - 10);

        float ls = cliffs[new Vector2Int(px, py)].localScale.y;
        Vector3 lScale = new Vector3(1, ls, 1);
        for (int x = 0; x < 5; x++)
            for(int y = 0; y < 5; y++)
            {
                cliffs[new Vector2Int(x + px, y + py)].localScale = lScale;
                cliffs[new Vector2Int(x + px, y + py)].Find("Top").gameObject.SetActive(false);
            }

        CompainMeshes();
    }

    void CompainMeshes()
    {
        Dictionary<int, List<CombineInstance>> combined = new Dictionary<int, List<CombineInstance>>();

        int vertex = 0;
        int step = 0;
        
        combined.Add(step, new List<CombineInstance>());

        foreach (Transform cliff in cliffs.Values)
        {
            MeshFilter[] meshes = cliff.GetComponentsInChildren<MeshFilter>();
            int i = 0;
            while (i < meshes.Length)
            {
                if (meshes[i].gameObject.activeInHierarchy)
                {
                    combined[step].Add(new CombineInstance()
                    {
                        mesh = meshes[i].sharedMesh,
                        transform = meshes[i].transform.localToWorldMatrix
                    });
                    meshes[i].gameObject.SetActive(false);

                    vertex += meshes[i].sharedMesh.vertexCount;
                    if (vertex > 65000)
                    {
                        step++;
                        combined.Add(step, new List<CombineInstance>());
                        vertex = 0;
                    }
                }
                i++;
            }
        }
        

        foreach (int i in combined.Keys)
        {
            Mesh mesh = new Mesh();
            mesh.CombineMeshes(combined[i].ToArray());

            Transform parent = Instantiate(meshTransform, transform);
            parent.GetComponent<MeshFilter>().sharedMesh = mesh;
            parent.GetComponent<MeshCollider>().sharedMesh = mesh;
            parent.gameObject.SetActive(true);
        }

    }
}
