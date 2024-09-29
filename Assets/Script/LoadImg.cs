using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LoadImg : MonoBehaviour
{
    public void LoadDogImg(string url, System.Action<Texture> onSuccess)
    {
        StartCoroutine(RequestImg(url, onSuccess));
    }

    public IEnumerator RequestImg(string url, System.Action<Texture> onSuccess)
    {
        Debug.Log("Starting image request for URL: " + url);

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error loading image from URL: " + url + " | Error: " + www.error);
        }
        else
        {
            Debug.Log("Image loaded successfully from URL: " + url);
            Texture texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            onSuccess(texture);
        }
    }
}
