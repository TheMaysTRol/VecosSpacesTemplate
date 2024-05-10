using ExitGames.Client.Photon;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the loading, downloading, and handling of addressable assets.
/// </summary>
public class AddressablesLoader : MonoBehaviour
{

    public static AddressablesLoader Instance;
    public bool clearPreviousScene = false;
    SceneInstance previousLoadedScene;
    private bool isLoading = false;

    /// <summary>
    /// Awake method to set up the singleton instance.
    /// </summary>
    public void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        if (!Instance)
        {
            Instance = this;
        }
    }

    /// <summary>
    /// Start method to parse and process command-line arguments.
    /// </summary>
    void Start()
    {
        string[] args = System.Environment.GetCommandLineArgs();

        for (int i = 1; i < args.Length; i++) 
        {
            string arg = args[i];
            // Process the argument as needed
            if (arg.StartsWith("--param1="))
            {
                string param1Value = arg.Substring("--param1=".Length);
                Debug.LogError("Param1: " + param1Value);
            }
            else if (arg.StartsWith("--param2="))
            {
                string param2Value = arg.Substring("--param2=".Length);
                Debug.LogError("Param2: " + param2Value);
            }
        }
    }



    //Scene Handeling

    /// <summary>
    /// Loads an addressable scene with the specified key and invokes a callback when the scene is loaded.
    /// </summary>
    /// <param name="key">Key of the addressable scene to load.</param>
    /// <param name="SceneLoaded">Callback invoked when the scene is loaded.</param>
    public void LoadAddressableScene(string key, Action SceneLoaded = null, LoadSceneMode sceneType = LoadSceneMode.Single)
    {
        if (!isLoading)
        {
            isLoading = true;
            Debug.Log("Loading scene addressables....");
            if (PhotonAddressables.Instance)
            {
                PhotonAddressables.Instance.ClearAll();
            }
            if (clearPreviousScene)
            {
                Addressables.UnloadSceneAsync(previousLoadedScene).Completed += (asyncHandle) =>
                {
                    clearPreviousScene = false;
                    previousLoadedScene = new SceneInstance();
                    Debug.Log("Unloaded previous scene");
                };
            }
            PhotonNetwork.IsMessageQueueRunning = false;
            Addressables.LoadSceneAsync(key, sceneType).Completed += (asyncHandle) =>
            {
                clearPreviousScene = true;
                previousLoadedScene = asyncHandle.Result;
                //Debug.Log("Loaded scene");
                PhotonNetwork.IsMessageQueueRunning = true;
                Debug.Log("Succesfully loaded addressables scene");
                isLoading = false;
                if (SceneLoaded != null)
                {
                    SceneLoaded.Invoke();
                }
            };
        }

    }

    /// <summary>
    /// Initiates the download of addressable assets with progress tracking and completion callbacks.
    /// </summary>
    /// <param name="keys">List of keys for addressable assets to download.</param>
    /// <param name="onPercentageChange">Callback for tracking download progress.</param>
    /// <param name="onDownloadComplete">Callback invoked when the download is complete.</param>
    /// <param name="unloadAddressables">Flag indicating whether to unload addressables after download.</param>
    public void DownloadAddressable(List<string> keys, UnityAction<long, float> onPercentageChange, UnityAction<AsyncOperationHandle, List<string>> onDownloadComplete, bool unloadAddressables = true)
    {
        StartCoroutine(DownloadAddressableCor(keys, onPercentageChange, onDownloadComplete, unloadAddressables));
    }


    /// <summary>
    /// Retrieves the total size of addressable assets to be downloaded and invokes a callback with the size.
    /// </summary>
    /// <param name="keys">List of keys for addressable assets.</param>
    /// <param name="onGetSize">Callback for receiving the total size.</param>
    public void GetAddressablesTotalSize(List<string> keys, UnityAction<long> onGetSize)
    {
        long totalSize = 0;
        Addressables.GetDownloadSizeAsync(keys).Completed += (opHandle) =>
        {
            totalSize = opHandle.Result;
            onGetSize.Invoke(totalSize);
        };
    }


    /// <summary>
    /// Coroutine for downloading addressable assets, tracking progress, and handling completion.
    /// </summary>
    /// <param name="keys">List of keys for addressable assets to download.</param>
    /// <param name="onPercentageChange">Callback for tracking download progress.</param>
    /// <param name="onDownloadComplete">Callback invoked when the download is complete.</param>
    /// <param name="unloadAddressables">Flag indicating whether to unload addressables after download.</param>
    /// <returns>IEnumerator for the coroutine.</returns>
    private IEnumerator DownloadAddressableCor(List<string> keys, UnityAction<long, float> onPercentageChange, UnityAction<AsyncOperationHandle, List<string>> onDownloadComplete, bool unloadAddressables = true)
    {
        AsyncOperationHandle downloading;
        long totalSize = 0;
        Addressables.GetDownloadSizeAsync(keys).Completed += (opHandle) =>
        {
            totalSize = opHandle.Result;
        };
        yield return totalSize;
        downloading = Addressables.DownloadDependenciesAsync(keys, Addressables.MergeMode.Union, unloadAddressables);
        downloading.Completed += ((AsyncOperationHandle opHandle) =>
        {
            if (onDownloadComplete != null)
            {
                OnDownloadCompleted(opHandle, onDownloadComplete, keys);
            }
        });
        while (!downloading.IsDone)
        {
            yield return new WaitForEndOfFrame();
            if (downloading.IsValid())
            {
                var status = downloading.GetDownloadStatus();
                float progress = status.Percent;
                if (onPercentageChange != null)
                {
                    onPercentageChange.Invoke(totalSize, progress * 100);
                }
            }
        }
        yield return downloading;
    }


    /// <summary>
    /// Handles the completion of the addressable asset download operation.
    /// </summary>
    /// <param name="opHandle">AsyncOperationHandle for the download.</param>
    /// <param name="OnDownloadComplete">Callback invoked when the download is complete.</param>
    /// <param name="keys">List of keys downloaded.</param>
    public void OnDownloadCompleted(AsyncOperationHandle opHandle, UnityAction<AsyncOperationHandle, List<string>> OnDownloadComplete, List<string> keys)
    {
        if (opHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("Downloaded succesfully");
            OnDownloadComplete.Invoke(opHandle, keys);
        }
        else if (opHandle.Status == AsyncOperationStatus.Failed)
        {
            Debug.LogError("Download failed");
        }
    }

    //Bug fix in async loading , execture photon coroutines from this script

    /// <summary>
    /// Coroutine for retrying Photon RPC execution in case of failure.
    /// </summary>
    /// <param name="rpcData">Data for the RPC to be retried.</param>
    /// <param name="sender">Player sending the RPC.</param>
    /// <param name="retries">Number of retry attempts.</param>
    /// <returns>IEnumerator for the coroutine.</returns>
    IEnumerator ReExectueRPCs(ExitGames.Client.Photon.Hashtable rpcData, Photon.Realtime.Player sender, int retries = 0)
    {
        yield return new WaitForSeconds(2f);
        retries++;
        if (retries <= 5)
        {
            Debug.LogError("Retrying...");
            Debug.LogError(retries);
            PhotonNetwork.ExecuteRpc(rpcData, sender, retries);
        }
    }

    /// <summary>
    /// Retries Photon RPC execution.
    /// </summary>
    /// <param name="rpcData">Data for the RPC to be retried.</param>
    /// <param name="sender">Player sending the RPC.</param>
    /// <param name="retries">Number of retry attempts.</param>
    public void ReExectueRPC(ExitGames.Client.Photon.Hashtable rpcData, Photon.Realtime.Player sender, int retries = 0)
    {
        //StartCoroutine(ReExectueRPCs(rpcData, sender, retries));
    }

    //Bug fix in async event loading , execture photon coroutines from this script
    /// <summary>
    /// Coroutine for retrying Photon event execution in case of failure.
    /// </summary>
    /// <param name="photonEvent">Photon event data to be retried.</param>
    /// <param name="retries">Number of retry attempts.</param>
    /// <returns>IEnumerator for the coroutine.</returns>
    IEnumerator ReExectueEvents(EventData photonEvent, int retries = 0)
    {
        yield return new WaitForSeconds(2f);
        retries++;
        if (retries <= 5)
        {
            Debug.LogError("Retrying...");
            Debug.LogError(retries);
            PhotonNetwork.OnEvent2(photonEvent, retries);
        }
    }

    /// <summary>
    /// Retries Photon event execution.
    /// </summary>
    /// <param name="photonEvent">Photon event data to be retried.</param>
    /// <param name="retries">Number of retry attempts.</param>
    public void ReExectueEvent(EventData photonEvent, int retries = 0)
    {
        //StartCoroutine(ReExectueEvents(photonEvent, retries));
    }

}
