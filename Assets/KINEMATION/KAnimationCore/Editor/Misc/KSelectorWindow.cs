
using KINEMATION.KAnimationCore.Editor.Rig;

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KINEMATION.KAnimationCore.Editor.Misc
{
    public class KSelectorWindow : EditorWindow
    {
        private OnTreeItemClicked _onClicked;
        private OnSelectionChanged _onSelectionChanged;
        
        private Vector2 _scrollPosition;
        private string _searchEntry = string.Empty;

        private RigTreeWidget _rigTreeWidget;
        private bool _useSelection = false;
        private List<(string, int)> _selectedItems;
        
        public static void ShowWindow(ref List<string> names, ref List<int> depths, OnTreeItemClicked onClicked, 
            OnSelectionChanged onSelectionChanged, bool useSelection, List<int> selection = null, string title = "Selection")
        {
            KSelectorWindow window = CreateInstance<KSelectorWindow>();

            window._useSelection = useSelection;
            window._onClicked = onClicked;
            window._onSelectionChanged = onSelectionChanged;
            window.titleContent = new GUIContent(title);

            (string, int)[] namesAndDepths = new (string, int)[names.Count];
            for (int i = 0; i < names.Count; i++)
            {
                namesAndDepths[i] = (names[i], depths[i]);
            }
            
            window._rigTreeWidget = new RigTreeWidget();

            if (window._useSelection)
            {
                window._rigTreeWidget.rigTreeView.onSelectionChanged = window.OnSelectionChanged;
                window._rigTreeWidget.rigTreeView.drawToggleBoxes = true;
                window._selectedItems = new List<(string, int)>();
            }
            else
            {
                window._rigTreeWidget.rigTreeView.onItemClicked = window.OnItemClicked;
            }
            
            window._rigTreeWidget.Refresh(ref namesAndDepths);

            if (window._useSelection && selection != null)
            {
                window._rigTreeWidget.rigTreeView.SetSelection(selection);
            }
            
            window.ShowAuxWindow();
        }

        private void OnItemClicked(string itemName, int index)
        {
            _onClicked.Invoke(itemName, index);
            Close();
        }

        private void OnSelectionChanged(List<(string, int)> selectedItems)
        {
            _selectedItems = selectedItems;
        }

        private void OnGUI()
        {
            if (_useSelection && GUILayout.Button("Save Selection"))
            {
                if(_selectedItems.Count > 0) _onSelectionChanged.Invoke(_selectedItems);
                Close();
            }
            
            EditorGUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
            GUI.SetNextControlName("SearchEntryField");
            _searchEntry = EditorGUILayout.TextField(_searchEntry, EditorStyles.toolbarSearchField);
            EditorGUILayout.EndHorizontal();
            
            // Automatically set focus to the text field when the window is opened
            if (Event.current.type == EventType.Repaint && GUI.GetNameOfFocusedControl() == "")
            {
                Debug.Log("Set focused to the search entry field!");
                GUI.FocusControl("SearchEntryField");
            }
            
            _rigTreeWidget.rigTreeView.Filter(_searchEntry);
            _rigTreeWidget.Render();
        }
    }
}