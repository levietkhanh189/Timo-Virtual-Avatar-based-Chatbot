using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class ChatbotAPI : MonoBehaviour
{
    // URL của API FastAPI
    private string apiUrl = "http://127.0.0.1:8000/ask";

    // This function will be called when the user submits a query
    public void OnSubmitQuery(string userQuery,Action<bool,string> onProcess)
    {
        if (!string.IsNullOrEmpty(userQuery))
        {
            // Start the API request coroutine
            StartCoroutine(SendQueryToAPI(userQuery, onProcess));
        }
        else
        {
            onProcess?.Invoke(false, "");
        }
    }

    // Coroutine to send the query to the API and receive the response
    private IEnumerator SendQueryToAPI(string query, Action<bool, string> onProcess)
    {
        // Create a new JSON object containing the user's query
        var jsonData = JsonConvert.SerializeObject(new { query = query });

        // Create a UnityWebRequest with POST method
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

            // Attach the request body
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();

            // Set the content type to JSON
            request.SetRequestHeader("Content-Type", "application/json");

            // Send the request and wait for the response
            yield return request.SendWebRequest();

            // Check for errors
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("API request failed: " + request.error);
                onProcess?.Invoke(false, request.error);
            }
            else
            {
                // Parse the JSON response
                string jsonResponse = request.downloadHandler.text;
                ApiResponse response = JsonConvert.DeserializeObject<ApiResponse>(jsonResponse);

                onProcess?.Invoke(true, response.answer);
            }
        }
    }

    // Class to represent the structure of the API response
    [System.Serializable]
    public class ApiResponse
    {
        public string question;
        public string answer;
    }
}
