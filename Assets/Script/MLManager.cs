using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Diagnostics;
using System.Text;
using UnityEngine;

public class MLManager : MonoBehaviour
{
    public IEnumerator ConnectModel(string numInput, int timeoutSeconds = 600)
    {
        string pythonExePath = Application.streamingAssetsPath + "/ML/.venv/Scripts/python.exe";
        string pythonScriptPath = Application.streamingAssetsPath + "/ML/run.py";   
        int num =  int.Parse(numInput);

        Process process = new Process();
        process.StartInfo.FileName = pythonExePath;
        process.StartInfo.Arguments = $"\"{pythonScriptPath}\"";
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
        process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";

        StringBuilder outputBuilder = new StringBuilder();
        StringBuilder errorBuilder = new StringBuilder();

        // 실시간 출력을 유니티 콘솔에 표시
        process.OutputDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                outputBuilder.AppendLine(args.Data); // 출력을 저장
                UnityEngine.Debug.Log("Python Output: " + args.Data);
            }
        };

        process.ErrorDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                errorBuilder.AppendLine(args.Data); // 오류를 저장
                UnityEngine.Debug.LogError("Python Error: " + args.Data);
            }
        };

        try
        {
            process.Start();
            // 실시간 스트리밍 활성화
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("Failed to start Python process: " + ex.Message);
            yield break;
        }

        // Python에 입력값 전달
        using (StreamWriter writer = process.StandardInput)
        {
            writer.WriteLine(num);
        }

        // 타임아웃 설정
        float startTime = Time.time;

        // 프로세스가 끝날 때까지 기다림
        while (!process.HasExited)
        {
            if (Time.time - startTime > timeoutSeconds)
            {
                UnityEngine.Debug.LogError("Python process timed out.");
                process.Kill(); // 프로세스 종료
                yield break;
            }
            yield return null; // 다음 프레임까지 대기
        }

        // 프로세스 종료 후 출력과 오류 처리
        process.WaitForExit();
    }
}

