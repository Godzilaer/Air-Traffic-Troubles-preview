using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Linq;

public class CaptureRunwayThumbnail
{
    [SerializeField] private int levelNum;
    [SerializeField] private bool save;

    [MenuItem("Runway Thumbnail/Capture")]
    public static void Capture() {
        GameObject cameraObj = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/CaptureRunwayThumbnail.prefab"));

        Camera cam = cameraObj.GetComponent<Camera>();

        cam.Render();
        RenderTexture rt = cam.targetTexture;

        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        RenderTexture.active = null;

        byte[] bytes;
        bytes = tex.EncodeToPNG();

        string levelNum = new string(SceneManager.GetActiveScene().name.Where(char.IsDigit).ToArray());
        string path = "Assets/Resources/MainMenu/LevelRunwayPreviews/" + levelNum + ".png";

        System.IO.File.WriteAllBytes(path, bytes);
        AssetDatabase.ImportAsset(path);

        Object.DestroyImmediate(cameraObj);
    }
}
