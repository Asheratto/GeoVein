using UnityEngine;

public class MapTarget : MonoBehaviour
{
    public string displayName;
    public Transform focusPoint;

    public Vector3 GetFocusPosition()
    {
        if (focusPoint != null)
            return focusPoint.position;

        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return transform.position;

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds.center;
    }
}

public class Card
{
    public string Name { get; private set; }
    public string Layer { get; private set; }
    public string Center { get; private set; }

    public object Source { get; private set; }

    public Card(string name, string layer, string center)
    {
        Name = name;
        Layer = layer;
        Center = center;
        Source = null;
    }

    public Card(string name, string layer, string center, object source)
    {
        Name = name;
        Layer = layer;
        Center = center;
        Source = source;
    }
}