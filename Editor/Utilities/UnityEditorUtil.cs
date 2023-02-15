using NPOI.OpenXmlFormats.Dml;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hypnos.Editor
{
    public static class UnityEditorUtil
    {
        #region Build-In Resources
        public const string DefalutAvatarFbxPath = "Avatar/DefaultAvatar.fbx";

        public static Object GetDefaultAvatarFbx()
        {
            return EditorGUIUtility.Load(DefalutAvatarFbxPath);
        }

        public static GameObject InstantiateForAnimatorPreview(Object obj)
        {
            Type editorType = Type.GetType("UnityEditor.EditorUtility, UnityEditor");
            return (GameObject)editorType.InvokeMember(
                "InstantiateForAnimatorPreview",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod,
                null, null, new object[] { obj });
        }
        #endregion

        public static Texture2D MakeTexForBackGround(Color col)
        {
            Texture2D result = new Texture2D(1, 1);
            result.SetPixel(0, 0, col);
            result.Apply();
            return result;
        }

        public static void DrawHorizontalBar()
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        public static void ShowTitle(string title, bool isBold = true)
        {
            if (isBold)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
                EditorGUILayout.Space();
            }
            else
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(title);
                EditorGUILayout.Space();
            }
        }

        public static Texture2D GenerateGridTexture(Color line, Color bg)
        {
            Texture2D tex = new Texture2D(64, 64);
            Color[] cols = new Color[64 * 64];
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    Color col = bg;
                    if (y % 16 == 0 || x % 16 == 0)
                    {
                        col = Color.Lerp(line, bg, 0.65f);
                    }

                    if (y == 63 || x == 63)
                    {
                        col = Color.Lerp(line, bg, 0.35f);
                    }

                    cols[y * 64 + x] = col;
                }
            }

            tex.SetPixels(cols);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            tex.name = "Grid";
            tex.Apply();
            return tex;
        }

        public static Texture2D GenerateGridTextureUE4Style()
        {
            Color OutLine = Color.black;
            Color InLine = Color.grey;
            Color bg = new Color(.15f, .15f, .15f, 1f);

            Texture2D tex = new Texture2D(512, 512);
            Color[] cols = new Color[512 * 512];
            for (int y = 0; y < 512; y++)
            {
                for (int x = 0; x < 512; x++)
                {
                    Color col = bg;

                    if (y % 64 == 0 || x % 64 == 0)
                    {
                        col = InLine;
                    }
                    //if (y == 127 || x == 127) col = OutLine;
                    if (y == 0 || x == 0)
                    {
                        col = OutLine;
                    }

                    cols[y * 512 + x] = col;
                }
            }

            tex.SetPixels(cols);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            tex.name = "Grid";
            tex.Apply();
            return tex;
        }

        public static Texture2D GenerateCrossTexture(Color line)
        {
            Texture2D tex = new Texture2D(64, 64);
            Color[] cols = new Color[64 * 64];
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    Color col = line;
                    if (y != 31 && x != 31)
                    {
                        col.a = 0;
                    }

                    cols[y * 64 + x] = col;
                }
            }

            tex.SetPixels(cols);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            tex.name = "Grid";
            tex.Apply();
            return tex;
        }

        public static Texture2D GenerateCrossTextureUE4Style()
        {
            Color line = Color.black;

            Texture2D tex = new Texture2D(512, 512);
            Color[] cols = new Color[512 * 512];
            for (int y = 0; y < 512; y++)
            {
                for (int x = 0; x < 512; x++)
                {
                    Color col = line;
                    if (y != 255 && x != 255)
                    {
                        col.a = 0;
                    }

                    cols[y * 64 + x] = col;
                }
            }

            tex.SetPixels(cols);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            tex.name = "Grid";
            tex.Apply();
            return tex;
        }

        public static void CreateCircle()
        {
            Mesh mesh = new Mesh();
            DrawPolygon(mesh);
            mesh.RecalculateNormals();
            GameObject obj = new GameObject("Circle");
            obj.AddComponent<MeshRenderer>();
            obj.AddComponent<MeshFilter>().mesh = mesh;
        }

        private static void DrawPolygon(Mesh mesh)
        {
            int circle_count = 64;
            Vector3[] vertices = new Vector3[circle_count + 1];
            vertices[0] = Vector3.zero;
            float pre_rad = Mathf.Deg2Rad * 360 / circle_count;
            for (int i = 0; i < circle_count; i++)
            {
                float deg = -i * pre_rad;
                float x = Mathf.Cos(deg);
                float y = Mathf.Sin(deg);
                vertices[i + 1] = new Vector3(x, y, 0) * 3;
            }
            mesh.vertices = vertices;

            int[] triangles = new int[circle_count * 3];
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int first = 0;
                int second = i / 3 + 1;
                int third = second + 1;
                if (third > circle_count)
                {
                    third = 1;
                }
                triangles[i] = first;
                triangles[i + 1] = second;
                triangles[i + 2] = third;
            }
            mesh.triangles = triangles;
        }

        public static Vector2 Drag2d(Vector2 scrollPos, Rect rect)
        {
            int controlId = GUIUtility.GetControlID("Slider".GetHashCode(), FocusType.Passive);
            Event currEvt = Event.current;
            if (currEvt.button == 2)
            {
                //GUIUtility.hotControl = controlID;
                //current.Use();
                return new Vector2(-currEvt.delta.x, currEvt.delta.y);
            }

            switch (currEvt.GetTypeForControl(controlId))
            {
                case EventType.MouseDown:
                {
                    if (rect.Contains(currEvt.mousePosition) && rect.width > 50f)
                    {
                        GUIUtility.hotControl = controlId;
                        currEvt.Use();
                        EditorGUIUtility.SetWantsMouseJumping(1); // 让鼠标可以拖动到屏幕外后，从另一边出来
                    }
                    break;
                }
                case EventType.MouseUp:
                {
                    if (GUIUtility.hotControl == controlId)
                    {
                        GUIUtility.hotControl = 0;
                    }
                    EditorGUIUtility.SetWantsMouseJumping(0);
                    break;
                }
                case EventType.MouseDrag:
                {
                    if (GUIUtility.hotControl == controlId)
                    {
                        // 按住Shift键后，可以加快旋转
                        scrollPos -= 140f
                            * (!currEvt.shift ? 1 : 3) / Mathf.Min(rect.width, rect.height)
                            * currEvt.delta;
                        scrollPos.y = Mathf.Clamp(scrollPos.y, -90f, 90f);
                        currEvt.Use();
                        GUI.changed = true;
                    }
                    break;
                }
            }
            return scrollPos;
        }

        public static bool ScrollWheel(ref float targetValue, Rect rect)
        {
            const float speed = 0.35f;

            int controlID = GUIUtility.GetControlID("Slider".GetHashCode(), FocusType.Passive);
            Event currEvt = Event.current;
            EventType type = currEvt.GetTypeForControl(controlID);
            if (type != EventType.ScrollWheel)
            {
                return false;
            }

            if (rect.Contains(currEvt.mousePosition))
            {
                targetValue += currEvt.delta.y * speed;
                //Debug.Log($"delta:{Event.current.delta.y}");
                //Debug.Log($"now:{targetValue}");
                return true;
            }
            return false;
        }

        public static void GetRenderableBoundsRecursively(ref Bounds bounds, GameObject go)
        {
            if (!go.TryGetComponent<Renderer>(out Renderer curRenderer))
            {
                foreach (Transform transform in go.transform)
                {
                    GetRenderableBoundsRecursively(ref bounds, transform.gameObject);
                }
                return;
            }

            switch (curRenderer)
            {
                case MeshRenderer meshRenderer:
                {
                    MeshFilter meshFilter = go.GetComponent<MeshFilter>();
                    if (meshRenderer && meshFilter && meshFilter.sharedMesh)
                    {
                        if (bounds.extents == Vector3.zero)
                        {
                            bounds = meshRenderer.bounds;
                        }
                        else
                        {
                            // 扩展包围盒，以让包围盒能够包含另一个包围盒
                            bounds.Encapsulate(meshRenderer.bounds);
                        }
                    }

                    break;
                }
                case SkinnedMeshRenderer skinnedMeshRenderer:
                {
                    if (skinnedMeshRenderer && skinnedMeshRenderer.sharedMesh)
                    {
                        if (bounds.extents == Vector3.zero)
                        {
                            bounds = skinnedMeshRenderer.bounds;
                        }
                        else
                        {
                            bounds.Encapsulate(skinnedMeshRenderer.bounds);
                        }
                    }

                    break;
                }
            }

            foreach (Transform transform in go.transform)
            {
                GetRenderableBoundsRecursively(ref bounds, transform.gameObject);
            }
        }
    }
}