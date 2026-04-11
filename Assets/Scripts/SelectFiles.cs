using UnityEngine;
using SFB;
using System.IO; // Importante: Esta es la librería que acabas de instalar

public class SelectFiles : MonoBehaviour
{
    public void AbrirExplorador()
    {
        // Esto abre la ventana nativa de Windows
        var paths = StandaloneFileBrowser.OpenFilePanel("Seleccionar Imagen", "", "png", false);
        
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            Debug.Log("Ruta detectada: " + paths[0]);
            
        }
    }

    public void SelectTIF() {
        var paths = StandaloneFileBrowser.OpenFilePanel("Seleccionar csv", "", "tif", true);
        foreach (var path in paths)
        {
            Debug.Log("Archivo seleccionado: " + path);
            //Verificar la integridad
            byte[] bytes = File.ReadAllBytes(path);
            APIRequest.Instance.PostTif(bytes);
            //Procesar raster a dem

        }
        
    }
}
