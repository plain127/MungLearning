using System;
using System.IO; 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public partial class UIManager : MonoBehaviour
{
    //Using Input Scene
    public Button submitBtn;
    public InputField startDate;
    public InputField endDate;
    public InputField numInput;
    GameObject apiManager;
    GameObject mlManager;

    //Using Output Scene
    public RawImage dogImg;
    List<string> dogNumbers;
    List<string> imgUrls;
    public Button dogId;
    Transform contentTransform;
    Transform imgTransform;
    GameObject imgManager;
   
    public class ModelResult
    {
        public List<string> dog_num { get; set; }
        public List<string> urls { get; set; }
    }   

    public void Start()
    {   
        apiManager = GameObject.Find("ApiManager");
        mlManager = GameObject.Find("MLManager");
        imgManager = GameObject.Find("ImgManager");
        submitBtn.onClick.AddListener(SendModel);
    }

    public void SendModel()
    {
        apiManager.GetComponent<APIManager>().SendApi(startDate.text, endDate.text);
        
        StartCoroutine(ProcessModelResult());
        
    }

    public IEnumerator ProcessModelResult()
    {
        // MLManager의 코루틴 호출
        yield return StartCoroutine(mlManager.GetComponent<MLManager>().ConnectModel(numInput.text, 600));
        //yield return null;

        UnityEngine.Debug.Log("파이썬한테 값 받았음");
        
        string jsonFilePath = Application.streamingAssetsPath + "/ML/output_data.json";
        // JSON 파일 읽기
        if (File.Exists(jsonFilePath))
        {
            string jsonContent = File.ReadAllText(jsonFilePath);

            try
            {
                // JSON 내용을 ModelResult 클래스로 파싱
                ModelResult result = JsonConvert.DeserializeObject<ModelResult>(jsonContent);

                if (result != null)
                {
                    dogNumbers = result.dog_num;
                    imgUrls = result.urls;

                    UnityEngine.Debug.Log("dogNumbers: " + string.Join(", ", dogNumbers));
                    UnityEngine.Debug.Log("imgUrls: " + string.Join(", ", imgUrls));
                }
                else
                {
                    UnityEngine.Debug.LogError("Failed to parse JSON.");
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("Exception while parsing JSON: " + ex.Message);
            }
        }
        else
        {
            UnityEngine.Debug.LogError("JSON file not found: " + jsonFilePath);
        }

        // Output 씬 로드
        SceneManager.LoadScene("Output");
        SceneManager.sceneLoaded += OnOutputSceneLoaded;
    }

    
    public void OnOutputSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == "Output")
        {
            ScrollRect scrollView = GameObject.Find("OutputView").GetComponent<ScrollRect>();
            Image img = GameObject.Find("DogImage").GetComponent<Image>();

            contentTransform = scrollView.content;
            imgTransform = img.GetComponent<RectTransform>();
            if (contentTransform == null)
            {
                UnityEngine.Debug.LogError("contentTransform is null. Check if the ScrollRect is properly assigned.");
            }

            imgManager = GameObject.Find("ImgManager");

            DogsAdoptResult();
        }
        SceneManager.sceneLoaded -= OnOutputSceneLoaded;
    }

    public void DogsAdoptResult()
    {
        float buttonHeight = 30f; // 각 버튼의 높이 (필요에 따라 조정)
        float spacing = 0f; // 버튼 사이의 간격
        float startY = -buttonHeight / 2;

        for (int idx = 0; idx < dogNumbers.Count; idx++)
        {   
            // 버튼 생성
            Button IdDog = Instantiate(dogId, contentTransform).GetComponent<Button>();

            // RectTransform을 가져와 위치 설정
            RectTransform rt = IdDog.GetComponent<RectTransform>();
            if (rt != null)
            {
                // Top Center를 위한 anchor 설정
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                // Y축 기준으로 버튼의 위치 설정 (아래로 배치)
                rt.anchoredPosition = new Vector2(0, startY - (idx * (buttonHeight + spacing)));
            }

            // 버튼의 텍스트 설정
            IdDog.GetComponentInChildren<Text>().text = dogNumbers[idx];

            int localIdx = idx;
            // 클릭 이벤트 설정
            IdDog.onClick.AddListener(() => LoadResultImg(imgUrls[localIdx]));
        }
    }
    

    public void LoadResultImg(string url)
    {

        if (imgManager != null)
        {
            imgManager.GetComponent<LoadImg>().LoadDogImg(url, (Texture texture) => {
                RawImage imgDog = Instantiate(dogImg, imgTransform).GetComponent<RawImage>();
                imgDog.texture = texture;
                RectTransform rt = imgDog.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(150, 150); // RawImage 크기 설정
                rt.anchoredPosition = new Vector2(0, 0); // 적절한 위치 설정

                // 부모의 anchor 설정에 따라 위치를 조정해 줄 수 있음
                rt.anchorMin = new Vector2(0.5f, 0.5f); // 부모의 가운데에 배치
                rt.anchorMax = new Vector2(0.5f, 0.5f); // 부모의 가운데에 배치
                rt.pivot = new Vector2(0.5f, 0.5f);     // 중심점을 가운데로 설정
            });
        }
        else
        {
            Debug.LogError("imgManager is null or destroyed. Cannot load image.");
        }
    }

}
