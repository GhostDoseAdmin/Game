using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

//Draws the tooltips in the shader inspector and allows hiding features based on other features.
public class CustomInspectorDrawer : MaterialPropertyDrawer
{
    enum RenderingMode
    {
        Opaque, Cutout, Fade, Transparent
    }

    struct RenderingSettings
    {
        public RenderQueue queue;
        public string renderType;
        public BlendMode srcBlend, dstBlend;
        public bool zWrite;

        public static RenderingSettings[] modes = 
        {
            new RenderingSettings()
            {
                queue = RenderQueue.Geometry,
                renderType = "",
                srcBlend = BlendMode.One,
                dstBlend = BlendMode.Zero,
                zWrite = true
            },
            new RenderingSettings()
            {
                queue = RenderQueue.AlphaTest,
                renderType = "TransparentCutout",
                srcBlend = BlendMode.One,
                dstBlend = BlendMode.Zero,
                zWrite = true
            },
            new RenderingSettings()
            {
                queue = RenderQueue.Transparent,
                renderType = "Transparent",
                srcBlend = BlendMode.SrcAlpha,
                dstBlend = BlendMode.OneMinusSrcAlpha,
                zWrite = false
            },
            new RenderingSettings()
            {
                queue = RenderQueue.Transparent,
                renderType = "Transparent",
                srcBlend = BlendMode.One,
                dstBlend = BlendMode.OneMinusSrcAlpha,
                zWrite = false
            }
        };
    }

    string tooltipValue;
    string[] dependantArgValue;
    bool hasSecondArg;
    string[] dependantArgValue2;
    bool drawScaleOffsetValue;
    bool isToggleValue;

    protected bool renderMode;
    protected bool[] renderModeOutput;

    protected GUIContent staticLabel = new GUIContent();

    public CustomInspectorDrawer(string tooltip)
    {
        if (tooltip.Contains("RENDERMODE"))
        {
            string[] splitLine = tooltip.Split('_');
            tooltipValue = splitLine[0];
            renderModeOutput = new bool[4];
            char[] renderModeValues = splitLine[1].ToCharArray();

            for (int i = 0; i < renderModeOutput.Length; i++)
            {
                if(renderModeValues[i] == '1')
                {
                    renderModeOutput[i] = true;
                }
                else
                {
                    renderModeOutput[i] = false;
                }
            }

            renderMode = true;
        }
        else
        {
            tooltipValue = tooltip;
        }

        GetDependantArgs(null, null);
        drawScaleOffsetValue = true;
    }

    public CustomInspectorDrawer(string tooltip, string dependantArg)
    {
        tooltipValue = tooltip;
        drawScaleOffsetValue = true;
        GetDependantArgs(dependantArg, null);
        isToggleValue = false;
    }

    public CustomInspectorDrawer(string tooltip, string dependantArg, string dependantArg2)
    {
        tooltipValue = tooltip;
        GetDependantArgs(dependantArg, dependantArg2);
        drawScaleOffsetValue = true;
        isToggleValue = false;
    }

    public CustomInspectorDrawer(string tooltip, string dependantArg, string dependantArg2, string drawScaleOffset)
    {
        tooltipValue = tooltip;
        GetDependantArgs(dependantArg, dependantArg2);
        bool drawScaleParse = false;

        if (bool.TryParse(drawScaleOffset, out drawScaleParse))
        {
            drawScaleOffsetValue = drawScaleParse;
        }
        else
        {
            drawScaleOffsetValue = false;
        }

        isToggleValue = false;
    }

    public CustomInspectorDrawer(string tooltip, string dependantArg, string dependantArg2, string drawScaleOffset, string isToggle)
    {
        tooltipValue = tooltip;
        GetDependantArgs(dependantArg, dependantArg2);
        bool drawScaleParse = false;

        if (bool.TryParse(drawScaleOffset, out drawScaleParse))
        {
            drawScaleOffsetValue = drawScaleParse;
        }
        else
        {
            drawScaleOffsetValue = true;
        }

        bool isToggleParse = false;

        if (bool.TryParse(isToggle, out isToggleParse))
        {
            isToggleValue = isToggleParse;
        }
        else
        {
            isToggleValue = false;
        }
    }

    void GetDependantArgs(string arg1, string arg2)
    {
        if (arg1 != null && arg1 != "_")
        {
            dependantArgValue = arg1.Split(' ');
        }
        else
        {
            dependantArgValue = new string[0];
        }

        if (arg2 != null && arg2 != "_")
        {
            dependantArgValue2 = arg2.Split(' ');
            if (dependantArgValue2.Length > 0)
            {
                hasSecondArg = true;
            }
        }
        else
        {
            dependantArgValue2 = new string[0];
        }
    }

