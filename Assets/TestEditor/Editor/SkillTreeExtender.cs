using UnityEditor;
using UnityEngine;
using MyExtender;

[CustomEditor(typeof(SkillTree))]
public class SkillTreeExtender : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);

        SkillTree tree = (SkillTree)target;

        if (GUILayout.Button("Add Child Object"))
        {
            Skill childObject = ScriptableObject.CreateInstance<Skill>();
            childObject.name = "SubItem";

            Undo.RegisterCreatedObjectUndo(childObject, "Create Child Object");

            AssetDatabase.AddObjectToAsset(childObject, tree);
            AssetDatabase.SaveAssets();

            tree.skills.Add(childObject);
            EditorUtility.SetDirty(tree);
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Remove All Child Objects"))
        {
            foreach (Skill childObject in tree.skills)
            {
                AssetDatabase.RemoveObjectFromAsset(childObject);
                DestroyImmediate(childObject, true);
            }

            tree.skills.Clear();
            AssetDatabase.SaveAssets();

            EditorUtility.SetDirty(tree);
        }
    }
}
