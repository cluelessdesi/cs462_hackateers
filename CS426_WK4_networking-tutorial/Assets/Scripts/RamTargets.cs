using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class RamTarget : NetworkBehaviour
{

    private static List<string> hints = new List<string>(){

        "RAM temporarily stores data and programs that the CPU is actively using.",
        "More RAM allows you to run more applications at once without slowing down.",
        "RAM is volatile, meaning everything in it is cleared when the computer powers off."
    };
   
    private void OnCollisionEnter(Collision collision)
    {

      
        Debug.Log("Collision Detected");
        // we want to find who hit
        NetworkObject hitNetObj = collision.gameObject.GetComponent<NetworkObject>();
 
        DestroyTargetServerRpc();
    }

    public int TargetsLeft()
    {
        return hints.Count;
    }



    [ServerRpc(RequireOwnership = false)]
    public void DestroyTargetServerRpc()
    {
        // get a random hint
        int index = Random.Range(0, hints.Count);
        string selected = hints[index];
        ShowHitClientRpc(selected);
        hints.RemoveAt(index);
        //despawn
        GetComponent<NetworkObject>().Despawn(true);
        Destroy(gameObject);
    }

    [ClientRpc]
    private void ShowHitClientRpc(string msg)
    {
        if (HitMessageUI.Instance != null)
            HitMessageUI.Instance.ShowLocal(msg);
    }
}