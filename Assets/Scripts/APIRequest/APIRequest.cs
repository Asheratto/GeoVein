using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;


public class APIRequest : MonoBehaviour
{

    public Renderer objetoVisualizador; // Arrastra aquí el Plano o Cubo
    public static APIRequest Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(GetHeightMap());
    }

    public void PostTif(byte[] filebyte)
    {
        StartCoroutine(CPostTif(filebyte));
    }

    private IEnumerator CPostTif(byte[] filebyte)
    {

        string url = "http://localhost:5026/cubos/upload-dem";

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", filebyte, "dem.tif", "image/tiff");

        UnityWebRequest www = UnityWebRequest.Post(url, form);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Archivo enviado correctamente");
            Debug.Log(www.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error al enviar: " + www.error);
        }

    }

    IEnumerator GetHeightMap()
    {
        string url = "https://localhost:7223/cubos/dem/heightmap/1";
        UnityWebRequest www = UnityWebRequest.Get(url);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string json = www.downloadHandler.text;

            Debug.Log("JSON recibido: " + json);

            HeightmapResponse data = JsonConvert.DeserializeObject<HeightmapResponse>(json);

            if (data != null && data.valores != null)
            {
                // Convertimos de [][] a [,] que es lo que Terrain quiere
                float[,] grid = ConvertToHeights(data.valores, data.width, data.height);
                Apply3DGeometry(grid, data.width, data.height);
                //ApplyToImage(grid, data.width, data.height);
                Debug.Log("¡Terreno generado con éxito!");
            }
        }
        else
        {
            Debug.LogError("Error: " + www.error);
        }

    }

    float EncontrarMayorEnMatriz(float[,] matriz)
    {
        float mayor = float.MinValue; // El valor más pequeño posible para empezar

        for (int i = 0; i < matriz.GetLength(0); i++)
        {
            for (int j = 0; j < matriz.GetLength(1); j++)
            {
                if (matriz[i,j] > mayor) 
                {
                    mayor = matriz[i,j];
                }
            }
        }
        return mayor;
    }

    float EncontrarMenorMatriz(float[,] matriz)
    {
        float menor = float.MaxValue; // El valor más pequeño posible para empezar

        for (int i = 0; i < matriz.GetLength(0); i++)
        {
            for (int j = 0; j < matriz.GetLength(1); j++)
            {
                if (matriz[i, j] < menor)
                {
                    menor = matriz[i, j];
                }
            }
        }
        return menor;
    }

    void Apply3DGeometry(float[,] heights, int width, int height)
    {

        MeshFilter meshFilter = objetoVisualizador.GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // Para soportar muchos puntos

        Vector3[] vertices = new Vector3[width * height];
        int[] triangles = new int[(width - 1) * (height - 1) * 6];

        // 1. Crear Vértices (Aquí aplicamos la ALTURA)
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float h = heights[y, x] * 50f; // Multiplicamos por 50 para que el relieve se note
                vertices[y * width + x] = new Vector3(x, h, y);
            }
        }

        // 2. Crear Triángulos (La superficie)
        int tri = 0;
        for (int y = 0; y < height - 1; y++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                int i = y * width + x;
                triangles[tri++] = i;
                triangles[tri++] = i + width;
                triangles[tri++] = i + 1;
                triangles[tri++] = i + 1;
                triangles[tri++] = i + width;
                triangles[tri++] = i + width + 1;
            }
        }

        // 3. Asignar a la malla
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals(); // Esto hace que la luz rebote bien en el relieve

        meshFilter.mesh = mesh;

        /*/ --- PARTE 2: CREAR LA IMAGEN DE COLOR (LA TEXTURA) ---

        MeshRenderer meshRenderer = objetoVisualizador.GetComponent<MeshRenderer>();
        if (meshRenderer == null) meshRenderer = objetoVisualizador.AddComponent<MeshRenderer>();

        // Creamos la textura con el tamaño EXACTO (84x97)
        Texture2D tex = new Texture2D(width, height);
        tex.filterMode = FilterMode.Point; // Mantiene los bordes definidos estilo GIS
        tex.wrapMode = TextureWrapMode.Clamp;

        float valorMaximo = EncontrarMayorEnMatriz(heights);
        float valorMinimo = EncontrarMenorMatriz(heights);

        float v1 = (valorMaximo - valorMinimo)/10;
        

        // Recorremos la matriz para asignar colores según la altura
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float h = heights[y, x]; // Valor entre 0 y 1

                

                // Definimos el color según la profundidad/altura
                Color colorPixel;
                if (h < v1) colorPixel = Color.blue;          // Agua muy profunda
                else if(h < v1*2) colorPixel = Color.brown;    // Agua media
                else if (h < v1*5) colorPixel = Color.green;   // Terreno
                else colorPixel = Color.gray;                  // Cumbres/Roca

                tex.SetPixel(x, y, colorPixel);
            }
        }

        tex.Apply(); // Guardar cambios en la textura

        // --- PARTE 3: UNIR AMBOS (APLICAR TEXTURA A LA MALLA) ---

        // Asignamos la textura generada al material del objeto
        // Asegúrate de que el material use un Shader compatible (como Standard)
        meshRenderer.material.mainTexture = tex;

        // Opcional: Centrar el objeto en su origen
        objetoVisualizador.transform.position = new Vector3(-width / 2f, 0, -height / 2f);
    */
    }
    float[,] ConvertToHeights(float[][] valores, int width, int height)
    {
        Debug.Log(valores[0][0]);
        //Debug.Log(valores.Length);
        float[,] heights = new float[height, width];

        for (int y = 0; y < height; y++)
        {
            // Validación 1: ¿Existe esta fila?
            if (y >= valores.Length)
            {
                Debug.LogError($"Faltan filas en el JSON. Esperaba {height}, tengo {valores.Length}");
                break;
            }

            for (int x = 0; x < width; x++)
            {
                // Validación 2: ¿Existe esta columna en esta fila?
                if (valores[y] == null || x >= valores[y].Length)
                {
                    Debug.LogError($"Fila {y} incompleta. Esperaba {width} columnas, tengo {valores[y]?.Length ?? 0}");
                    continue;
                }

                heights[y, x] = valores[y][x] / 1000f;
            }
        }

        Debug.Log(heights[0,0]);
        return heights;
    }
}

