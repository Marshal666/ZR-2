using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Player))]
[CanEditMultipleObjects]
public class PlayerEditor : Editor
{

    SerializedProperty currentPos;

    private void OnEnable()
    {
        currentPos = serializedObject.FindProperty("CurrentPosition");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(currentPos);
        Player pl = (Player)target;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("State: ");
        if (pl && pl.PlayerState != null)
        {
            Player.PlayerStates st = (Player.PlayerStates)EditorGUILayout.EnumPopup(pl.PlayerState.State);
            if (st != pl.PlayerState.State)
            {
                pl.PlayerState.SwitchState(st);
            }
        }
        EditorGUILayout.EndHorizontal();

    }

}
