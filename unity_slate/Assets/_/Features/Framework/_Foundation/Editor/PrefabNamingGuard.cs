using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

namespace Foundation.Editor
{
    
    [InitializeOnLoad]
    public static  class PrefabNamingGuardDragFeedback
    {

        #region Publics
        
            public static List<GameObject> m_draggedObjects = new List<GameObject>();
            public static bool m_isProcessing = false;

        #endregion

        
        #region Unity API

            static PrefabNamingGuardDragFeedback()
            {
                EditorApplication.projectWindowItemOnGUI += OnProjectGUI;
            }

            private static void OnProjectGUI(string guid, Rect rect)
            {

                if (m_isProcessing) return;
                
                //Debug.Log(Print.Green($"// Dragged Count : {m_draggedObjects.Count}"));
                
                var dragged = DragAndDrop.objectReferences;

                foreach (var obj in dragged)
                {
                    if (obj is GameObject go)
                    {
                        if (!m_draggedObjects.Contains(go) && !go.name.StartsWith("P_"))
                        {
                            //Debug.Log(Print.Red($"Added object {go}"));
                            m_draggedObjects.Add(go);
                        }
                    }
                }
            }
            
        #endregion

        
        
        #region Main Methods

            public static List<GameObject> GetDraggedGameObject()
            {
                return m_draggedObjects;
            }
            
        
            public static void RemoveObject(GameObject go)
            {
                if(m_draggedObjects.Contains(go)) {m_draggedObjects.Remove(go);}
            }
            
        #endregion
        
    }


    public class PrefabNamingAssetProcessor : AssetModificationProcessor
    {
        
        #region Unity API
            public static void OnWillCreateAsset(string path)
            {
                if (!path.EndsWith(".prefab.meta"))
                    return;
                
                //Debug.Log(Print.Cyan($"Current path {path}"));
                string prefabPath = path.Replace(".meta", "");
                string fileName = System.IO.Path.GetFileName(prefabPath);
                string goName = System.IO.Path.GetFileNameWithoutExtension(prefabPath);
                
                //Debug.Log(Print.Green($"Filename ? :  {fileName} Prefab Path : {prefabPath} Local dragables {_dragables.Count}"));
                //if(_dragables.Count >0 )Debug.Log(Print.Green($" Dragable at 0 {_dragables[0]}"));
                
                
                _dragables = PrefabNamingGuardDragFeedback.GetDraggedGameObject();
                var go = _dragables.Find(gameO => gameO.name == goName);
                PrefabNamingGuardDragFeedback.m_isProcessing = true;
                
                
                
                switch (_dragables.Count)
                {
                    case 0:
                        
                        break;
                    
                    case 1:
                        if (_isSomeoneYouDontLike)
                        {
                            ShowRenamePrefabModal(fileName, prefabPath, go);
                        }
                        else
                        {
                          
                            ShowAutoRenamePrefabModal(prefabPath, go);
                        }

                        break;
                    
                    case >1:
                        HandleMultiple(prefabPath,go);
                        break;
                    
                }
            }
            
        #endregion
        
        
        #region Utils
        
            
            private static void HandleMovingPrefab(string newPrefabPath)
            {
                if (IsPrefabInValidFolder(newPrefabPath)) return;
                
                if (!Directory.Exists(ValidPrefabRoot))
                {
                    Debug.LogError($"Prefab path {ValidPrefabRoot} doesn't exist, remove commentary line here if needed.");
                    //Directory.CreateDirectory(ValidPrefabRoot);
                }
                
                ShowMovingPrefabModal(newPrefabPath);
                
            }
            
            private static void ShowMovingPrefabModal(string newPrefabPath)
            {
                string fileNameOnly = System.IO.Path.GetFileName(newPrefabPath);
                string validPath = $"{ValidPrefabRoot}/{fileNameOnly}";
                PrefabMovingPopup.Show(validPath, (newPath) =>
                {
                    MovePrefab(newPrefabPath, newPath);
                });
            }
            
            private static void MovePrefab(string prefabPath, string newPath)
            {
                if(_fileCreated == false) return;
               
                var result = AssetDatabase.MoveAsset(prefabPath, newPath);
                if (!string.IsNullOrEmpty(result))
                    Debug.LogError("Erreur lors du déplacement : " + result + $"Path {newPath}  ");
                else
                    Debug.Log($"Prefab déplacé automatiquement vers {newPath}");
                _fileCreated = false;
            
            }
            
            
            
