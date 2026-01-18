using UnityEngine;
using UnityEditor;
using System.Linq;

public class TaeGradientShaderGUI : ShaderGUI
{
    private Gradient gradient = new Gradient();
    private bool gradientInitialized = false;
    private const int MAX_GRADIENT_KEYS = 8;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Material target = materialEditor.target as Material;

        // Initialize gradient from shader properties once
        if (!gradientInitialized)
        {
            LoadGradientFromMaterial(target);
            gradientInitialized = true;
        }
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tae Gradient", EditorStyles.boldLabel);
        
        EditorGUI.BeginChangeCheck();
        gradient = EditorGUILayout.GradientField("Gradient", gradient);
        
        // Check for too many color keys
        if (gradient.colorKeys.Length > MAX_GRADIENT_KEYS)
        {
            EditorGUILayout.HelpBox(
                $"Gradient has {gradient.colorKeys.Length} colors but only {MAX_GRADIENT_KEYS} are supported! " +
                "Remove some color stops.", 
                MessageType.Error
            );
        }
        else if (EditorGUI.EndChangeCheck())
        {
            SaveGradientToMaterial(target);
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Other Properties", EditorStyles.boldLabel);

        foreach (var prop in properties)
        {
            // Skip gradient-related properties
            if (prop.name.Contains("TaeColor") || prop.name.Contains("TaePosition"))
                continue;

            // Skip Unity's built-in properties (lightmaps, shadowmasks, etc.)
            if (prop.name.StartsWith("unity_"))
                continue;

            // Skip internal properties with HideInInspector flag
            if (prop.name.StartsWith("_") && (prop.flags & MaterialProperty.PropFlags.HideInInspector) != 0)
                continue;

            // Draw the property
            materialEditor.ShaderProperty(prop, prop.displayName);
        }
    }

    void LoadGradientFromMaterial(Material mat)
    {
        // Collect all valid color/position pairs
        var colorKeys = new System.Collections.Generic.List<GradientColorKey>();
        var alphaKeys = new System.Collections.Generic.List<GradientAlphaKey>();

        for (int i = 0; i < MAX_GRADIENT_KEYS; i++)
        {
            string colorProp = $"_TaeColor{i}";
            string posProp = $"_TaePosition{i}";

            if (mat.HasProperty(posProp))
            {
                float pos = mat.GetFloat(posProp);
                
                // Only include if position is in range 0-1
                if (pos >= 0f && pos <= 1f)
                {
                    Color col = mat.HasProperty(colorProp) ? mat.GetColor(colorProp) : Color.white;
                    
                    colorKeys.Add(new GradientColorKey(col, pos));
                    alphaKeys.Add(new GradientAlphaKey(col.a, pos));
                }
            }
        }

        // Ensure we have at least 2 keys (gradient requirement)
        if (colorKeys.Count < 2)
        {
            colorKeys.Clear();
            alphaKeys.Clear();
            colorKeys.Add(new GradientColorKey(Color.black, 0f));
            colorKeys.Add(new GradientColorKey(Color.white, 1f));
            alphaKeys.Add(new GradientAlphaKey(1f, 0f));
            alphaKeys.Add(new GradientAlphaKey(1f, 1f));
        }

        gradient.SetKeys(colorKeys.ToArray(), alphaKeys.ToArray());
    }

    void SaveGradientToMaterial(Material mat)
    {
        GradientColorKey[] colorKeys = gradient.colorKeys;
        
        for (int i = 0; i < MAX_GRADIENT_KEYS; i++)
        {
            string colorProp = $"_TaeColor{i}";
            string posProp = $"_TaePosition{i}";

            if (i < colorKeys.Length)
            {
                mat.SetColor(colorProp, colorKeys[i].color);
                mat.SetFloat(posProp, colorKeys[i].time);
            }
            else
            {
                mat.SetFloat(posProp, -1f);
                mat.SetColor(colorProp, Color.black);
            }
        }

        EditorUtility.SetDirty(mat);
    }
}