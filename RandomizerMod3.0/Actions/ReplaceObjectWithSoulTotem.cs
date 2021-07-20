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
    public class ReplaceObjectWithSoulTotem : RandomizerAction
    {
        private readonly string _newTotemName;
        private readonly string _objectName;
        private readonly string _sceneName;
        private readonly float _elevation;
        private readonly SoulTotemSubtype _subtype;
        private readonly string _item;
        private readonly string _location;

        public static Dictionary<SoulTotemSubtype, float> Elevation = new Dictionary<SoulTotemSubtype, float>() {
            [SoulTotemSubtype.A] = 0.5f,
            [SoulTotemSubtype.B] = -0.1f,
            [SoulTotemSubtype.C] = -0.1f,
            [SoulTotemSubtype.D] = 1.3f,
            [SoulTotemSubtype.E] = 1.2f,
            [SoulTotemSubtype.F] = 0.8f,
            [SoulTotemSubtype.G] = 0.2f,
            [SoulTotemSubtype.Palace] = 1.3f,
            [SoulTotemSubtype.PathOfPain] = 1.5f,
        };

        public ReplaceObjectWithSoulTotem(string sceneName, string objectName, float elevation, string newTotemName, string item, string location, SoulTotemSubtype subtype)
        {
            _newTotemName = newTotemName;
            _objectName = objectName;
            _sceneName = sceneName;
            _elevation = elevation;
            _subtype = subtype;
            _item = item;
            _location = location;
        }

        public override ActionType Type => ActionType.GameObject;

        public override void Process(string scene, Object changeObj)
        {
            if (scene != _sceneName)
            {
                return;
            }

            Scene currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            string[] objectHierarchy = _objectName.Split('\\');
            int i = 1;
            GameObject obj = currentScene.FindGameObject(objectHierarchy[0]);
            while (i < objectHierarchy.Length)
            {
                obj = obj.FindGameObjectInChildren(objectHierarchy[i++]);
            }

            if (obj == null) return;

            GameObject totem = ObjectCache.SoulTotem(_subtype);
            totem.name = _newTotemName;
            if (obj.transform.parent != null)
            {
                totem.transform.SetParent(obj.transform.parent);
            }
            totem.transform.position = obj.transform.position;
            totem.transform.localPosition = obj.transform.localPosition;
            totem.transform.position += Vector3.up * (Elevation[_subtype] - _elevation);
            totem.SetActive(obj.activeSelf);
            SetSoul(totem, _item, _location);
            Object.Destroy(obj);
        }

        public static void SetSoul(GameObject totem, string item, string location)
        {
            var fsm = FSMUtility.LocateFSM(totem, "soul_totem");
            var init = fsm.GetState("Init");
            init.RemoveActionsOfType<BoolTest>();
            init.RemoveActionsOfType<IntCompare>();
            init.AddAction(new RandomizerExecuteLambda(() => fsm.SendEvent(RandomizerMod.Instance.Settings.CheckLocationFound(location) ? "DEPLETED" : null)));
            var hit = fsm.GetState("Hit");
            hit.ClearTransitions();
            hit.AddTransition("FINISHED", "Depleted");
            hit.RemoveActionsOfType<IntCompare>();
            var giveSoul = hit.GetActionOfType<FlingObjectsFromGlobalPool>();
            giveSoul.spawnMin.Value = 100;
            giveSoul.spawnMax.Value = 101;
            hit.AddAction(new RandomizerExecuteLambda(() =>  GiveItem(GiveAction.None, item, location)));
        }
    }
}