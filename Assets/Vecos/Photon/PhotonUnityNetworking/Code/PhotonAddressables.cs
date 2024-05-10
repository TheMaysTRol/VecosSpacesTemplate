using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


/// <summary>
/// Manages Photon-related operations with Addressable Assets .
/// </summary>
public class PhotonAddressables : MonoBehaviour, IOnEventCallback
{
    public static PhotonAddressables Instance;

    public Dictionary<string, BundleTrack> ListOfAddressablesObjects = new Dictionary<string, BundleTrack>();

    /// <summary>
    /// Awake method to set up the singleton instance.
    /// </summary>
    public void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
    }

    /// <summary>
    /// Enables Photon callbacks on object enable.
    /// </summary>
    private void OnEnable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
        PhotonNetwork.AddCallbackTarget(this);
    }

    /// <summary>
    /// Disables Photon callbacks on object disable.
    /// </summary>
    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }


    /// <summary>
    /// Destroys a GameObject instantiated from an Addressable Asset.
    /// </summary>
    /// <param name="gameObject">GameObject to destroy.</param>
    public void Destroy(GameObject gameObject)
    {
        string path = "";
        var runtimeMade = gameObject.GetComponent<PhotonAddressablesPath>();
        if (runtimeMade)
        {
            path = runtimeMade.addressablesPath;
        }
        var a = Addressables.ReleaseInstance(gameObject);
        if (!a)
        {
            GameObject.Destroy(gameObject);
            //Debug.LogError("Normal destroy");
        }
        if (ListOfAddressablesObjects.ContainsKey(path))
        {
            if (ListOfAddressablesObjects[path].handle.IsValid())
            {
                Addressables.Release(ListOfAddressablesObjects[path].handle.Result);
                //Addressables.Release(ListOfAddressablesObjects[path].handle);
            }
            //Debug.LogError("Released");
            ListOfAddressablesObjects.Remove(path);
        }
    }

    /// <summary>
    /// Instantiates a GameObject from an Addressable Asset.
    /// </summary>
    /// <param name="prefabId">ID of the Addressable Asset prefab.</param>
    /// <param name="position">Position to instantiate the GameObject.</param>
    /// <param name="rotation">Rotation of the instantiated GameObject.</param>
    /// <returns>Task returning the instantiated GameObject.</returns>
    public async Task<GameObject> Instantiate(string prefabId, Vector3 position, Quaternion rotation)
    {
        GameObject instance = null;
        var handle = Addressables.LoadAssetAsync<GameObject>(prefabId + ".prefab");
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject prefab = handle.Result;
            prefab.SetActive(false);

            instance = Instantiate(prefab, position, rotation);
            PhotonAddressablesPath run = instance.AddComponent<PhotonAddressablesPath>();
            if (run != null)
            {
                string uniqueId = Guid.NewGuid().ToString();
                string path = (prefabId) + uniqueId;
                run.addressablesPath = path;
                if (!ListOfAddressablesObjects.ContainsKey((path)))
                {
                    BundleTrack bun = new BundleTrack(path, handle);
                    ListOfAddressablesObjects.Add(path, bun);
                }
            }
        }
        //Debug.LogError("Instantiating object from photon pool");
        return instance;
    }

    private byte ManualInstantiationCodePhoton = 22;
    private byte ManualDestroyCodePhoton = 23;

    /// <summary>
    /// Spawns a GameObject using Photon and Addressable Assets.
    /// </summary>
    /// <param name="prefabName">Name of the prefab to spawn.</param>
    /// <param name="position">Position to spawn the GameObject.</param>
    /// <param name="rotation">Rotation of the spawned GameObject.</param>
    /// <param name="group">Photon group ID.</param>
    /// <param name="customData">Custom data for spawning.</param>
    /// <returns>Task returning the spawned GameObject.</returns>
    public async Task<GameObject> SpawnObject(string prefabName, Vector3 position, Quaternion rotation, int group, object[] customData)
    {
        Hashtable data = new Hashtable();
        GameObject myobject = await Instantiate(prefabName, position, rotation); //look for object to instantiate here using prefab name
        PhotonView photonView = myobject.GetComponent<PhotonView>();
        if (PhotonNetwork.AllocateViewID(photonView))
        {
            PhotonView[] childViews = myobject.GetComponentsInChildren<PhotonView>();
            if (childViews.Length > 1)
            {
                List<int> childsIds = new List<int>();
                for (int i = 0; i < childViews.Length; i++)
                {
                    if (childViews[i].gameObject == photonView.gameObject)
                    {
                        continue;
                    }
                    if (PhotonNetwork.AllocateViewID(childViews[i]))
                    {
                        childsIds.Add(childViews[i].ViewID);
                    }
                }
                data.Add("childsIds", childsIds.ToArray());
            }


            data.Add("prefabname", prefabName);
            data.Add("position", myobject.transform.position);
            data.Add("rotation", myobject.transform.rotation);
            data.Add("photonviewid", photonView.ViewID);

            data.Add("customdata", customData);

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.Others,
                CachingOption = EventCaching.AddToRoomCache
            };

            SendOptions sendOptions = new SendOptions
            {
                Reliability = true
            };
            photonView.InstantiationData = customData;

            PhotonNetwork.RaiseEvent(ManualInstantiationCodePhoton, data, raiseEventOptions, sendOptions);
            myobject.SetActive(true);
            PhotonMessageInfo info = new PhotonMessageInfo(null, 0, photonView);
            myobject.GetComponent<IPunInstantiateMagicCallback>().OnPhotonInstantiate(info);
            photonView.onDestroyEvent.AddListener(OnPhotonDestroyCallBack);
        }
        else
        {
            Debug.LogError("Failed to allocate a ViewId.");

            this.Destroy(myobject);
        }
        return myobject;
    }


    /// <summary>
    /// Deletes a GameObject and notifies other clients via Photon.
    /// </summary>
    /// <param name="myobject">GameObject to delete.</param>
    public void DeleteObject(GameObject myobject)
    {
        PhotonView photonView = myobject.GetComponent<PhotonView>();
        if (photonView && photonView.IsMine)
        {
            Hashtable data = new Hashtable();
            data["photonviewid"] = photonView.ViewID;

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.Others,
                CachingOption = EventCaching.DoNotCache
            };

            SendOptions sendOptions = new SendOptions
            {
                Reliability = true
            };

            RemoveFromRoomCache(photonView.ViewID);
            PhotonNetwork.RaiseEvent(ManualDestroyCodePhoton, data, raiseEventOptions, sendOptions);
            this.Destroy(myobject);
        }
    }


    /// <summary>
    /// Removes a Photon View from the room cache.
    /// </summary>
    /// <param name="photonView">Photon View ID to remove.</param>
    /// <returns>True if removal from the room cache is successful, otherwise false.</returns>
    public bool RemoveFromRoomCache(int photonView)
    {
        SendOptions sendOptions = new SendOptions
        {
            Reliability = true
        };
        Hashtable myhash = new Hashtable();

        myhash.Add("photonviewid", photonView);
        return PhotonNetwork.RaiseEvent(
                eventCode: ManualInstantiationCodePhoton,
                eventContent: myhash,
                sendOptions: sendOptions,
                raiseEventOptions: new RaiseEventOptions
                {
                    Receivers = ReceiverGroup.Others,
                    CachingOption = EventCaching.RemoveFromRoomCache
                });
    }

    /// <summary>
    /// Handles incoming Photon events.
    /// </summary>
    /// <param name="photonEvent">Photon event data.</param>
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == ManualInstantiationCodePhoton)
        {
            Debug.LogError("I RECIEVED THE INSTANTIATING CODE !!!");
            LoadAsyncObject(photonEvent);
        }
        else if (photonEvent.Code == ManualDestroyCodePhoton)
        {
            Hashtable data = (Hashtable)photonEvent.CustomData;
            PhotonView pv = PhotonNetwork.GetPhotonView((int)data["photonviewid"]);
            if (pv)
            {
                this.Destroy(pv.gameObject);
            }
            Debug.LogError("Recieved the delete code and deleted object");
        }
    }

    /// <summary>
    /// Loads an Addressable Asset asynchronously based on a Photon event.
    /// </summary>
    /// <param name="photonEvent">Photon event data.</param>
    public async void LoadAsyncObject(EventData photonEvent)
    {
        PhotonNetwork.IsMessageQueueRunning = false;
        Hashtable data = (Hashtable)photonEvent.CustomData;
        GameObject myobject = await Instantiate((string)data["prefabname"], (Vector3)data["position"], (Quaternion)data["rotation"]); //look for object to instantiate here using prefab name
        PhotonView photonView = myobject.GetComponent<PhotonView>();
        photonView.ViewID = (int)data["photonviewid"];
        Debug.LogError("Spawning " + photonView.name + photonView.ViewID);
        myobject.SetActive(true);
        photonView.InstantiationData = (object[])data["customdata"];
        PutChildsIds(photonEvent, myobject);
        PhotonMessageInfo info = new PhotonMessageInfo(null, 0, photonView);
        photonView.onDestroyEvent.AddListener(OnPhotonDestroyCallBack);
        Debug.LogError("Spawned " + photonView.name + photonView.ViewID);
        myobject.GetComponent<IPunInstantiateMagicCallback>().OnPhotonInstantiate(info);
        PhotonNetwork.IsMessageQueueRunning = true;
    }

    public void PutChildsIds(EventData photonEvent, GameObject myobject)
    {
        Hashtable data = (Hashtable)photonEvent.CustomData;
        bool doesItExist = data.TryGetValue("childsIds", out object obj);
        if (doesItExist)
        {
            int[] childsIds = (int[])obj;
            List<PhotonView> childViews = myobject.GetComponentsInChildren<PhotonView>().ToList();
            childViews.Remove(myobject.GetPhotonView());
            if (childsIds.Length > 0 && childsIds.Length == childViews.Count)
            {
                Debug.Log("weslo " + childsIds.Length);
                for (int i = 0; i < childsIds.Length; i++)
                {
                    Debug.Log(childViews[i].name + ": " + childsIds[i]);
                    childViews[i].ViewID = childsIds[i];
                }
            }
        }
    }

    /// <summary>
    /// Callback method when a GameObject is destroyed via Photon.
    /// </summary>
    /// <param name="myobject">Destroyed GameObject.</param>
    /// <param name="myview">PhotonView associated with the destroyed GameObject.</param>
    public void OnPhotonDestroyCallBack(GameObject myobject, PhotonView myview)
    {
        myview.onDestroyEvent.RemoveListener(OnPhotonDestroyCallBack);
    }

    /// <summary>
    /// Clears all loaded Addressable Assets.
    /// </summary>
    public void ClearAll()
    {
        foreach (BundleTrack a in ListOfAddressablesObjects.Values)
        {
            // Addressables.Release(a.handle.Result);
            // Ensure we release the handle when the script is destroyed
            if (a.handle.IsValid())
            {
                Debug.LogError("it is valid!!");
                Addressables.Release(a.handle);
            }
        }
        ListOfAddressablesObjects.Clear();

        //Debug.LogError("Cleared all ram from objects");
    }

    /// <summary>
    /// Extracts the folder path from a given path.
    /// </summary>
    /// <param name="path">Input path.</param>
    /// <returns>Extracted folder path.</returns>
    public string ExtractFolderPath(string path)
    {
        int lastSlashIndex = path.LastIndexOf('/');
        if (lastSlashIndex >= 0)
        {
            return path.Substring(0, lastSlashIndex);
        }
        else
        {
            // If no slash is found, return the original path as there is no folder path to extract.
            return path;
        }
    }
}

[Serializable]
/// <summary>
/// Represents a bundle track for tracking loaded Addressable Assets.
/// </summary>
public class BundleTrack
{
    public string path = "";
    public AsyncOperationHandle handle;
    public int count = 0;

    /// <summary>
    /// Constructor for BundleTrack.
    /// </summary>
    /// <param name="path">Path of the Addressable Asset.</param>
    /// <param name="handle">AsyncOperationHandle for the loaded Addressable Asset.</param>
    public BundleTrack(string path, AsyncOperationHandle handle)
    {
        this.path = path;
        this.handle = handle;
        this.count = 1;
    }
}
