using UnityEngine;

public class ClearModulos : MonoBehaviour
{
    public void DeleteChildren()
    {
        foreach (Transform hijo in transform)
        {
            Destroy(hijo.gameObject);
        }
    }
}
