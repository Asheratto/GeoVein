using UnityEngine;
using UnityEngine.UI;

public class ToggleActive : MonoBehaviour
{
    [SerializeField] private Toggle miToggle;

    public void Start()
    {
        miToggle = GetComponent<Toggle>();
    }


    public void ActivarCuenca()
    {
        if (miToggle.isOn)
        {
            //GeometryProccesor.instance.config.EnableCuenca = true;
        }
        else
        {
            //GeometryProccesor.instance.config.EnableCuenca = false;
        }
        
    }

    public void ActivarAcuifero()
    {
        if (miToggle.isOn)
        {
            //GeometryProccesor.instance.config.EnableCuenca = true;
        }
        else
        {
            //GeometryProccesor.instance.config.EnableCuenca = false;
        }

    }

    public void ActivarDem()
    {
        if (miToggle.isOn)
        {
            //GeometryProccesor.instance.config.EnableCuenca = true;
        }
        else
        {
            //GeometryProccesor.instance.config.EnableCuenca = false;
        }

    }

    public void ActivarLagos()
    {
        if (miToggle.isOn)
        {
            //GeometryProccesor.instance.config.EnableCuenca = true;
        }
        else
        {
            //GeometryProccesor.instance.config.EnableCuenca = false;
        }

    }
}
