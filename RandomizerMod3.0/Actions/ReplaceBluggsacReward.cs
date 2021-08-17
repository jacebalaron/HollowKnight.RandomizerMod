using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HutongGames.PlayMaker;
using SereCore;
using UnityEngine;
using RandomizerMod.FsmStateActions;

namespace RandomizerMod.Actions
{
    public class ReplaceBluggsacReward : RandomizerAction
    {
        private readonly string _sceneName;
        private readonly string _shinyName;

        public ReplaceBluggsacReward(string sceneName, string shinyName)
        {
            _sceneName = sceneName;
            _shinyName = shinyName;
        }

        public override ActionType Type => ActionType.PlayMakerFSM;

        public override void Process(string scene, UnityEngine.Object changeObj)
        {
            if (!(scene == _sceneName && changeObj is PlayMakerFSM fsm && fsm.FsmName == "Control" && fsm.gameObject.name.StartsWith("Corpse Egg Sac")))
            {
                return;
            }

            FsmState init = fsm.GetState("Init");
            init.Actions[1] = new RandomizerExecuteLambda(() =>
            {
                fsm.FsmVariables.GetFsmGameObject("Egg").Value = GameObject.Find(_shinyName + " Parent").FindGameObjectInChildren(_shinyName);
            });
        }
    }
}
