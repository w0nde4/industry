#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ConnectionPoint))]
public class ConnectionPointEditor : Editor
{
    private void OnSceneGUI()
    {
        var point = (ConnectionPoint)target;
        
        EditorGUI.BeginChangeCheck();
        
        var newPos = Handles.PositionHandle(point.transform.position, Quaternion.identity);
        
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(point.transform, "Move Connection Point");
            point.transform.position = newPos;
        }
    }
}
#endif