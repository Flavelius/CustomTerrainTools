using UnityEditor;
using UnityEngine;

public class TerrainLayerVisualizer : EditorWindow
{
    [MenuItem("Window/Terrain Layer Visualizer")]
    static void Open()
    {
        GetWindow<TerrainLayerVisualizer>();
    }

    Terrain target;
    TerrainLayerState[] layers = new TerrainLayerState[0];

    Texture2D colorTex;

    Color currentColor = Color.red;

    struct TerrainLayerState
    {
        public TerrainLayer Layer { get; set; }
        public Texture2D OriginalDiffuse { get; set; }
    }

    void OnEnable()
    {
        SetPreviewColor(Color.red);
        OnSelectionChange();
    }

    void SetPreviewColor(Color newColor)
    {
        if (colorTex == null)
        {
            colorTex = new Texture2D(1, 1);
        }
        colorTex.SetPixel(0, 0, newColor);
        colorTex.Apply();
        currentColor = newColor;
    }

    void OnDisable()
    {
        RestoreActiveLayers();
        if (colorTex != null) DestroyImmediate(colorTex, false);
    }

    void OnSelectionChange()
    {
        var newTarget = Selection.activeGameObject;
        if (newTarget == null)
        {
            RestoreActiveLayers();
            target = null;
        }
        else
        {
            target = newTarget.GetComponent<Terrain>();
            RestoreActiveLayers();
            GatherLayers();
        }
        Repaint();
    }

    bool IsVisualized(TerrainLayerState layer)
    {
        return layer.Layer.diffuseTexture == colorTex;
    }

    void Update()
    {
        if (target == null) return;
        if (target.terrainData != null && target.terrainData.terrainLayers.Length != layers.Length)
        {
            RestoreActiveLayers();
            GatherLayers();
            Repaint();
        }
    }

    void OnGUI()
    {
        if (target == null)
        {
            EditorGUILayout.HelpBox("Select the target terrain", MessageType.Info);
            return;
        }
        if (layers.Length == 0)
        {
            EditorGUILayout.HelpBox("No layers assigned", MessageType.Info);
            return;
        }
        const int previewSize = 64;
        EditorGUILayout.Space();
        var newColor = EditorGUILayout.ColorField("Preview Color", currentColor);
        if (newColor != currentColor)
        {
            SetPreviewColor(newColor);
        }
        for (int i = 0; i < layers.Length; i++)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label(i.ToString(), GUILayout.Width(50));
            if (layers[i].OriginalDiffuse != null) GUILayout.Box(layers[i].OriginalDiffuse, GUILayout.Width(previewSize), GUILayout.Height(previewSize));
            var isSelected = IsVisualized(layers[i]);
            var newSelected = EditorGUILayout.Toggle(isSelected);
            if (newSelected != isSelected)
            {
                layers[i].Layer.diffuseTexture = newSelected ? colorTex : layers[i].OriginalDiffuse;
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    void GatherLayers()
    {
        if (target.terrainData == null) return;
        var tLayers = target.terrainData.terrainLayers;
        layers = new TerrainLayerState[tLayers.Length];
        for (int i = 0; i < tLayers.Length; i++)
        {
            layers[i].OriginalDiffuse = tLayers[i].diffuseTexture;
            layers[i].Layer = tLayers[i];
        }
    }

    void RestoreActiveLayers()
    {
        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i].Layer == null) continue;
            layers[i].Layer.diffuseTexture = layers[i].OriginalDiffuse;
        }
        layers = new TerrainLayerState[0];
    }
}
