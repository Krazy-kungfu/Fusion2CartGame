using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class BananaPowerup : SpawnedPowerup {

    [Networked, OnChangedRender(nameof(OnColliderOn))] public NetworkBool isColliderOn { get; set;}
    public new Collider collider;
    public float enableDelay = 1f;
    
    [Networked] public TickTimer CollideTimer { get; set; }
    
    private void Awake() {
        //
        // We start the collider off as disabled, because the object may be predicted, so it takes time for FUN methods
        // to be called on this object. When the object has Spawned(), then the collider will be enabled.
        //
        collider.enabled = false;
        //isColliderOn = false;
    }

    public override void Spawned() {
        base.Spawned();

        AudioManager.PlayAndFollow("bananaDropSFX", transform, AudioManager.MixerTarget.SFX);

        //
        // We create a timer to count down so that the kart who spawned this object has time to drive away before the 
        // collider enables again. Without this, the person who drops the banana will spin themselves out!
        //
        if (Object.HasStateAuthority) {
            StartCoroutine(EnableCollider(enableDelay));
            //CollideTimer = TickTimer.CreateFromSeconds(Runner, enableDelay);
        }
    }

    public override void FixedUpdateNetwork() {
        base.FixedUpdateNetwork();

        //
        // We want to set this every frame because we dont want to accidentally enable this somewhere in code, because
        // that will mess up prediction somewhere.
        //
        //if (Object.HasStateAuthority) {
        //if(CollideTimer.ExpiredOrNotRunning(Runner)) {
        //    isColliderOn = true;
        //}
        //collider.enabled = CollideTimer.ExpiredOrNotRunning(Runner);
        //}
    }

    public override void Despawned(NetworkRunner runner, bool hasState) {
        base.Despawned(runner, hasState);
        isColliderOn = false;
        collider.enabled = false;
    }

    public IEnumerator EnableCollider(float time) {
        yield return new WaitForSeconds(time);
        isColliderOn = true;
    }

    public void OnColliderOn() {
        collider.enabled = isColliderOn;
    }

    public override bool Collide(KartEntity kart) {
        if ( Object.IsValid && !HasInit ) return false;

        kart.SpinOut();

        Runner.Despawn(Object);

        return true;
    }
}