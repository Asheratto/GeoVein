using TMPro;
using UnityEngine;

public class DetailUpdate : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] TextMeshProUGUI text1;
    [SerializeField] TextMeshProUGUI text2;
    [SerializeField] TextMeshProUGUI text3;
    [SerializeField] TextMeshProUGUI text4;

    public static DetailUpdate instance;

    private void Awake()
    {
        

            instance = this;
        
    }

    public void UpdateData(Acuifero a)
    {
        text1.text = a.id.ToString();
        text2.text = a.name;
        text3.text = a.region;
        text4.text = a.shac;
    }
}
