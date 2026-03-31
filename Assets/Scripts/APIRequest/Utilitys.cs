using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Cubo
{
    public int id;
    public float x;
    public float y;
    public float z;
    public float size;
}

[System.Serializable]
public class CuboList
{
    public List<Cubo> cubos;
}

