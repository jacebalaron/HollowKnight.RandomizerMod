using System.Collections.Generic;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using SereCore;
using UnityEngine;
using UnityEngine.SceneManagement;
using RandomizerMod.FsmStateActions;
using static RandomizerMod.GiveItemActions;

namespace RandomizerMod.Actions
{
    public class CreateNewSoulTotem : RandomizerAction
    {
        private readonly float _x;
        private readonly float _y;
        private readonly string _sceneName;
        private readonly string _totemName;
        private readonly string _item;
        private readonly string _location;
        private readonly SoulTotemSubtype _subtype;

        public static Dictionary<SoulTotemSubtype, float> Elevation = new Dictionary<SoulTotemSubtype, float>() {
            [SoulTotemSubtype.A] = 0.5f,
            [SoulTotemSubtype.B] = -0.1f,
            [SoulTotemSubtype.C] = -0.1f,
            // Some elevation values adjusted from the originals to account for the shrinkage.
            [SoulTotemSubtype.D] = 1.3f - 0.3f,
            [SoulTotemSubtype.E] = 1.2f - 0.3f,
            [SoulTotemSubtype.F] = 0.8f,
            [SoulTotemSubtype.G] = 0.2f,
            [SoulTotemSubtype.Palace] = 1.3f - 0.3f,
            [SoulTotemSubtype.PathOfPain] = 1.5f - 0.9f,
        };

        public static Dictionary<SoulTotemSubtype, float> ShrinkageFactor = new Dictionary<SoulTotemSubtype, float>() {
            [SoulTotemSubtype.D] = 0.7f,
            [SoulTotemSubtype.E] = 0.7f,
            [SoulTotemSubtype.Palace] = 0.8f,
            [SoulTotemSubtype.PathOfPain] = 0.7f,
        };

        public CreateNewSoulTotem(string sceneName, float x, float y, string totemName, string item, string location, SoulTotemSubtype   subtype)
        {
            _sceneName = sceneName;
            _x = x;
            _y = y;
            _totemName = totemName;
            _item = item;
            _location = location;
            _subtype = subtype;
        }

        public override ActionType Type => ActionType.GameObject;

        public override void Process(string scene, Object changeObj)
        {
            if (scene != _sceneName)
            {
                return;
            }

            GameObject totem = ObjectCache.SoulTotem(_subtype);
            totem.name = _totemName;
            totem.transform.position = new Vector3(_x, _y, totem.transform.position.z);
            if (ShrinkageFactor.TryGetValue(_subtype, out var k))
            {
                var t = totem.transform;
                t.localScale = new Vector3(t.localScale.x * k, t.localScale.y * k, t.localScale.z);
            }
            totem.SetActive(true);
            SetSoul(totem, _item, _location);
        }

        public static void SetSoul(GameObject totem, string item, string location)
        {
            var fsm = FSMUtility.LocateFSM(totem, "soul_totem");
            var init = fsm.GetState("Init");
            init.RemoveActionsOfType<BoolTest>();
            init.RemoveActionsOfType<IntCompare>();
            init.AddAction(new RandomizerExecuteLambda(() => fsm.SendEvent(RandomizerMod.Instance.Settings.CheckLocationFound(location) ? "DEPLETED" : null)));
            var hit = fsm.GetState("Hit");
            // Path of Pain totems do not have a depleted state.
            if (fsm.GetState("Depleted") != null)
            {
                hit.ClearTransitions();
                hit.AddTransition("FINISHED", "Depleted");
            }
            hit.RemoveActionsOfType<IntCompare>();
            var giveSoul = hit.GetActionOfType<FlingObjectsFromGlobalPool>();
            giveSoul.spawnMin.Value = 100;
            giveSoul.spawnMax.Value = 101;
            hit.AddAction(new RandomizerExecuteLambda(() =>  GiveItem(GiveAction.None, item, location)));
        }
    }
}