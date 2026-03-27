using UnityEngine;
using SFB; // Importante: Esta es la librería que acabas de instalar

public class SelectorDeArchivos : MonoBehaviour
{
    public void AbrirExplorador()
    {
        // Esto abre la ventana nativa de Windows
        var paths = StandaloneFileBrowser.OpenFilePanel("Seleccionar Imagen", "", "png", false);
        
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            Debug.Log("Ruta detectada: " + paths[0]);
            // Aquí es donde llamaremos a la función para cargar la imagen
        }
    }
}
