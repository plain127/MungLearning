using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using ReqandRes;

[Serializable]
public class ApiKeyData
{
    public string Encoding;
}

public class APIManager : MonoBehaviour
{
   private string baseUrl = "http://apis.data.go.kr/1543061/abandonmentPublicSrvc/abandonmentPublic";
    private float retryDelay = 10.0f;
    private int maxRetries = 3;
    private int currentRetries = 0;

    public void SendApi(string startDate, string endDate)
    {
        StartCoroutine(Request(startDate, endDate));
    }

    public IEnumerator Request(string startDate, string endDate)
    {
        RequestData requestData = new RequestData();
        string json = JsonUtility.ToJson(requestData);
        
        string apiKeyJson = File.ReadAllText(Application.streamingAssetsPath + "/Json/ApiKey.json");
        ApiKeyData apiKeyData = JsonUtility.FromJson<ApiKeyData>(apiKeyJson);
        requestData.serviceKey = apiKeyData.Encoding;
        requestData.bgnde = startDate;
        requestData.endde = endDate;

        string url = $"{baseUrl}?bgnde={requestData.bgnde}&endde={requestData.endde}&pageNo={requestData.pageNo}&numOfRows=1000" +
                     $"&upkind={requestData.upkind}&upr_cd={requestData.upr_cd}&org_cd={requestData.org_cd}"+
                     $"&state={requestData.state}&neuter_yn={requestData.neuter_yn}"+
                     $"&serviceKey={requestData.serviceKey}&_type=json";
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                if (www.responseCode == 429 && currentRetries < maxRetries)
                {
                    currentRetries++;
                    Debug.Log("Rate limit reached. Waiting for " + retryDelay + " seconds. Retry count: " + currentRetries);
                    yield return new WaitForSeconds(retryDelay);
                    StartCoroutine(Request(requestData.bgnde, requestData.endde));
                }
                else
                {
                    Debug.LogError("Error: " + www.error);
                }
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
                ApiResponse apiResponse = JsonUtility.FromJson<ApiResponse>(www.downloadHandler.text);
                if (apiResponse != null && apiResponse.response != null && apiResponse.response.body != null &&
                apiResponse.response.body.items != null && apiResponse.response.body.items.item != null)
                {
                    SaveItemsToCSV(apiResponse.response.body.items.item); 
                }
                else
                {
                    Debug.LogError("Error: Response data is incomplete or null.");
                }
            }
        }
    }
    void SaveItemsToCSV(Item[] items)
    {

        if (items == null || items.Length == 0)
        {
            Debug.LogError("No items to save.");
            return;
        }
        string filePath = Path.Combine(Application.streamingAssetsPath, "ML/items.csv");

        using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
        {
            writer.WriteLine("유기번호, 품종, 색상, 나이, 접수일시, 체중, 성별, 중성화유무, 기타 특징, 사진");
            foreach (Item item in items)
            {
                string csvLine = $"\"{item.desertionNo}\",\"{item.kindCd}\",\"{item.colorCd}\",\"{item.age}\",\"{item.happenDt}\",\"{item.weight}\",\"{item.sexCd}\",\"{item.neuterYn}\",\"{item.specialMark}\",\"{item.popfile}\"";

                writer.WriteLine(csvLine);
            }

            Debug.Log("CSV 파일 저장 완료: " + filePath);
        }

    }
}