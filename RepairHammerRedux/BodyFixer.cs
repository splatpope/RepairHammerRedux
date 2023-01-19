using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using MSCLoader;
using UnityEngine;

namespace RepairHammerRedux
{
    // defines a component that, when added to some tool, enables that tool to repair car body parts by left clicking on them
    // currently, the only tool this can apply to is the Sledgehammer
    public class BodyFixer : MonoBehaviour
    {
        public static RepairHammerRedux mod_instance;
        public float repair_factor = 0.5f;
        public float repair_radius = 0.5f;

        private Animation tool_anim;
        private PlayMakerFSM tool_FSM;
        private bool swinging;

        private void Start()
        {
            this.tool_anim = base.transform.Find("Pivot").GetComponent<Animation>();
            this.tool_FSM = base.gameObject.GetComponent<PlayMakerFSM>();

            if (BodyFixer.mod_instance != null ) 
            {
                this.repair_factor = mod_instance.repairFactor.GetValue();
                this.repair_radius = mod_instance.repairRadius.GetValue();
            }
        }


        // u click brah ? bang on thang if we're not bangin on thang
        private void Update()
        {
            if (Input.GetMouseButton(0) && !this.swinging && this.tool_FSM.ActiveStateName == "State 1")
            {
                base.StartCoroutine(this.SwingTool());
            }
        }

        // cancel all banging and reset the tool
        private void OnDisable()
        {
            base.StopCoroutine(this.SwingTool());
            this.tool_FSM.enabled = true;
            this.swinging = false;
            this.tool_anim.Stop();
            this.tool_anim.transform.localEulerAngles = Vector3.zero;
        }

        // emulate FSM behavior for the tool that is being used to repair body damage
        // i.e. make it so the hammer swings and repairs
        private System.Collections.IEnumerator SwingTool()
        {
            this.swinging = true;
            this.tool_FSM.enabled = false; // ensure that normal hammer operation cannot be performed
            this.tool_anim.Play("sledgehammer_up");
            while (this.tool_anim.isPlaying)
            {
                yield return null;
            }
            this.tool_anim.Play("sledgehammer_hit");
            if (Camera.main != null)
            {
                this.Repair();
            }
            while (this.tool_anim.isPlaying)
            {
                yield return null;
            }
            this.tool_FSM.enabled = true;
            this.swinging = false;
            yield break;

        }

        private void Repair()
        {
            // if the tool should hit something, get what it hit, else do nothing
            if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit tool_raycast, 1.5f))
            {
                return;
            }
            // retrieve the deformable parts from what was hit (should only yield satsuma body parts)
            // note : these deformables should all be disabled per DeformLogic FSM normal operation
            Deformable[] body_parts = tool_raycast.collider.transform.root.GetComponentsInChildren<Deformable>(true);
            // do nothing if we hit nothing or what was hit has no deformables
            if (body_parts == null || !body_parts.Any<Deformable>())
            {
                return;
            }
            // for all the deformables we find, use the game's currently unused repair code to repair them, based on where the car was hit
            // then send the HAMMER event to the car's DeformLogic child's FSM so that the deformables are enabled and update their meshes
            PlayMakerFSM deform_logic_FSM = GameObject.Find("SATSUMA(557kg, 248)/DeformLogic").GetComponent<PlayMakerFSM>();
            // be aware that this will run deformable.Repair for ALL installed car body parts if you hit the car itself
            foreach (Deformable deformable in body_parts)
            {
                try
                {
                    // TODO : tweak this so things get repaired somewhat realistically
                    // TODO : not hardcode the various coefficients
                    deformable.Repair(this.repair_factor, tool_raycast.point, this.repair_radius); 
                    deform_logic_FSM.SendEvent("HAMMER");
                }
                catch
                {
                    ModConsole.LogError("Something went wrong repairing " + deformable.ToString());
                }
            }
            MasterAudio.PlaySound3DAtVector3AndForget("Crashes", tool_raycast.point, 0.1f, null, 0f, null);
        }
    }
    
}
