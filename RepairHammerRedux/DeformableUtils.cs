using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using MSCLoader;
using UnityEngine;

namespace RepairHammerRedux
{

#if DEBUG
    [HarmonyPatch(typeof(Deformable), "Awake")]
    class DeformableAwakePatch
    {
        static void Postfix(Deformable __instance, Vector3[] ___vertices, Vector3[] ___baseVertices, Mesh ___mesh)
        {
            ModConsole.Log("Just exited awake for deformable " + DeformableUtils.fullName(__instance));
            if (DeformableUtils.isMainBody(__instance))
            {
                ModConsole.Log("Car body has awaken :\n----------------------");
                ModConsole.Log(DeformableUtils.dataReport(___vertices, ___baseVertices, ___mesh));
                ModConsole.Log("----------------------\n");
            }
        }
    }

    [HarmonyPatch(typeof(Deformable), "LoadMesh")]
    class DeformableLoadMeshPatch
    {
        static void Postfix(Deformable __instance, Vector3[] ___vertices, Vector3[] ___baseVertices, Mesh ___mesh)
        {
            ModConsole.Log("Just exited LoadMesh for deformable " + DeformableUtils.fullName(__instance));
            if (DeformableUtils.isMainBody(__instance))
            {
                ModConsole.Log("Car body has Loaded Mesh :\n----------------------");
                ModConsole.Log(DeformableUtils.dataReport(___vertices, ___baseVertices, ___mesh));
                ModConsole.Log("----------------------\n");
            }
        }
    }
#endif
    internal class DeformableUtils
    {
#if DEBUG
        public static void SanityCheck(Deformable bodypart, UnityEngine.Vector3[] pristine_vertices) 
        {
            string bp_name = bodypart.ToString();
            Traverse def_info = Traverse.Create(bodypart);
            var def_vertices_field = def_info.Field("vertices");
            var def_baseVertices_field = def_info.Field("baseVertices");
            var def_mesh_field = def_info.Field("mesh");
            if (!def_mesh_field.FieldExists())
            {
                ModConsole.Log("what the fuck man");
            }

            var vertices = def_vertices_field.GetValue() as Vector3[];
            var baseVertices = def_vertices_field.GetValue() as Vector3[];
            var mesh = def_mesh_field.GetValue() as Mesh;

            if (mesh == null)
            {
                ModConsole.Log("mesh is null");
            }

            ModConsole.Log("is deformable.mesh readable ? " + mesh.isReadable);

            ModConsole.Log(bp_name + " pristine vs vertices " + differing_vertices(pristine_vertices, vertices));
            ModConsole.Log(bp_name + " pristine vs baseVertices " + differing_vertices(pristine_vertices, baseVertices));
            ModConsole.Log(bp_name + " pristine vs stored mesh " + differing_vertices(pristine_vertices, mesh.vertices));

        }

        public static int differing_vertices(Vector3[] list1, Vector3[] list2)
        {
            if (list1.Length != list2.Length)
            {
                return -999;
            }
            int count = 0;
            for (int i = 0; i < list1.Length; i++)
            {
                if (list1[i] != list2[i])
                {
                    count++;
                }
            }
            return count;
        }

        public static bool isMainBody(Deformable bodypart)
        {
            return hasMeshFilterNamed(bodypart, "car body(xxxxx)");
        }

        public static string dataReport(Vector3[] vertices, Vector3[] baseVertices, Mesh mesh)
        {
            string report = "Vertices : ";
            report += vertices.ToString();
            report += "(" + vertices.Length + ")\n";
            report += "baseVertices : ";
            report += baseVertices.ToString();
            report += "(" + baseVertices.Length + ")\n";
            report += "Mesh : ";
            report +=  mesh == null ? "null" : mesh.ToString();

            return report;
        }

        public static string fullName(Deformable bodypart)
        {
            return bodypart.name + "(" + bodypart.meshFilter.name + ")";
        }
#endif
        public static bool hasMeshFilterNamed(Deformable bodypart, string name)
        {
            return bodypart.meshFilter.name.StartsWith(name, System.StringComparison.Ordinal);
        }

        public static Deformable byMeshFilterName(Deformable[] deformables, string name)
        {
            return deformables.FirstOrDefault(
                    (Deformable x) => hasMeshFilterNamed(x, name)
                );
        }
    }
}
