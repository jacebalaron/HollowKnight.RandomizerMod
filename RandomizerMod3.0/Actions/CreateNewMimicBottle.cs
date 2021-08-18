using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using SereCore;
using UnityEngine;
using RandomizerMod.FsmStateActions;
using static RandomizerMod.GiveItemActions;

namespace RandomizerMod.Actions
{
    internal class CreateNewMimicBottle : RandomizerAction
    {
        public const float MIMIC_BOTTLE_ELEVATION = 0.1f;

        private readonly string _bottleName;
        private readonly string _sceneName;
        private readonly float _x;
        private readonly float _y;
        private readonly string _item;
        private readonly string _location;
        private readonly bool _unrandomized;

        public CreateNewMimicBottle(string sceneName, float x, float y, string bottleName, string item, string location, bool unrandomized = false)
        {
            _sceneName = sceneName;
            _x = x;
            _y = y;
            _bottleName = bottleName;
            _item = item;
            _location = location;
            _unrandomized = unrandomized;
        }

        public override ActionType Type => ActionType.GameObject;

        public override void Process(string scene, Object changeObj)
        {
            if (scene != _sceneName)
            {
                return;
            }

            GameObject mimicBottle = ObjectCache.MimicBottle;
            mimicBottle.name = _bottleName + " Bottle";

            // Move the bottle forward so it appears in front of any background objects
            mimicBottle.transform.position = new Vector3(_x, _y, mimicBottle.transform.position.z - 0.1f);

            GameObject mimicTop = ObjectCache.MimicTop;
            mimicTop.name = _bottleName + " Top";
            mimicTop.transform.position = new Vector3(_x, _y - 0.2f, mimicTop.transform.position.z - 0.1f);

            GameObject mimic = mimicTop.FindGameObjectInChildren("Grub Mimic 1");
            mimic.name = _bottleName + " Mimic";
            Vector3 pos = mimic.transform.localPosition;
            mimic.transform.localPosition = new Vector3(pos.x, pos.y, pos.z - 0.1f);

            GameObject mimicDialogue = ObjectCache.MimicDreamDialogue;
            mimicDialogue.name = _bottleName + " Dialogue";
            mimicDialogue.transform.SetPosition2D(mimicTop.transform.position);

            FixMimicFSMs(mimicBottle, mimicTop, mimic, mimicDialogue, _item, _location, _sceneName, _unrandomized);

            mimicBottle.SetActive(true);
            mimicTop.SetActive(true);
            mimicDialogue.SetActive(true);
        }

        public static void FixMimicFSMs(GameObject bottle, GameObject top, GameObject mimic, GameObject dialogue, string item, string location, string sceneName, bool unrandomized)
        {
            // Use the game's in-built scenedata feature to decide what to do with the bottle and mimic
            PersistentBoolData bottleData = bottle.GetComponent<PersistentBoolItem>().persistentBoolData;
            bottleData.sceneName = sceneName;
            bottleData.id = location + " bottle";
            PersistentBoolData mimicData = mimic.GetComponent<PersistentBoolItem>().persistentBoolData;
            mimicData.sceneName = sceneName;
            mimicData.id = location + " mimic";

            // Correctly link the top to the bottle
            PlayMakerFSM bottleFSM = FSMUtility.LocateFSM(bottle, "Bottle Control");
            FsmState bottleInit = bottleFSM.GetState("Init");
            bottleInit.GetActionOfType<SendEventByName>().eventTarget.gameObject.GameObject.Value = top.gameObject;
            FsmState bottleShatter = bottleFSM.GetState("Shatter");
            bottleShatter.GetActionsOfType<SendEventByName>()[1].eventTarget.gameObject.GameObject.Value = top.gameObject;

            // Correctly link the Dialogue to the top
            PlayMakerFSM topFSM = FSMUtility.LocateFSM(top, "Grub Control");
            topFSM.GetState("Pause").GetActionOfType<SetParent>().gameObject.GameObject.Value = dialogue;

            // It's easiest to simply use the sceneData check to decide whether to destroy the bottle
            // bottleInit.RemoveActionsOfType<BoolTest>();
            // bottleInit.AddFirstAction(new RandomizerExecuteLambda(() => bottleFSM.SendEvent(RandomizerMod.Instance.Settings.CheckLocationFound(location) ? "ACTIVATE" : null)));

            // Run the randomizer code when we collect the grub mimic
            if (!unrandomized) bottleShatter.AddFirstAction(new RandomizerExecuteLambda(() => GiveItem(GiveAction.None, item, location)));
        }
    }
}
