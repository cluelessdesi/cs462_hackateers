using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// adding namespaces
using Unity.Netcode;

// this is just the target code, gonna modify later but the logic should be similar, just dont want to destroy the 
// platform and want the question to stay up while player stays on the platform
// also want buttons to show up for the user to choose answers, if theres time also implement heart system 
public class Platform : NetworkBehaviour
{

    private List<string> hints = new List<string>(){

        "A GPU (Graphics Processing Unit) is a specialized processor designed to handle many calculations at the same time.",
        "More hints to come :p",
        "Just want functionality for now"
    };
    //this method is called whenever a collision is detected
    private void OnTriggerEnter(Collider collision)
    {

        // printing if collision is detected on the console
        Debug.Log("Collision Detected");
        // we want to find who hit
        var hitNetObj = GetComponent<Collider>().gameObject.GetComponentInParent<NetworkObject>();
        ulong clientId = hitNetObj.OwnerClientId;
        // if the collision is detected destroy the object
        ShowQuestionToClient(clientId);
    }

    private void OnTriggerExit(Collider collision)
    {

        // printing if collision is detected on the console
        Debug.Log("Collision Detected");
        // we want to find who hit
        var hitNetObj = GetComponent<Collider>().gameObject.GetComponentInParent<NetworkObject>();
        ulong clientId = hitNetObj.OwnerClientId;
        // if the collision is detected destroy the object
        HideQuestionClientRpc(new ClientRpcParams
        {
            Send = new ClientRpcSendParams 
            { 
                TargetClientIds = new[] { clientId } 
            }
        });
    }

    private void ShowQuestionToClient(ulong clientId){
        Debug.Log($"[Platform] ShowQuestionClientRpc on client {NetworkManager.Singleton.LocalClientId}. UI instance? {HitMessageUI.Instance != null}");
        var rpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
        };
        string msg = "This will be the question";
        ShowQuestionClientRpc(msg, rpcParams);
    }

    // client can not spawn or destroy objects
    // so we need to use ServerRpc
    // we also need to add RequireOwnership = false
    // because we want to destroy the object even if the client is not the owner
    // [ServerRpc(RequireOwnership = false)]
    // public void DestroyTargetServerRpc()
    // {
    //     // get a random hint
    //     int index = Random.Range(0, hints.Count);
    //     string selected = hints[index];
    //     ShowHitClientRpc(selected);
    //     hints.RemoveAt(index);
    //     //despawn
    //     //GetComponent<NetworkObject>().Despawn(true);
    //     //after collision is detected destroy the gameobject
    //     //Destroy(gameObject);
    // }

    [ClientRpc]
    private void ShowQuestionClientRpc(string msg, ClientRpcParams clientRpcParams)
    {
        if (HitMessageUI.Instance != null)
            HitMessageUI.Instance.ShowLocalQ(msg);
    }
    [ClientRpc]
    private void HideQuestionClientRpc(ClientRpcParams clientRpcParams)
    {
        HitMessageUI.Instance.HideLocal();
    }
}