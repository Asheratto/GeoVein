using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;



public class CubosLoader : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(GetCubos());
    }

    IEnumerator GetCubos()
    {
        string url = "https://localhost:7223/cubos";

        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;

            // Unity necesita esto para listas
            json = "{\"cubos\":" + json + "}";

            CuboList lista = JsonUtility.FromJson<CuboList>(json);

            foreach (var c in lista.cubos)
            {
                CrearCubo(c);
            }
        }
        else
        {
            Debug.LogError(request.error);
        }
    }

    void CrearCubo(Cubo c)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3(c.x, c.y, c.z);
        cube.transform.localScale = Vector3.one * c.size;
    }
}

