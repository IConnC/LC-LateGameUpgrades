﻿using GameNetcodeStuff;
using MoreShipUpgrades.Managers;
using MoreShipUpgrades.Misc;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace MoreShipUpgrades.UpgradeComponents
{
    public class defibScript : BaseUpgrade
    {
        void Start()
        {
            DontDestroyOnLoad(gameObject);
            UpgradeBus.instance.UpgradeObjects.Add("Interns", gameObject);
            UpgradeBus.instance.internScript = this;
        }

        public override void Register()
        {
            if(!UpgradeBus.instance.UpgradeObjects.ContainsKey("Interns")) { UpgradeBus.instance.UpgradeObjects.Add("Interns", gameObject); }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ReviveTargetedPlayerServerRpc()
        {
            ReviveTargetedPlayerClientRpc();

            Vector3 vector = RoundManager.Instance.insideAINodes[UnityEngine.Random.Range(0, RoundManager.Instance.insideAINodes.Length)].transform.position;
            vector = RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(vector, 10f, default(NavMeshHit));
            NetworkBehaviourReference netRef = new NetworkBehaviourReference(StartOfRound.Instance.mapScreen.targetedPlayer);

            TelePlayerClientRpc(vector, netRef);
        }

        [ClientRpc]
        private void TelePlayerClientRpc(Vector3 vector, NetworkBehaviourReference netRef)
        {
            netRef.TryGet(out NetworkBehaviour player);
            if(player != null)
            {
                player.transform.GetComponent<PlayerControllerB>().TeleportPlayer(vector);
            }
        }

        [ClientRpc]
        private void ReviveTargetedPlayerClientRpc()
        {
            PlayerControllerB player = StartOfRound.Instance.mapScreen.targetedPlayer;

            player.ResetPlayerBloodObjects(player.isPlayerDead);
            if (player.isPlayerDead || player.isPlayerControlled)
            {
                player.isClimbingLadder = false;
                player.ResetZAndXRotation();
                player.thisController.enabled = true;
                player.health = 100;
                player.disableLookInput = false;
                if (player.isPlayerDead)
                {
                    player.isPlayerDead = false;
                    player.isPlayerControlled = true;
                    player.isInElevator = true;
                    player.isInHangarShipRoom = true;
                    player.isInsideFactory = false;
                    player.wasInElevatorLastFrame = false;
                    StartOfRound.Instance.SetPlayerObjectExtrapolate(false);
                    player.setPositionOfDeadPlayer = false;
                    player.helmetLight.enabled = false;
                    player.Crouch(false);
                    player.criticallyInjured = false;
                    if (player.playerBodyAnimator != null)
                    {
                        player.playerBodyAnimator.SetBool("Limp", false);
                    }
                    player.bleedingHeavily = false;
                    player.activatingItem = false;
                    player.twoHanded = false;
                    player.inSpecialInteractAnimation = false;
                    player.disableSyncInAnimation = false;
                    player.inAnimationWithEnemy = null;
                    player.holdingWalkieTalkie = false;
                    player.speakingToWalkieTalkie = false;
                    player.isSinking = false;
                    player.isUnderwater = false;
                    player.sinkingValue = 0f;
                    player.statusEffectAudio.Stop();
                    player.DisableJetpackControlsLocally();
                    player.health = 100;
                    player.mapRadarDotAnimator.SetBool("dead", false);
                    if (player.IsOwner)
                    {
                        HUDManager.Instance.gasHelmetAnimator.SetBool("gasEmitting", false);
                        player.hasBegunSpectating = false;
                        HUDManager.Instance.RemoveSpectateUI();
                        HUDManager.Instance.gameOverAnimator.SetTrigger("revive");
                        player.hinderedMultiplier = 1f;
                        player.isMovementHindered = 0;
                        player.sourcesCausingSinking = 0;
                    }
                }
                SoundManager.Instance.earsRingingTimer = 0f;
                player.voiceMuffledByEnemy = false;
                if (player.currentVoiceChatIngameSettings == null)
                {
                    StartOfRound.Instance.RefreshPlayerVoicePlaybackObjects();
                }
                if (player.currentVoiceChatIngameSettings != null)
                {
                    if (player.currentVoiceChatIngameSettings.voiceAudio == null)
                    {
                        player.currentVoiceChatIngameSettings.InitializeComponents();
                    }
                    if (player.currentVoiceChatIngameSettings.voiceAudio == null)
                    {
                        return;
                    }
                    player.currentVoiceChatIngameSettings.voiceAudio.GetComponent<OccludeAudio>().overridingLowPass = false;
                }
            }
            StartOfRound.Instance.livingPlayers++;
            if(GameNetworkManager.Instance.localPlayerController == player)
            {
                player.bleedingHeavily = false;
                player.criticallyInjured = false;
                player.playerBodyAnimator.SetBool("Limp", false);
                player.health = 100;
                HUDManager.Instance.UpdateHealthUI(100, false);
                player.spectatedPlayerScript = null;
                HUDManager.Instance.audioListenerLowPass.enabled = false;
                StartOfRound.Instance.SetSpectateCameraToGameOverMode(false, player);
                StartOfRound.Instance.UpdatePlayerVoiceEffects();
            }
            else
            {
                player.thisPlayerModel.enabled = true;
                player.thisPlayerModelLOD1.enabled = true;
                player.thisPlayerModelLOD2.enabled = true;
            }
        }


    }
}
