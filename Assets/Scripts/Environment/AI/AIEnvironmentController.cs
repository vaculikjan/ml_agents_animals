// Author: Jan Vaculik

using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TrainingUtils;
using UnityEngine;

namespace Environment
{
    public class AIEnvironmentController : MonoBehaviour
    {
        private const string CONFIG_PATH = "environment_config.json";
        
        [Header("Config")]
        [SerializeField]
        private EnvironmentConfig _EnvironmentConfig;
        
        [Header("Arena")]
        [SerializeField]
        private Bounds _ArenaBounds;
        
        [Header("Managers")]
        [SerializeField] 
        private FoodManager _FoodManager;
        [SerializeField]
        private AIDeerSpawner _DeerManager;
        [SerializeField]
        private AIWolfSpawner _WolvesManager;

        public static AIEnvironmentController Instance;
        public EnvironmentConfig EnvironmentConfig => _EnvironmentConfig;
        public Bounds ArenaBounds => _ArenaBounds;
        private string _environmentConfigPath;
        public string EnvironmentConfigPath => _environmentConfigPath;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                LoadSettings();
                ResetEnvironment();
            }
            else
            {
                Destroy(this);
            }
        }
        
        private void OnDrawGizmos()
        {
            var position = transform.position;
            _ArenaBounds?.DrawBounds(position);
            if (_FoodManager)
                _FoodManager.DrawBounds(position);
            if (_DeerManager)
                _DeerManager.DrawBounds(position);
        }

        private void ResetEnvironment()
        {
            _FoodManager.ResetFood();
            _DeerManager.ResetAgents();
            _WolvesManager.ResetAgents();
        }

        private void LoadSettings()
        {
            var args = System.Environment.GetCommandLineArgs();
            _environmentConfigPath = Path.Combine(Application.streamingAssetsPath, CONFIG_PATH);
        
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] == "-config" && i + 1 < args.Length)
                {
                    _environmentConfigPath = args[i + 1];
                    break;
                }
            }
            
            if (File.Exists(_environmentConfigPath))
            {
                var json = File.ReadAllText(_environmentConfigPath);
                JsonConvert.PopulateObject(json, _EnvironmentConfig);

            }
            else
            {
                Debug.LogError($"Unable to find environment config file at {_environmentConfigPath}! Using default values.");
            }
        }
        
        private static void AddDataItem(ILogData dataItem, Dictionary<string, ILogData> dataItemsByType)
        {
            var typeName = dataItem.GetType().Name;
            dataItemsByType.TryAdd(typeName, dataItem);
        }

        /*
        private void OnApplicationQuit()
        { 
            var data = new Dictionary<string, ILogData>();
            AddDataItem(_DeerManager.LogData(), data);
            AddDataItem(_WolvesManager.LogData(), data);
            AddDataItem(_EnvironmentConfig.LogData(), data);
    
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
    
            var directoryPath = string.IsNullOrEmpty(_environmentConfigPath) 
                ? Application.persistentDataPath 
                : Path.GetDirectoryName(_environmentConfigPath);

            if (directoryPath == null) return;
            
            var path = Path.Combine(directoryPath, "dataOutput.json");

            File.WriteAllText(path, json);
        }
        */
    }
}
