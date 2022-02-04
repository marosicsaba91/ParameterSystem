#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PlayBox
{
public class VariableWindow : EditorWindow
{ 
    [SerializeField] Vector2 scrollPosition;
    const string editorPrefsKey = "Variable Window Info";
    const string windowName = "Variable Dashboard";
    const string menuItemName = "Tools/" + windowName;

    public void OnEnable()
    {
        Undo.undoRedoPerformed += Repaint;
        wantsMouseMove = true;
        string data = EditorPrefs.GetString(
            editorPrefsKey, JsonUtility.ToJson(this, prettyPrint: false));
        JsonUtility.FromJsonOverwrite(data, this); 
    }

    public void OnDisable()
    {
        Undo.undoRedoPerformed -= Repaint;
        string data = JsonUtility.ToJson(this, prettyPrint: false);
        EditorPrefs.SetString(editorPrefsKey, data);
    }

    [MenuItem(menuItemName)]
    static void ShowWindow()
    {
        var window = GetWindow<VariableWindow>();
        window.titleContent = new GUIContent(windowName);
        window.Show(); 
    }


    void OnGUI()
    {
        IEnumerable<Variable> variableTree = VariableHelper.AllGlobalVariables();
        var pos = position;
        pos.position = Vector2.zero;
        
        List<string> openedElements = VariableHelper.OpenedVariables;
        scrollPosition = VariableTreeDrawer.DrawVariables(variableTree, openedElements, scrollPosition, pos);
    }
}

} 
#endif