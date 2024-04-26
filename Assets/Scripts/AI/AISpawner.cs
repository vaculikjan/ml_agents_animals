// Author: Jan Vaculik

using System;
using System.IO;
using Environment;
using UnityEngine;

namespace AI
{
    public abstract class AISpawner<T> : ASpawner<T> where T : MonoBehaviour
    {
        protected async void LogToFileAsync(string logMessage, string filePath)
        {
            try
            {
                await using StreamWriter streamWriter = File.AppendText(filePath);
                await streamWriter.WriteLineAsync(logMessage);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to log data: {ex.Message}");
            }
        }
    }
}
