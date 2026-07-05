using UnityEngine;

public class LoadAquifero : MonoBehaviour
{
    public static LoadAquifero instance;

    private Acuifero data;

    private void Awake()
    {
        instance = this;
    }


    public void AqSelected(Acuifero a)
    {
        Debug.Log("DataSeleccopmada");
        data = a;
    }

    public void LoadData()
    {
        //
        if (data != null)
        {
            Debug.Log("Cargando Acuifero");
            ApiRequest.instance.StartLoad3DModel(data);
        }
        
    }
}
