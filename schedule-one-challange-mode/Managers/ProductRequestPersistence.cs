using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace challange_mode.Managers
{
    /// <summary>
    /// Handles persistence of product request tracking data
    /// </summary>
    [Serializable]
    public class ProductRequestSaveData
    {
        [Serializable]
        public class CustomerTrackingEntry
        {
            public string customerId;
            public string lastMessageTime; // ISO 8601 format
            public int failedRequestCount;
        }

        public List<CustomerTrackingEntry> trackingData = new List<CustomerTrackingEntry>();
    }

    public static class ProductRequestPersistence
    {
        private const string SAVE_FILE_NAME = "product_request_tracking.json";
        
        /// <summary>
        /// Get the full path to the save file
        /// </summary>
        private static string GetSaveFilePath()
        {
            // Save to game's persistent data path
            string directory = Path.Combine(Application.persistentDataPath, "ChallengeMod");
            
            // Ensure directory exists
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            return Path.Combine(directory, SAVE_FILE_NAME);
        }

        /// <summary>
        /// Save tracking data to disk
        /// </summary>
        public static void SaveData(Dictionary<string, DateTime> lastMessageTimes, 
                                    Dictionary<string, int> failedRequestCounts)
        {
            try
            {
                var saveData = new ProductRequestSaveData();

                // Combine data from both dictionaries
                var allCustomerIds = new HashSet<string>(lastMessageTimes.Keys);
                allCustomerIds.UnionWith(failedRequestCounts.Keys);

                foreach (var customerId in allCustomerIds)
                {
                    var entry = new ProductRequestSaveData.CustomerTrackingEntry
                    {
                        customerId = customerId,
                        lastMessageTime = lastMessageTimes.ContainsKey(customerId) 
                            ? lastMessageTimes[customerId].ToString("O") 
                            : string.Empty,
                        failedRequestCount = failedRequestCounts.ContainsKey(customerId) 
                            ? failedRequestCounts[customerId] 
                            : 0
                    };

                    saveData.trackingData.Add(entry);
                }

                string json = JsonUtility.ToJson(saveData, prettyPrint: true);
                string filePath = GetSaveFilePath();
                File.WriteAllText(filePath, json);

                Debug.Log($"[ProductRequestPersistence] Saved tracking data to {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ProductRequestPersistence] Error saving data: {ex.Message}");
            }
        }

        /// <summary>
        /// Load tracking data from disk
        /// </summary>
        public static void LoadData(out Dictionary<string, DateTime> lastMessageTimes,
                                    out Dictionary<string, int> failedRequestCounts)
        {
            lastMessageTimes = new Dictionary<string, DateTime>();
            failedRequestCounts = new Dictionary<string, int>();

            try
            {
                string filePath = GetSaveFilePath();

                if (!File.Exists(filePath))
                {
                    Debug.Log("[ProductRequestPersistence] No save file found, starting fresh");
                    return;
                }

                string json = File.ReadAllText(filePath);
                var saveData = JsonUtility.FromJson<ProductRequestSaveData>(json);

                if (saveData?.trackingData == null)
                {
                    Debug.LogWarning("[ProductRequestPersistence] Invalid save data");
                    return;
                }

                foreach (var entry in saveData.trackingData)
                {
                    // Parse last message time
                    if (!string.IsNullOrEmpty(entry.lastMessageTime))
                    {
                        if (DateTime.TryParse(entry.lastMessageTime, out DateTime parsedTime))
                        {
                            lastMessageTimes[entry.customerId] = parsedTime;
                        }
                    }

                    // Load failed request count
                    if (entry.failedRequestCount > 0)
                    {
                        failedRequestCounts[entry.customerId] = entry.failedRequestCount;
                    }
                }

                Debug.Log($"[ProductRequestPersistence] Loaded tracking data for {saveData.trackingData.Count} customers");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ProductRequestPersistence] Error loading data: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete the save file (for testing/cleanup)
        /// </summary>
        public static void ClearSaveData()
        {
            try
            {
                string filePath = GetSaveFilePath();
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log("[ProductRequestPersistence] Cleared save data");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ProductRequestPersistence] Error clearing data: {ex.Message}");
            }
        }
    }
}

