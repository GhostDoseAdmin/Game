using InteractionSystem;
using UnityEngine;
using NetworkSystem;
using Newtonsoft.Json;
using GameManager;

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
        if (GameDriver.instance.twoPlayer) { NetworkDriver.instance.sioCom.Instance.Emit("event", JsonConvert.SerializeObject($"{{'obj':'{gameObject.name}','type':'key','event':'pickup','pass':'{keyPass}'}}"), false); }

        DestroyWithSound(false);
    }

    public string GetKeyPass()
    {
        return this.keyPass;
    }

    public void DestroyWithSound(bool otherPlayer)
    {
        if (!otherPlayer) { AudioManager.instance.Play("PickUp", GameObject.Find("Player").GetComponent<PlayerController>().audioSource); }
        else { AudioManager.instance.Play("PickUp", GameObject.Find("Client").GetComponent<ClientPlayerController>().audioSource); }
        Destroy(gameObject);
    }
}