using InteractionSystem;
using UnityEngine;
using NetworkSystem;
using Newtonsoft.Json;

public class Key : Item
{
    [Header("KEY PASSWORD")] //Key password (must match the door password)
    [Space(10)]
    [SerializeField] private string keyPass;

    [Header("KEY SOUND")]
    [Space(10)]
    [SerializeField] private string pickUpKey;

    public override void ActivateObject(bool otherPlayer)
    {
        this.AddKey();
    }

    private void AddKey()
    {
        KeyInventory.instance.AddKey(keyPass);
        AudioManager.instance.Play(pickUpKey, audioSource);
        NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject($"{{'obj':'{gameObject.name}','type':'key','event':'pickup','pass':'{keyPass}'}}"), false);

        this.DestroyObject(0);
       
        //EMIT PICKUP, on networkdriver send other player info to destroy the key, if it doesnt exist, remove that key from player inventory so there are no duplicates
    }

    public string GetKeyPass()
    {
        return this.keyPass;
    }
}