﻿using GameNetcodeStuff;
using MoreShipUpgrades.Managers;
using MoreShipUpgrades.Misc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace MoreShipUpgrades.UpgradeComponents
{
    public class pagerScript : BaseUpgrade
    {

        void Start()
        {
            StartCoroutine(lateApply());
        }

        private IEnumerator lateApply()
        {
            yield return new WaitForSeconds(1);
            UpgradeBus.instance.pager = true;
            UpgradeBus.instance.pageScript = this;
            HUDManager.Instance.chatText.text += "\n<color=#FF0000>Pager is active!</color>";
            UpgradeBus.instance.UpgradeObjects.Add("Pager", gameObject);
            DontDestroyOnLoad(gameObject);
            load();
        }

        [ServerRpc(RequireOwnership = false)]
        public void ReqBroadcastChatServerRpc(string msg)
        {
            ReceiveChatClientRpc(msg);
        }

        [ClientRpc]
        public void ReceiveChatClientRpc(string msg)
        {
            HUDManager.Instance.chatText.text += $"\n<color=#FF0000>Terminal</color><color=#0000FF>:</color> <color=#FF00FF>{msg}</color>";
            HUDManager.Instance.PingHUDElement(HUDManager.Instance.Chat, 4f, 1f, 0.2f);
        }
    }
}
