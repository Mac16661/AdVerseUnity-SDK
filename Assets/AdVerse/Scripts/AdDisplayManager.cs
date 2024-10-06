using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AdDisplayManager : MonoBehaviour
{
    [SerializeField] private Material adMaterial;
    [SerializeField] private RawImage adRawImage;
    private const float AD_DISPLAY_DURATION = 15f;
    private const float CHECK_INTERVAL = 5f;

    private void Start()
    {
        StartCoroutine(DisplayAdsCoroutine());
    }

    private IEnumerator DisplayAdsCoroutine()
    {
        while (true)
        {
            if (ResponseWrapper.data.Count > 0)
            {
                foreach (var adData in ResponseWrapper.data)
                {
                    yield return StartCoroutine(DownloadAndDisplayImage(adData.image));
                    yield return new WaitForSeconds(AD_DISPLAY_DURATION);
                }
            }
            yield return new WaitForSeconds(CHECK_INTERVAL);
        }
    }

    private IEnumerator DownloadAndDisplayImage(string url)
    {
        using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture downloadedTexture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            adMaterial.mainTexture = downloadedTexture;
            adRawImage.texture = downloadedTexture;
        }
        else
        {
            Debug.LogError($"Failed to download image: {request.error}");
        }
    }
}