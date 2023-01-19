using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HutongGames.PlayMaker.Actions;
using MSCLoader;
using UnityEngine;

namespace RepairHammerRedux
{
    // defines a component that waits 2 seconds before changing the baseVertices of all car deformables to undeformed data from fleetari's jobs
    // a good idea is to slap it onto the DeformLogic gameobject
    public class DeformableUpdater : MonoBehaviour
    {
        private PlayMakerFSM bodyfix_FSM;

        private System.Type deformableType;
        private System.Reflection.FieldInfo deformableBaseVertices;

        private void Start()
        {
            this.deformableType = typeof(Deformable);
            this.deformableBaseVertices = this.deformableType.GetField("baseVertices", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            // Make sure that all deformables get enabled by DeformLogic, resetting their mesh data to the latest MeshFiter (damaged)
            PlayMakerFSM deform_logic_FSM = GameObject.Find("SATSUMA(557kg, 248)/DeformLogic").GetComponent<PlayMakerFSM>();
            deform_logic_FSM.SendEvent("HAMMER"); // this will enable all the deformables gracefully and disable them after one second

            ModConsole.Log("Hang on, making the repair hammer work...");
            base.StartCoroutine(WaitForDeformables(2f, RetrievePristineCarMeshes));

            // now hear me out : we don't need to do any of this more than once because
            // the only thing that should ever erase the baseVertices during gameplay is fleetari repairing the car
            // which is obviously a good thing
        }

        private System.Collections.IEnumerator WaitForDeformables(float waitTime, System.Action afteraction)
        {
            yield return new WaitForSeconds(waitTime);
            afteraction();
        }

        private void RetrievePristineCarMeshes()
        {
            
            // retrieve the body fixing jobs from fleetari's
            // keep in mind that all the Jobs are disabled at load and might not be fully loaded
            GameObject repair_shop = GameObject.Find("REPAIRSHOP");
            this.bodyfix_FSM = repair_shop.transform.Find("Jobs/Bodyfix").GetComponent<PlayMakerFSM>();

            if (!repair_shop.transform.Find("Jobs").gameObject.activeInHierarchy)
            {
                repair_shop.transform.Find("Jobs").gameObject.SetActive(true);
                repair_shop.transform.Find("Jobs").gameObject.SetActive(false);

            }
            ReplaceBodyBaseVertices();
            ReplaceDetachablesBaseVertices();
            ModConsole.Log("Base vertices replaced, the repair hammer is now fully functional.");
            this.enabled = false;
        }

        void ReplaceBodyBaseVertices()
        {
            // so all the meshes we want should be stored inside that bodyfix job's FSM
            // all we need is to grab em and store them inside the proper deformables's basevertices attributes
            try
            {
                GameObject satsuma = GameObject.Find("SATSUMA(557kg, 248)");

                // since the main body deformables aren't adjacent to their meshfilter (unlike the detachable parts), we need to treat them separately
                Deformable[] car_main_deformables = satsuma.GetComponents<Deformable>();
                Deformable body_deformable = DeformableUtils.byMeshFilterName(car_main_deformables, "car body(xxxxx)");
                Deformable body_masse_deformable = DeformableUtils.byMeshFilterName(car_main_deformables, "car body masse(xxxxx)");

                // the "Fix" state holds the data for the the main body meshes in its first two actions
                SetProperty fix_body = PlayMakerExtensions.GetAction<SetProperty>(PlayMakerExtensions.GetState(this.bodyfix_FSM, "Fix"), 0);
                SetProperty fix_body_masse = PlayMakerExtensions.GetAction<SetProperty>(PlayMakerExtensions.GetState(this.bodyfix_FSM, "Fix"), 1);

                // the data we want is the parameter to these actions
                Mesh pristine_body = fix_body.targetProperty.ObjectParameter.Value as Mesh;
                Mesh pristine_body_masse = fix_body_masse.targetProperty.ObjectParameter.Value as Mesh;

                // fleetari just replaces the mesh data in the deformable's mesh filter
                // forcing it to update the inner mesh data on the next FixedUpdate
                // we, on the other hand, update the basevertices directly, not changing anything else

                this.deformableBaseVertices.SetValue(body_deformable, pristine_body.vertices);
                this.deformableBaseVertices.SetValue(body_masse_deformable, pristine_body_masse.vertices);
            }
            catch (System.Exception e)
            {
                ModConsole.LogError("Something went wrong fetching base body meshes, report this");
                throw e;
            }
        }

        void ReplaceDetachablesBaseVertices()
        {
            // very meaningful state names corresponding to every step of the bodyfix job for each detachable deformable, thanks topless
            string[] state_names = { "Fix2", "Fix2 2", "Fix2 3", "Fix2 4", "Fix2 5", "Fix2 6", "Fix2 7", "Fix2 8", "Fix2 9", "Fix2 10" };
            foreach (string state_name in state_names)
            {
                try
                {
                    SetProperty fix_part = this.bodyfix_FSM.GetState(state_name).Actions[0] as SetProperty;
                    Mesh pristine_mesh = fix_part.targetProperty.ObjectParameter.Value as Mesh;
                    MeshFilter current_meshfilter = fix_part.targetProperty.TargetObject.Value as MeshFilter;
                    Deformable detachable = current_meshfilter.gameObject.GetComponent<Deformable>();
                    this.deformableBaseVertices.SetValue(detachable, pristine_mesh.vertices);
                }
                catch (System.Exception e)
                {
                    ModConsole.LogError("Something went wrong fetching pristine mesh data for state " + state_name + ", report this");
                    throw e;
                }
            }
        }
    }
}
