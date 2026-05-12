using UnityEngine;

public class URPMaterialFixer : MonoBehaviour
{
    public Color floorColor = new Color(0.45f, 0.45f, 0.45f, 1f);

    void Start()
    {

        Shader target = Shader.Find("Universal Render Pipeline/Lit")
                     ?? Shader.Find("Universal Render Pipeline/Unlit")
                     ?? Shader.Find("Standard");

        if (target == null)
        {
            Debug.LogWarning("[URPMaterialFixer] No se encontró ningún shader compatible.");
            return;
        }

        int fixedCount = 0;



        foreach (Renderer r in FindObjectsOfType<Renderer>())
        {

            if (r.GetComponentInParent<GazeTimerButton>()      != null) continue;
            if (r.GetComponentInParent<GazeTimerButtonMouse>() != null) continue;



            Material[] mats = r.materials;
            bool changed = false;

            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null) continue;

                string sName = mats[i].shader.name;


                if (!sName.Contains("Universal Render Pipeline") &&
                    !sName.Contains("Skybox"))
                {
                    var newMat  = new Material(target);
                    newMat.name = mats[i].name + "_URP_Fixed";
                    newMat.color = floorColor;
                    mats[i]     = newMat;
                    changed     = true;
                }
            }

            if (changed)
            {
                r.materials = mats;
                fixedCount++;
                Debug.Log($"[URPMaterialFixer] Material corregido en: {r.gameObject.name}");
            }
        }

        if (fixedCount == 0)
            Debug.Log("[URPMaterialFixer] Todos los materiales ya son compatibles con URP.");
        else
            Debug.Log($"[URPMaterialFixer] {fixedCount} material(es) corregido(s).");
    }
}
