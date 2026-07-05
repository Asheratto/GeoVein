using System.Collections.Generic;
using UnityEngine;

public interface IElementMap 
{
    Vector2 Center { get; }
    string Name { get; }
    Color DisplayColor();
}

public interface IGeometricElement : IElementMap
{
    List<Vector2> GetVertices();
}

public interface IRasterElement : IElementMap
{
    Texture2D GetHeightMap();
}


public interface ILagos : IGeometricElement
{ 

}

public interface ICuenca : IGeometricElement 
{

}
public interface IDem : IRasterElement 
{ 

}
public interface IAcuifero : IGeometricElement 
{ 

}




