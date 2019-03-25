using UnityEngine;
using System.Collections;
using UnityEditor;

namespace iningwei.AStar
{
    [CustomEditor(typeof(AStar))]
    public class AStarEditor : Editor
    {
        SerializedProperty gridSizeProp;
        SerializedProperty rowCountProp;
        SerializedProperty columnCountProp;

        void OnEnable()
        {
            gridSizeProp = serializedObject.FindProperty("gridSize");
            rowCountProp = serializedObject.FindProperty("rowCount");
            columnCountProp = serializedObject.FindProperty("columnCount");
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.DelayedFloatField(gridSizeProp, new GUIContent("grid size:"));
            EditorGUILayout.DelayedIntField(rowCountProp, new GUIContent("row count:"));
            EditorGUILayout.DelayedIntField(columnCountProp, new GUIContent("column count:"));


            serializedObject.ApplyModifiedProperties();
        }




    }
}