    public override void OnGUI(Rect position, MaterialProperty property, GUIContent label, MaterialEditor editor)
    {
        EditorGUI.BeginChangeCheck();

        bool drawProperty = true;
        Material mat = editor.target as Material;

        if (renderMode)
        {
            DisplayRenderingMode(mat);

            return;
        }

        if (dependantArgValue.Length > 0)
        {
            float enabled = 0;

            for (int i = 0; enabled == 0 && i < dependantArgValue.Length; i++)
            {
                enabled = MaterialEditor.GetMaterialProperty(new Object[] { mat }, dependantArgValue[i]).floatValue;
            }

            if (enabled == 1 && hasSecondArg)
            {
                enabled = 0;

                for (int i = 0; enabled == 0 && i < dependantArgValue2.Length; i++)
                {
                    enabled = MaterialEditor.GetMaterialProperty(new Object[] { mat }, dependantArgValue2[i]).floatValue;
                }
            }

            if (enabled != 1)
            {
                drawProperty = false;
            }        
        }

        if(drawProperty)
        {
            switch (property.type)
            {
                case MaterialProperty.PropType.Vector:
                    position.size = new Vector2(position.size.x - 10, position.size.y);
                    EditorGUI.LabelField(position, new GUIContent(label.text, tooltipValue));
                    position.y += 20;
                    Vector4 newVectorValue = EditorGUI.Vector3Field(position, new GUIContent("", tooltipValue), property.vectorValue);
                    GUILayout.Space(25);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.vectorValue = newVectorValue;
                    }
                    break;
                case MaterialProperty.PropType.Float:
                    float newFloatValue;
                    if(isToggleValue)
                    {
                        bool toggleValue = false;

                        if(property.floatValue == 1)
                        {
                            toggleValue = true;
                        }

                        bool boolFloatValue = EditorGUI.Toggle(position, new GUIContent(label.text, tooltipValue), toggleValue);

                        if(boolFloatValue)
                        {
                            newFloatValue = 1;
                        }
                        else
                        {
                            newFloatValue = 0;
                        }

                        SetKeyword(mat, property.name, boolFloatValue);
                    }
                    else
                    {
                        newFloatValue = EditorGUI.FloatField(position, new GUIContent(label.text, tooltipValue), property.floatValue);
                    }       
                    
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.floatValue = newFloatValue;
                    }
                    break;
                case MaterialProperty.PropType.Color:
                    Color newColorValue = EditorGUI.ColorField(position, new GUIContent(label.text, tooltipValue), property.colorValue, true, true, true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.colorValue = newColorValue;
                    }
                    break;
                case MaterialProperty.PropType.Range:
                    float newRangeValue = GUILayout.HorizontalSlider(property.floatValue, property.rangeLimits.x, property.rangeLimits.y);
                    newRangeValue = EditorGUI.FloatField(position, new GUIContent(label.text, tooltipValue), newRangeValue);
                    EditorGUI.LabelField(position, new GUIContent(label.text, tooltipValue));
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.floatValue = newRangeValue;
                    }
                    break;
                case MaterialProperty.PropType.Texture:
                    position.position = new Vector2(position.position.x, position.position.y + 5);
                    Texture newTextureValue = editor.TextureProperty(position, property, label.text, tooltipValue, drawScaleOffsetValue);
                    GUILayout.Space(60);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.textureValue = newTextureValue;
                    }
                    break;
                default:
                    break;
            }
        }
        else
        {
            GUILayout.Space(-20);
        }
    }

    void DisplayRenderingMode(Material material)
    {
        RenderingMode mode = RenderingMode.Opaque;

        if (IsKeywordEnabled(material, "_RENDERING_CUTOUT"))
        {
            mode = RenderingMode.Cutout;

            if(renderModeOutput[1])
            {
                material.SetFloat("RENDERMODE", 1);
            } 
            else
            {
                material.SetFloat("RENDERMODE", 0);
            }
        }
        else if (IsKeywordEnabled(material, "_RENDERING_FADE"))
        {
            mode = RenderingMode.Fade;

            if (renderModeOutput[2])
            {
                material.SetFloat("RENDERMODE", 1);
            }
            else
            {
                material.SetFloat("RENDERMODE", 0);
            }
        }
        else if (IsKeywordEnabled(material, "_RENDERING_TRANSPARENT"))
        {
            mode = RenderingMode.Transparent;

            if (renderModeOutput[3])
            {
                material.SetFloat("RENDERMODE", 1);
            }
            else
            {
                material.SetFloat("RENDERMODE", 0);
            }
        }
        else
        {
            if (renderModeOutput[0])
            {
                material.SetFloat("RENDERMODE", 1);
            }
            else
            {
                material.SetFloat("RENDERMODE", 0);
            }
        }

        EditorGUI.BeginChangeCheck();
        mode = (RenderingMode)EditorGUILayout.EnumPopup(ConstructLabel("Rendering Mode"), mode);
        if (EditorGUI.EndChangeCheck())
        {
            SetKeyword(material, "_RENDERING_CUTOUT", mode == RenderingMode.Cutout);
            SetKeyword(material, "_RENDERING_FADE", mode == RenderingMode.Fade);
            SetKeyword(material, "_RENDERING_TRANSPARENT", mode == RenderingMode.Transparent);

            RenderingSettings settings = RenderingSettings.modes[(int)mode];

            material.renderQueue = (int)settings.queue;
            material.SetOverrideTag("RenderType", settings.renderType);
            material.SetInt("_SrcBlend", (int)settings.srcBlend);
            material.SetInt("_DstBlend", (int)settings.dstBlend);
            material.SetInt("_ZWrite", settings.zWrite ? 1 : 0);           
        }
    }

    bool IsKeywordEnabled(Material target, string keyword)
    {
        return target.IsKeywordEnabled(keyword);
    }

    void SetKeyword(Material m, string keyword, bool state)
    {
        if (state)
        {
            m.EnableKeyword(keyword);          
        }
        else
        {
            m.DisableKeyword(keyword);
        }
    }

    GUIContent ConstructLabel(string text, string tooltip = null)
    {
        staticLabel.text = text;
        staticLabel.tooltip = tooltip;
        return staticLabel;
    }
}
