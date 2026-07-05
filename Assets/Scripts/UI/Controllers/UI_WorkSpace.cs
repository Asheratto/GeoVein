using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WorkSpace : MonoBehaviour
{

    public TextMeshProUGUI QuantityAquiferText;

    public Transform parentList;
    public GameObject prefabList;

    private void Start()
    {
        string url = $"http://localhost:5026/gvdb/acuiferoqa";
        string ur2 = $"http://localhost:5026/gvdb/acuiferos";
        StartCoroutine(ApiRequest.instance.GetQAcuifer(url, CrearListaAcuiferos));
        StartCoroutine(ApiRequest.instance.GetAcuiferos(ur2, UpdateBtnList));
    }

    private void CrearListaAcuiferos(int cantidad)
    {
        QuantityAquiferText.text = cantidad.ToString();
    }

    private void UpdateBtnList(List<Acuifero> list)
    {

        foreach (Acuifero aq in list)
        {
            var obj = Instantiate(prefabList, parentList);
            obj.GetComponent<BtnUpdate>().UpdateTexts(aq);
        }
    }
}
