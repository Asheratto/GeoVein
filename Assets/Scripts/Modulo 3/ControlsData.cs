using System;
using TMPro;
using UnityEngine;

public class ControlsData : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI m_TextMeshPro;
    [SerializeField] TextMeshProUGUI m_tittle;
    public void button1()
    {
        m_tittle.text = "Documentación de controles";
        m_TextMeshPro.text = "Click derecho + mover mouse: rota la cámara alrededor del modelo.\r\nRueda del mouse: acerca o aleja la vista.\r\nSelección desde la barra inferior: enfoca la cámara en el elemento seleccionado.";
    }
    public void button2()
    {
        m_tittle.text = "Carga del modelo";
        m_TextMeshPro.text = "Cargar modelo: obtiene los datos desde la base de datos y genera la visualización tridimensional.\r\nBorrar modelo: elimina los datos cargados, limpia la escena, reinicia la navegación y restablece los estados de simulación.";
    }

    public void button3()
    {
        m_tittle.text = "Visualización de capas";
        m_TextMeshPro.text = "El modelo está organizado por capas geométricas:\r\n\r\nTerreno: representa la superficie base generada desde el DEM.\r\nAcuíferos: representa las unidades hidrogeológicas cargadas desde la base de datos.\r\nCuencas: representa las áreas de drenaje asociadas al modelo.\r\nLagos: representa los cuerpos de agua superficiales.\r\n\r\nCada capa puede visualizarse de forma independiente para facilitar la exploración del modelo.";
    }

    public void button4()
    {
        m_tittle.text = "Barra inferior";
        m_TextMeshPro.text = "La barra inferior muestra los elementos cargados en la escena. Al seleccionar un elemento:\r\n\r\nse enfoca la cámara sobre su ubicación;\r\nse muestra su nombre;\r\nse indica la capa a la que pertenece;\r\nse facilita la navegación entre elementos del modelo.";
    }

    public void button5()
    {
        m_tittle.text = "Simulación hidrológica";
        m_TextMeshPro.text = "Inicializar simulación: prepara el motor hidrológico usando los datos geométricos cargados.\r\nEjecutar / pausar: inicia o detiene temporalmente la simulación.\r\nPaso: avanza la simulación una iteración.\r\nReiniciar simulación: vuelve al estado inicial usando los datos actualmente cargados.\r\n\r\nLa simulación utiliza el terreno, las máscaras de capas y los parámetros hidrológicos configurados para representar el comportamiento del agua superficial.";
    }

    public void button6()
    {
        m_tittle.text = "Recomendación de uso";
        m_TextMeshPro.text = "Verificar que la base de datos esté conectada.\r\nSeleccionar y cargar un modelo.\r\nRevisar las capas generadas en la escena.\r\nUsar la barra inferior para enfocar elementos específicos.\r\nInicializar la simulación hidrológica.\r\nEjecutar, pausar o avanzar la simulación según sea necesario.\r\nBorrar el modelo antes de cargar uno nuevo.";
    }

    public void button7()
    {
        m_tittle.text = "Acerda de";
        m_TextMeshPro.text = "GeoVein es una herramienta de visualización tridimensional para datos hidrogeológicos. Permite cargar información desde una base de datos, procesar geometrías y generar modelos 3D interactivos de terreno, acuíferos, cuencas y lagos.\r\n\r\nEl sistema fue diseñado con una arquitectura modular, separando la obtención de datos, el procesamiento geométrico, la generación de mallas, el renderizado y la simulación hidrológica. Esto permite que la herramienta de generación de mallas pueda reutilizarse y extenderse a otros tipos de geometrías.\r\n\r\nGeoVein fue desarrollado como parte de un proyecto de memoria, con el objetivo de apoyar el análisis visual de información hidrogeológica mediante un entorno interactivo en Unity."; 
    }
}
