using LibTessDotNet;
using Newtonsoft.Json;
using NUnit.Framework.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using static UnityEngine.Audio.ProcessorInstance;


public class ApiRequest : MonoBehaviour
{

    public static ApiRequest instance;

    public List<IElementMap> Data = new List<IElementMap>(); //->SceneData

    public List<Acuifero> Acuiferos = new List<Acuifero>();

    public bool Testing = false;

    private Coroutine loadModelCoroutine;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    { 
        string url = $"http://localhost:5026/gvdb/dbstatus";
        StartCoroutine(CheckDBStatus(url));  
    }

    public void DeleteData()
    {
        StopCurrentLoad();

        Data.Clear();

        Debug.Log("Datos de API limpiados.");
    }


    public void StartLoad3DModel(Acuifero a)
    {
        StopCurrentLoad();

        DeleteData();

        if (GeometryProccesor.instance != null)
            GeometryProccesor.instance.DeleteData();

        loadModelCoroutine = StartCoroutine(Load3DModel(a));
    }

    private void StopCurrentLoad()
    {
        if (loadModelCoroutine != null)
        {
            StopCoroutine(loadModelCoroutine);
            loadModelCoroutine = null;
        }
    }

    IEnumerator CheckDBStatus(string url)
    {
        LoadingUI.instance.Show();
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("API no conectada");
            yield break;
        }

        string response = www.downloadHandler.text.Trim().ToLower();

        if (bool.TryParse(response, out bool dbConnected))
        {
            StatusUI.instance.SetStatusDB(true);
        }
        else
        {
            StatusUI.instance.SetStatusDB(false);
        }
        LoadingUI.instance.Hide();
    }

    public IEnumerator GetQAcuifer(string url, Action<int> onResult)
    {
        LoadingUI.instance.Show();
        UnityWebRequest www = UnityWebRequest.Get(url);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
            yield break;
        }

        string response = www.downloadHandler.text.Trim();

        if (int.TryParse(response, out int cantidad))
        {
            onResult?.Invoke(cantidad);
        }
        LoadingUI.instance.Hide();
    }

    //Debería setear todos los datos
    public IEnumerator Load3DModel(Acuifero a)
    {
        LoadingUI.instance.Show();
        int id = a.id;
        string urlDem = $"http://localhost:5026/gvdb/dem?id={id}";
        string urlSubSubCuenca = $"http://localhost:5026/gvdb/cuenca?id={id}";
        string urlLagos = $"http://localhost:5026/gvdb/lagos?id={id}";
        string urlPuntos = $"http://localhost:5026/gvdb/np?id={id}";

        Data.Add(a);

        yield return StartCoroutine(GetDem(urlDem, a));
        yield return StartCoroutine(GetCuenca(urlSubSubCuenca));
        yield return StartCoroutine(GetLago(urlLagos));

        GeometryProccesor.instance.SaveData(Data);
        LoadingUI.instance.Hide();
    }

    IEnumerator GetDem(string url, Acuifero a)
    {

        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            byte[] tiffData = www.downloadHandler.data;
            Dem obj = new Dem(tiffData, a.Center);
            obj.Name = a.region;
            Data.Add(obj);
        }
        Debug.Log("Datos recibidos Dem recibido");
    }

    #region -> Acuifero

    public IEnumerator GetAcuiferos(string url, Action<List<Acuifero>> onResult)
    {
        Acuiferos.Clear();

        Debug.Log("Llamando a URL: " + url);

        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(
                "Error HTTP: " + www.responseCode +
                "\nError: " + www.error +
                "\nURL: " + url +
                "\nRespuesta servidor: " + www.downloadHandler.text
            );

            yield break;
        }

        string json = www.downloadHandler.text;
        Debug.Log("Respuesta recibida: " + json);

        List<AcuiferoRaw> raws = JsonConvert.DeserializeObject<List<AcuiferoRaw>>(json);

        if (raws == null)
        {
            Debug.LogError("No se pudieron deserializar los acuíferos.");
            yield break;
        }

        foreach (var raw in raws)
        {
            Acuifero obj = new Acuifero(raw);
            Acuiferos.Add(obj);
        }

        onResult?.Invoke(Acuiferos);
    }

    public IEnumerator GetAcuifero(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        string json = www.downloadHandler.text;
        Debug.Log(json);

        AcuiferoRaw raw = JsonConvert.DeserializeObject<AcuiferoRaw>(json);
        Acuifero obj = new Acuifero(raw);

        Data.Add(obj);

    }

    IEnumerator GetCuenca(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            List<CuencaRaw> raws = JsonConvert.DeserializeObject<List<CuencaRaw>>(www.downloadHandler.text);
            Dictionary<string, Cuenca> hash = new Dictionary<string, Cuenca>();

            foreach (var raw in raws)
            {
                
                Cuenca obj = new Cuenca(raw);
                if (hash.TryAdd(obj.name, obj))
                    Data.Add(obj);                
            }
        }
        Debug.Log("Datos cuencas recibidos ");
    }
    //Hay q trabajar los nulls
    IEnumerator GetLago(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            
            List<LagosRaw> raws = JsonConvert.DeserializeObject<List<LagosRaw>>(www.downloadHandler.text);

            foreach (var raw in raws)
            {
                Debug.Log("Agregando Lago");
                Lagos obj = new Lagos(raw);
                Data.Add(obj);
            }
        }
        Debug.Log("Datos recibidos Lago");
    }

    //Hay q ver si los puntos estan malos.
    IEnumerator GetPuntos(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Datos recibidos: " + www.downloadHandler.text);
            /*List<CuencaRaw> raws = JsonConvert.DeserializeObject<List<CuencaRaw>>(www.downloadHandler.text);
            Debug.Log(raws[0].nombre);
            Dictionary<string, Cuenca> hash = new Dictionary<string, Cuenca>();

            foreach (var raw in raws)
            {

                Cuenca obj = new Cuenca(raw);
                //Debug.Log(obj.name);
                if (hash.TryAdd(obj.name, obj))
                    Objects.Add(obj);


            }
            //DrawAcuifero(www.downloadHandler.text);*/
        }
        else
        {
            Debug.LogError("Error al obtener datos: " + www.error);
            Debug.LogError("Error HTTP: " + www.responseCode);
            Debug.LogError("Detalle: " + www.downloadHandler.text);
        }

    }

    #endregion


}
