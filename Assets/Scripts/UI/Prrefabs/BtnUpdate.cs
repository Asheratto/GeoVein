using TMPro;
using UnityEngine;

public class BtnUpdate : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Acuifero aq;

    public void UpdateTexts(Acuifero _aq)
    {
        aq = _aq;
        text.text = aq.name + " (" + aq.id + ")";
    }

    public void UpdateText()
    {
        DetailUpdate.instance.UpdateData(aq);
        LoadAquifero.instance.AqSelected(aq);
    }

    
}
