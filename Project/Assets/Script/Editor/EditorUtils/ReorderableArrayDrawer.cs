using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

   [CustomEditor(typeof(Object), true, isFallback = true)]
   [CanEditMultipleObjects]
   public class ReorderableArrayInspector : Editor
   {
       protected static string GetGrandParentPath(SerializedProperty property)
       {
           string parent = property.propertyPath;
           int firstDot = property.propertyPath.IndexOf('.');
           if (firstDot > 0)
           {
               parent = property.propertyPath.Substring(0, firstDot);
           }
           return parent;
       }

       protected static bool FORCE_INIT = false;
       [DidReloadScripts]
       private static void HandleScriptReload()
       {
           FORCE_INIT = true;

           EditorApplication.delayCall += () => { EditorApplication.delayCall += () => { FORCE_INIT = false; }; };
       }

       private static GUIStyle styleHighlight;
       /// 内部类，为每个可重排的 SerializedProperty 管理 ReorderableList
       /// 该 SerializedProperty 位于 SerializedObject 的直接子级
       protected class SortableListData
       {
           public string Parent { get; private set; }
           public Func<int, string> ElementHeaderCallback = null;

           private readonly Dictionary<string, ReorderableList> propIndex = new Dictionary<string, ReorderableList>();
           private readonly Dictionary<string, Action<SerializedProperty, Object[]>> propDropHandlers = new Dictionary<string, Action<SerializedProperty, Object[]>>();
           private readonly Dictionary<string, int> countIndex = new Dictionary<string, int>();

           public SortableListData(string parent)
           {
               Parent = parent;
           }

           public void AddProperty(SerializedProperty property)
           {
               //检查这个属性是否真的属于同一个直接子级
               if (GetGrandParentPath(property).Equals(Parent) == false)
                   return;

               ReorderableList propList = new ReorderableList(
                   property.serializedObject, property,
                   draggable: true, displayHeader: false,
                   displayAddButton: true, displayRemoveButton: true)
               {
                   headerHeight = 5
               };

               propList.drawElementCallback = delegate (Rect rect, int index, bool active, bool focused)
               {
                   SerializedProperty targetElement = property.GetArrayElementAtIndex(index);

                   bool isExpanded = targetElement.isExpanded;
                   rect.height = EditorGUI.GetPropertyHeight(targetElement, GUIContent.none, isExpanded);

                   if (targetElement.hasVisibleChildren)
                       rect.xMin += 10;

                   //交给 Unity 绘制每个元素
                   GUIContent propHeader = new GUIContent(targetElement.displayName);
                   if (ElementHeaderCallback != null)
                       propHeader.text = ElementHeaderCallback(index);
                   EditorGUI.PropertyField(rect, targetElement, propHeader, isExpanded);
               };

               //Unity 5.3 起可重排列表支持元素高度不一
#if UNITY_5_3_OR_NEWER
               propList.elementHeightCallback = index => ElementHeightCallback(property, index);

               propList.drawElementBackgroundCallback = (rect, index, active, focused) =>
               {
                   if (styleHighlight == null)
                       styleHighlight = GUI.skin.FindStyle("MeTransitionSelectHead");
                   if (focused == false)
                       return;
                   rect.height = ElementHeightCallback(property, index);
                   GUI.Box(rect, GUIContent.none, styleHighlight);
               };
#endif
               propIndex.Add(property.propertyPath, propList);
           }

           private float ElementHeightCallback(SerializedProperty property, int index)
           {
               SerializedProperty arrayElement = property.GetArrayElementAtIndex(index);
               float calculatedHeight = EditorGUI.GetPropertyHeight(arrayElement,
                                                                   GUIContent.none,
                                                                   arrayElement.isExpanded);
               calculatedHeight += 3;
               return calculatedHeight;
           }

           public bool DoLayoutProperty(SerializedProperty property)
           {
               if (propIndex.ContainsKey(property.propertyPath) == false)
                   return false;

               //绘制表头
               string headerText = string.Format("{0} [{1}]", property.displayName, property.arraySize);
               EditorGUILayout.PropertyField(property, new GUIContent(headerText), false);

               //保存表头区域，用于处理拖放
               Rect dropRect = GUILayoutUtility.GetLastRect();

               //为这个属性绘制可重排列表
               if (property.isExpanded)
               {
                   int newArraySize = EditorGUILayout.IntField("Size", property.arraySize);
                   if (newArraySize != property.arraySize)
                       property.arraySize = newArraySize;
                   propIndex[property.propertyPath].DoLayoutList();
               }

               //处理拖放到表头
               Event evt = Event.current;
               if (evt == null)
                   return true;

#if UNITY_2018_2_OR_NEWER
               if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
#else
               if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
#endif
               {
                   if (dropRect.Contains(evt.mousePosition) == false)
                       return true;

                   DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                   if (evt.type == EventType.DragPerform)
                   {
                       DragAndDrop.AcceptDrag();
                       Action<SerializedProperty, Object[]> handler = null;
                       if (propDropHandlers.TryGetValue(property.propertyPath, out handler))
                       {
                           if (handler != null)
                               handler(property, DragAndDrop.objectReferences);
                       }
                       else
                       {
                           foreach (Object dragged_object in DragAndDrop.objectReferences)
                           {
                               if (dragged_object.GetType() != property.GetType())
                                   continue;

                               int newIndex = property.arraySize;
                               property.arraySize++;

                               SerializedProperty target = property.GetArrayElementAtIndex(newIndex);
                               target.objectReferenceInstanceIDValue = dragged_object.GetInstanceID();
                           }
                       }
                       evt.Use();
                   }
               }
               return true;
           }

           public int GetElementCount(SerializedProperty property)
           {
               if (property.arraySize <= 0)
                   return 0;

               int count;
               if (countIndex.TryGetValue(property.propertyPath, out count))
                   return count;

               var element = property.GetArrayElementAtIndex(0);
               var countElement = element.Copy();
               int childCount = 0;
               if (countElement.NextVisible(true))
               {
                   int depth = countElement.Copy().depth;
                   do
                   {
                       if (countElement.depth != depth)
                           break;
                       childCount++;
                   } while (countElement.NextVisible(false));
               }

               countIndex.Add(property.propertyPath, childCount);
               return childCount;
           }

           public ReorderableList GetPropertyList(SerializedProperty property)
           {
               if (propIndex.ContainsKey(property.propertyPath))
                   return propIndex[property.propertyPath];
               return null;
           }

           public void SetDropHandler(SerializedProperty property, Action<SerializedProperty, Object[]> handler)
           {
               string path = property.propertyPath;
               if (propDropHandlers.ContainsKey(path))
                   propDropHandlers[path] = handler;
               else
                   propDropHandlers.Add(path, handler);
           }
       }
       public bool isSubEditor;
       private readonly List<SortableListData> listIndex = new List<SortableListData>();
       private readonly Dictionary<string, Editor> editableIndex = new Dictionary<string, Editor>();
       protected bool alwaysDrawInspector = false;
       protected bool isInitialized = false;
       protected bool hasSortableArrays = false;
       protected bool hasEditable = false;
       ~ReorderableArrayInspector()
       {
           listIndex.Clear();
           editableIndex.Clear();
           isInitialized = false;
       }
       #region Initialization
       private void OnEnable()
       {
           InitInspector();
       }
       protected virtual void InitInspector(bool force)
       {
           if (force)
               isInitialized = false;
           InitInspector();
       }
       protected virtual void InitInspector()
       {
           if (isInitialized && FORCE_INIT == false)
               return;
           FindTargetProperties();
       }
       protected void FindTargetProperties()
       {
           listIndex.Clear();
           editableIndex.Clear();
           Type typeScriptable = typeof(ScriptableObject);
           SerializedProperty iterProp = serializedObject.GetIterator();
           //这个迭代器遍历所有子级序列化属性，寻找
           //带有 SortableArray 特性的属性
           if (iterProp.NextVisible(true))
           {
               do
               {
                   if (iterProp.isArray && iterProp.propertyType != SerializedPropertyType.String)
                   {
#if UNITY_2020_2_OR_NEWER
                       bool canTurnToList = false;
#else
                       bool canTurnToList = iterProp.HasAttribute<ReorderableAttribute>();
#endif
                       if (canTurnToList)
                       {
                           hasSortableArrays = true;
                           CreateListData(serializedObject.FindProperty(iterProp.propertyPath));
                       }
                   }
                   if (iterProp.propertyType == SerializedPropertyType.ObjectReference)
                   {
                       Type propType = iterProp.GetTypeReflection();
                       if (propType == null)
                           continue;
                       bool isScriptable = propType.IsSubclassOf(typeScriptable);
                       if (isScriptable)
                       {
                           bool makeEditable = true;
                           if (makeEditable)
                           {
                               Editor scriptableEditor = null;
                               if (iterProp.objectReferenceValue != null)
                               {
#if UNITY_5_6_OR_NEWER
                                   CreateCachedEditorWithContext(iterProp.objectReferenceValue,
                                                               serializedObject.targetObject, null,
                                                               ref scriptableEditor);
#else
								CreateCachedEditor(iterProp.objectReferenceValue, null, ref scriptableEditor);
#endif
                                   var reorderable = scriptableEditor as ReorderableArrayInspector;
                                   if (reorderable != null)
                                       reorderable.isSubEditor = true;
                               }
                               editableIndex.Add(iterProp.propertyPath, scriptableEditor);
                               hasEditable = true;
                           }
                       }
                   }
               } while (iterProp.NextVisible(true));
           }
           isInitialized = true;
           if (hasSortableArrays == false)
           {
               listIndex.Clear();
           }
       }
       private void CreateListData(SerializedProperty property)
       {
           string parent = GetGrandParentPath(property);
           //尝试在 SortableListData 中查找祖父级
           SortableListData data = listIndex.Find(listData => listData.Parent.Equals(parent));
           if (data == null)
           {
               data = new SortableListData(parent);
               listIndex.Add(data);
           }
           data.AddProperty(property);
           object[] attr = property.GetAttributes<ReorderableAttribute>();
           if (attr != null && attr.Length == 1)
           {
               ReorderableAttribute arrayAttr = (ReorderableAttribute)attr[0];
               if (arrayAttr != null)
               {
                   HandleReorderableOptions(arrayAttr, property, data);
               }
           }
       }
       private void HandleReorderableOptions(ReorderableAttribute arrayAttr, SerializedProperty property, SortableListData data)
       {
           //自定义元素表头
           if (string.IsNullOrEmpty(arrayAttr.ElementHeader) == false)
           {
               data.ElementHeaderCallback = i => string.Format("{0} {1}", arrayAttr.ElementHeader, (arrayAttr.HeaderZeroIndex ? i : i + 1));
           }
           //按单行绘制属性
           if (arrayAttr.ElementSingleLine)
           {
               var list = data.GetPropertyList(property);
#if UNITY_5_3_OR_NEWER
               list.elementHeightCallback = index => EditorGUIUtility.singleLineHeight + 6;
               list.drawElementBackgroundCallback = (rect, index, active, focused) =>
               {
                   if (focused == false)
                       return;
                   if (styleHighlight == null)
                       styleHighlight = GUI.skin.FindStyle("MeTransitionSelectHead");
                   GUI.Box(rect, GUIContent.none, styleHighlight);
               };
#endif
               list.drawElementCallback = (rect, index, active, focused) =>
               {
                   var element = property.GetArrayElementAtIndex(index);
                   element.isExpanded = false;
                   int childCount = data.GetElementCount(property);
                   if (childCount < 1)
                       return;
                   rect.y += 3;
                   rect.height -= 6;
                   if (element.NextVisible(true))
                   {
                       float restoreWidth = EditorGUIUtility.labelWidth;
                       EditorGUIUtility.labelWidth /= childCount;
                       float padding = 5f;
                       float width = rect.width - padding * (childCount - 1);
                       width /= childCount;
                       Rect childRect = new Rect(rect) { width = width };
                       int depth = element.Copy().depth;
                       do
                       {
                           if (element.depth != depth)
                               break;
                           if (childCount <= 2)
                               EditorGUI.PropertyField(childRect, element, false);
                           else
                               EditorGUI.PropertyField(childRect, element, GUIContent.none, false);
                           childRect.x += width + padding;
                       } while (element.NextVisible(false));
                       EditorGUIUtility.labelWidth = restoreWidth;
                   }
               };
           }
       }
       /// 给定一个 SerializedProperty，返回分配给它的自动 ReorderableList（如果有）
       protected ReorderableList GetSortableList(SerializedProperty property)
       {
           if (listIndex.Count == 0)
               return null;
           string parent = GetGrandParentPath(property);
           SortableListData data = listIndex.Find(listData => listData.Parent.Equals(parent));
           if (data == null)
               return null;
           return data.GetPropertyList(property);
       }
       /// 在 SerializedObject 的 ReorderableList 上设置拖放处理函数（如果有）
       protected bool SetDragDropHandler(SerializedProperty property, Action<SerializedProperty, Object[]> handler)
       {
           if (listIndex.Count == 0)
               return false;
           string parent = GetGrandParentPath(property);
           SortableListData data = listIndex.Find(listData => listData.Parent.Equals(parent));
           if (data == null)
               return false;
           data.SetDropHandler(property, handler);
           return true;
       }
       #endregion
       protected bool InspectorGUIStart(bool force = false)
       {
           //未初始化，尝试初始化
           if (hasSortableArrays && listIndex.Count == 0)
               InitInspector();
           if (hasEditable && editableIndex.Count == 0)
               InitInspector();
           //没有可排序数组或列表索引未初始化
           bool cannotDrawOrderable = (hasSortableArrays == false || listIndex.Count == 0);
           bool cannotDrawEditable = (hasEditable == false || editableIndex.Count == 0);
           if (cannotDrawOrderable && cannotDrawEditable && force == false)
           {
               if (isSubEditor)
                   DrawPropertiesExcluding(serializedObject, "m_Script");
               else
                   base.OnInspectorGUI();
               return false;
           }
           serializedObject.Update();
           return true;
       }
       protected virtual void DrawInspector()
       {
           DrawPropertiesAll();
       }
       public override void OnInspectorGUI()
       {
           if (InspectorGUIStart(alwaysDrawInspector) == false)
               return;
           EditorGUI.BeginChangeCheck();
           DrawInspector();
           if (EditorGUI.EndChangeCheck())
           {
               serializedObject.ApplyModifiedProperties();
               InitInspector(true);
           }
       }
       protected enum IterControl
       {
           Draw,
           Continue,
           Break
       }
       protected void IterateDrawProperty(SerializedProperty property, Func<IterControl> filter = null)
       {
           if (property.NextVisible(true))
           {
               //记住迭代开始时的深度
               int depth = property.Copy().depth;
               do
               {
                   //如果比迭代深度更深，就退出
                   if (property.depth != depth)
                       break;
                   if (isSubEditor && property.name.Equals("m_Script"))
                       continue;
                   if (filter != null)
                   {
                       var filterResult = filter();
                       if (filterResult == IterControl.Break)
                           break;
                       if (filterResult == IterControl.Continue)
                           continue;
                   }
                   DrawPropertySortableArray(property);
               } while (property.NextVisible(false));
           }
       }
       /// 如果初始化时找到了，就把 SerializedProperty 绘制成 ReorderableList
       /// 否则用 EditorGUILayout.PropertyField
       protected void DrawPropertySortableArray(SerializedProperty property)
       {
           //尝试获取该属性所属的可排序列表
           SortableListData listData = null;
           if (listIndex.Count > 0)
               listData = listIndex.Find(data => property.propertyPath.StartsWith(data.Parent));
           //有 ReorderableList
           if (listData != null)
           {
               //尝试显示该列表
               if (listData.DoLayoutProperty(property) == false)
               {
                   EditorGUILayout.PropertyField(property, false);
                   if (property.isExpanded)
                   {
                       EditorGUI.indentLevel++;
                       SerializedProperty targetProp = serializedObject.FindProperty(property.propertyPath);
                       IterateDrawProperty(targetProp);
                       EditorGUI.indentLevel--;
                   }
               }
           }
           else
           {
               SerializedProperty targetProp = serializedObject.FindProperty(property.propertyPath);
               bool isStartProp = targetProp.propertyPath.StartsWith("m_");
               using (new EditorGUI.DisabledScope(isStartProp))
               {
#if !UNITY_2020_2_OR_NEWER
                   EditorGUILayout.PropertyField(targetProp, targetProp.isExpanded);
#else
                   EditorGUILayout.PropertyField(targetProp, true);
#endif
               }
           }
       }
       #region Helper functions
       /// 绘制默认 inspector，并带上可排序数组
       public void DrawPropertiesAll()
       {
           SerializedProperty iterProp = serializedObject.GetIterator();
           IterateDrawProperty(iterProp);
       }
       /// 绘制默认 inspector，但跳过给定的属性名
       public void DrawPropertiesExcept(params string[] propertyNames)
       {
           SerializedProperty iterProp = serializedObject.GetIterator();
           IterateDrawProperty(iterProp,
               filter: () =>
               {
                   if (propertyNames.Contains(iterProp.name))
                       return IterControl.Continue;
                   return IterControl.Draw;
               });
       }
       /// 绘制默认 inspector，从给定属性开始
       /// <param name="propertyStart">起始属性名</param>
       public void DrawPropertiesFrom(string propertyStart)
       {
           bool canDraw = false;
           SerializedProperty iterProp = serializedObject.GetIterator();
           IterateDrawProperty(iterProp,
               filter: () =>
               {
                   if (iterProp.name.Equals(propertyStart))
                       canDraw = true;
                   if (canDraw)
                       return IterControl.Draw;
                   return IterControl.Continue;
               });
       }
       /// 绘制默认 inspector，到给定属性为止
       /// <param name="propertyStop">结束属性名</param>
       public void DrawPropertiesUpTo(string propertyStop)
       {
           SerializedProperty iterProp = serializedObject.GetIterator();
           IterateDrawProperty(iterProp,
               filter: () =>
               {
                   if (iterProp.name.Equals(propertyStop))
                       return IterControl.Break;
                   return IterControl.Draw;
               });
       }
       /// 绘制默认 inspector，从起始属性到结束属性
       /// <param name="propertyStart">起始属性名</param>
       /// <param name="propertyStop">结束属性名</param>
       public void DrawPropertiesFromUpTo(string propertyStart, string propertyStop)
       {
           bool canDraw = false;
           SerializedProperty iterProp = serializedObject.GetIterator();
           IterateDrawProperty(iterProp,
               filter: () =>
               {
                   if (iterProp.name.Equals(propertyStop))
                       return IterControl.Break;
                   if (iterProp.name.Equals(propertyStart))
                       canDraw = true;
                   if (canDraw == false)
                       return IterControl.Continue;
                   return IterControl.Draw;
               });
       }
       #endregion
   }
