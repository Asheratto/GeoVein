using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class StatusUI : MonoBehaviour
{
    [SerializeField] private Image statusDb;
    [SerializeField] private Image statusAquifero;
    [SerializeField] private Image statusModelo3D;
    [SerializeField] private Image statusSim;


    public static StatusUI instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public void SetStatusDB(bool enable)
    {
        if (enable) {
            statusDb.color = Color.green;
            return;
        }
        statusDb.color = Color.red;
    }

    public void SetStatusAquifero(bool enable)
    {
        if (enable)
        {
            statusAquifero.color = Color.green;
            return;
        }
        statusAquifero.color = Color.red;
    }
    public void SetStatusModelo3D(bool enable)
    {
        if (enable)
        {
            statusModelo3D.color = Color.green;
            return;
        }
        statusModelo3D.color = Color.red;
    }

    public void SetStatusSim(bool enable)
    {
        if (enable)
        {
            statusSim.color = Color.green;
            return;
        }
        statusSim.color = Color.red;
    }
}