            /// <summary>
            /// For each Gameobjects that user tries to drag into project it will display a modal saying that multi-drag is not allowed for prefab making.
            /// And deletes every .prefab that tried to be dragged in the project.
            /// </summary>
            /// <param name="prefabPath">Path to .prefab</param>
            /// <param name="go">The game object in the scene the user tries to drag to make prefab.</param>
            private static void HandleMultiple(string prefabPath, GameObject go)
            {
                EditorUtility.DisplayDialog(
                    "Can't create multiple prefabs at once.",
                    $"They will be removed, add them one by one.",
                    "OK"
                );

                AssetDatabase.DeleteAsset(prefabPath);
                AssetDatabase.Refresh();
                //Debug.Log(Print.Yellow($"Removed {go.name} prefab Path After: {prefabPath}"));
                Remove(go);
            }


            
            private static void ShowAutoRenamePrefabModal(string prefabPath, GameObject go)
            {
                PrefabAutoRenamePopup.Show(
                    () =>
                    {
                        string name = System.IO.Path.GetFileNameWithoutExtension(prefabPath);
                        string newName = "P_" + name;
                        string dir = System.IO.Path.GetDirectoryName(prefabPath);
                        string newPath = System.IO.Path.Combine(dir, newName + ".prefab");
                        var result = AssetDatabase.MoveAsset(prefabPath, newPath);
                        AssetDatabase.Refresh();
                        
                        if (string.IsNullOrEmpty(result) && go != null)
                        {
                            Undo.RecordObject(go, "Rename scene GameObject");
                            go.name = newName;
                            EditorUtility.SetDirty(go);
                            _fileCreated = true;
                            Debug.Log($"Game Object Dirty : {go}");
                            PrefabNamingGuardDragFeedback.m_draggedObjects.Remove(go);
                            PrefabNamingGuardDragFeedback.m_isProcessing = false;
                            HandleMovingPrefab(newPath);

                        }
                        else
                        {
                            Debug.LogError("[AutoRename ERROR] " + result);
                            AssetDatabase.DeleteAsset(prefabPath);
                            AssetDatabase.Refresh();
                            Remove(go);
                            PrefabNamingGuardDragFeedback.m_isProcessing = false;
                        }

                    },
                    () =>
                    {
                        AssetDatabase.DeleteAsset(prefabPath);
                        AssetDatabase.Refresh();
                        Remove(go);
                        PrefabNamingGuardDragFeedback.m_isProcessing = false;
                    }
                    
                );
                
            }

            
            /// <summary>
            /// Opens a sort of Modal Window asking the user to either rename the gameobject to fit intern convention or to cancel.
            /// </summary>
            /// <param name="fileName">File that ends with .prefab</param>
            /// <param name="prefabPath">Path to .prefab</param>
            /// <param name="go">The game object in the scene the user tries to drag to make prefab.</param>
            private static void ShowRenamePrefabModal(string fileName, string prefabPath, GameObject go)
            {
                var name = fileName.Replace(".prefab", "");
                PrefabRenamePopup.Show(
                    name,
                    newName =>
                    {
                        if (!newName.StartsWith("P_"))
                        {
                            AssetDatabase.DeleteAsset(prefabPath);
                            AssetDatabase.Refresh();
                            Remove(go);
                            PrefabNamingGuardDragFeedback.m_isProcessing = false;
                            return;
                        }
                        string dir = System.IO.Path.GetDirectoryName(prefabPath);
                        string newFileName = newName + ".prefab";
                        string newPath = System.IO.Path.Combine(dir, newFileName);
                        var result = AssetDatabase.MoveAsset(prefabPath, newPath);
                
                        if (string.IsNullOrEmpty(result) && go != null)
                        {
                            Undo.RecordObject(go, "Rename scene GameObject");
                            go.name = newName;
                            EditorUtility.SetDirty(go);
                            HandleMovingPrefab(newPath);
                        }
                        AssetDatabase.Refresh();
                        PrefabNamingGuardDragFeedback.m_draggedObjects.Remove(go);
                        PrefabNamingGuardDragFeedback.m_isProcessing = false;
                        _fileCreated = true;

                    },
                    () =>
                    {
                        AssetDatabase.DeleteAsset(prefabPath);
                        AssetDatabase.Refresh();
                        Remove(go);
                        PrefabNamingGuardDragFeedback.m_isProcessing = false;
                        
                    }
                );
            }
        
            private static void Remove(GameObject go)
            {
                PrefabNamingGuardDragFeedback.RemoveObject(go);
                if (go != null && PrefabUtility.IsPartOfPrefabInstance(go))
                {
                    PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                    EditorUtility.SetDirty(go);
                }
            }
            
            private static bool IsPrefabInValidFolder(string path)
            {
                return path.StartsWith(ValidPrefabRoot, System.StringComparison.OrdinalIgnoreCase);
            }
            
        #endregion
        
        
        #region Privates

            private const string ValidPrefabRoot = "Assets/_/Database/Prefabs";
            private static bool _isSomeoneYouDontLike = false;
            private static bool _fileCreated = false;
            private static List<GameObject> _dragables = new List<GameObject>();

        #endregion
    }
}
