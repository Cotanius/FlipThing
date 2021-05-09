using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlipThing
{
    public class FlipThingScreenShadered : MonoBehaviour
    {
        [SerializeField]
        bool generateInEditor = false;
        [SerializeField]
        int updatePerFrame = 1;
        [SerializeField]
        int pixelsX = 64;
        [SerializeField]
        int pixelsY = 32;
        [SerializeField]
        float pixelSize = 1;
        [SerializeField]
        float pixelScale = 1.5f;
        [SerializeField]
        float scale = 1.15f;
        [SerializeField]
        float threshold = .2f;
        [SerializeField]
        float onSpeed = 60;
        [SerializeField]
        float offSpeed = 10;
        [SerializeField]
        public Color tint = Color.white;

        [SerializeField]
        MeshFilter meshFilter;

        [SerializeField]
        Mesh pixelMesh;

        [SerializeField]
        Camera mirrorCamera;
        RenderTexture renderTexture;
        Texture2D texture;
        Mesh pixelArray;
        Vector3[] pixelVerts;
        Vector3[] pixelNorms;
        Vector2[] pixelUVs;
        Vector2[] pixelUVs2; // Used to store index info
        int[] pixelTris;
        Color[] pixelColors;
        float[,] pixelAngles;


        public MeshRenderer meshRenderer;
        void Awake()
        {
            renderTexture = new RenderTexture(pixelsX, pixelsY, 16);
            renderTexture.filterMode = FilterMode.Point;

            texture = new Texture2D(pixelsX, pixelsY);
            texture.filterMode = FilterMode.Point;
            mirrorCamera.orthographicSize = pixelsY * pixelSize * .5f;
            mirrorCamera.transform.localPosition = Vector3.one * pixelSize * .5f;
            mirrorCamera.targetTexture = renderTexture;

            pixelAngles = new float[pixelsX, pixelsY];

            generate();
        }

        private void OnValidate()
        {
            if (!generateInEditor)
            {
                meshFilter.mesh = null;
                return;
            }
            generate();
        }

        void generate()
        {

            Mesh pixelArrayMesh = new Mesh();
            List<Vector3> _pixelVerts = new List<Vector3>();
            List<Vector3> _pixelNorms = new List<Vector3>();
            List<Vector2> _pixelUVs = new List<Vector2>();
            List<int> _pixelTris = new List<int>();

            pixelMesh.GetVertices(_pixelVerts);
            pixelMesh.GetUVs(0, _pixelUVs);
            pixelMesh.GetTriangles(_pixelTris, 0);
            pixelMesh.GetNormals(_pixelNorms);
            // I add 4 vertecis to expand bounds to keep the thing from getting culled
            pixelVerts = new Vector3[pixelsX * pixelsY * _pixelVerts.Count];
            pixelNorms = new Vector3[pixelsX * pixelsY * _pixelNorms.Count];
            pixelUVs = new Vector2[pixelsX * pixelsY * _pixelUVs.Count];
            pixelUVs2 = new Vector2[pixelsX * pixelsY * _pixelUVs.Count];
            pixelTris = new int[pixelsX * pixelsY * _pixelTris.Count];
            pixelColors = new Color[pixelsX * pixelsY * _pixelVerts.Count];
            for (int i = 0; i < pixelsX; i++)
            {
                for (int j = 0; j < pixelsY; j++)
                {
                    var centerPosition =
                          (Vector3.right * (i + .5f - pixelsX * .5f) +
                        Vector3.up * (j + .5f - pixelsY * .5f)) * pixelSize * scale;


                    for (int k = 0; k < _pixelVerts.Count; k++)
                    {
                        int index = (i + j * pixelsX) * _pixelVerts.Count + k;
                        pixelVerts[index] = centerPosition;
                        var localPos = _pixelVerts[k] * pixelScale * pixelSize * scale;
                        localPos += Vector3.one;
                        localPos *= .5f;
                        pixelColors[index] = new Color(localPos.x, localPos.y, localPos.z);
                        pixelUVs2[index] = new Vector2(i * 1.0f / pixelsX, j * 1.0f / pixelsY);
                    }

                    for (int k = 0; k < _pixelUVs.Count; k++)
                    {
                        int index = (i + j * pixelsX) * _pixelUVs.Count + k;
                        pixelUVs[index] = _pixelUVs[k];
                    }
                    for (int k = 0; k < _pixelTris.Count; k++)
                    {
                        int index = (i + j * pixelsX) * _pixelVerts.Count;
                        int indexTri = (i + j * pixelsX) * _pixelTris.Count;
                        pixelTris[indexTri + k] = index + _pixelTris[k];
                    }
                    for (int k = 0; k < _pixelNorms.Count; k++)
                    {
                        int index = (i + j * pixelsX) * _pixelNorms.Count + k;
                        pixelNorms[index] = _pixelNorms[k];
                    }
                }
            }

            pixelArrayMesh.SetVertices(pixelVerts);
            pixelArrayMesh.SetUVs(0, pixelUVs);
            pixelArrayMesh.SetUVs(1, pixelUVs2);
            pixelArrayMesh.SetTriangles(pixelTris, 0);
            pixelArrayMesh.SetNormals(pixelNorms);
            pixelArrayMesh.SetColors(pixelColors);
            meshFilter.mesh = pixelArrayMesh;

            // Thank you unity for giving an ERROR for trying to set a color in editor
            //var mat  = Material.Instantiate(meshRenderer.material);
            //mat.SetColor("_Color", tint);
            //meshRenderer.material = mat;
        }

        int lastVisible = 3;
        void Update()
        {
            if (meshRenderer.isVisible)
                lastVisible = 3;
            mirrorCamera.enabled = lastVisible-- > 0;
            if (Time.frameCount % updatePerFrame != 0 || lastVisible < 0)
                return;

            var dt = Time.deltaTime;
            RenderTexture.active = renderTexture;
            Rect readRect = new Rect(0, 0, pixelsX, pixelsY);
            texture.ReadPixels(readRect, 0, 0);

            var cols = texture.GetPixels();

            for (int i = 0; i < pixelsX; i++)
            {
                for (int j = 0; j < pixelsY; j++)
                {
                    var col = cols[j * pixelsX + i];
                    var val = (col.r * 0.299f + col.g * 0.587f + col.b * 0.114f) * col.a;
                    float angle = 0;
                    if (val < threshold)
                        angle = 180;
                    var diff = (angle - pixelAngles[i, j]) * dt;
                    diff *= diff >= 0 ? offSpeed : onSpeed;
                    pixelAngles[i, j] += diff;
                    pixelAngles[i, j]  = Mathf.Clamp(pixelAngles[i, j], 0, 180);
                    cols[j * pixelsX + i] = (Color.white * pixelAngles[i, j]) / 180;
                }
            }
            texture.SetPixels(cols);
            texture.Apply();
            meshRenderer.material.SetTexture("_ScanTex", texture);
            meshRenderer.material.SetColor("_Color", tint);

            mirrorCamera.orthographicSize = pixelsY * pixelSize * .5f;
            mirrorCamera.transform.localPosition = Vector3.one * pixelSize * .5f;

        }
    }
}
