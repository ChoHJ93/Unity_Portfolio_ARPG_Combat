using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CHJ
{
    public static class CustomExtensions
    {


        #region List & Dictionary
        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }
        public static bool IsNullOrEmpty<T>(this List<T> list)
        {
            return list == null || list.Count == 0;
        }
        #endregion

        #region Transform
        public static Transform FindChildAtDepthWithName(this Transform parent, int depth, string name)
        {
            if (depth < 0)
            {
                return null;
            }

            if (depth == 0)
            {
                foreach (Transform child in parent)
                {
                    if (child.name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        return child;
                    }
                }
                return null;
            }

            foreach (Transform child in parent)
            {
                Transform result = child.FindChildAtDepthWithName(depth - 1, name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
        public static Transform[] GetChildrenAtDepth(this Transform parent, int depth)
        {
            List<Transform> result = new List<Transform>();
            AddChildrenAtDepth(parent, depth, 0, result);
            return result.ToArray();
        }
        public static List<Transform> GetAllChildrenByDepth(this Transform parent)
        {
            List<Transform> result = new List<Transform>();
            Queue<Transform> queue = new Queue<Transform>();

            foreach (Transform child in parent)
            {
                queue.Enqueue(child);
            }

            while (queue.Count > 0)
            {
                Transform current = queue.Dequeue();
                result.Add(current);

                foreach (Transform child in current)
                {
                    queue.Enqueue(child);
                }
            }

            return result;
        }
        private static void AddChildrenAtDepth(Transform current, int targetDepth, int currentDepth, List<Transform> result)
        {
            if (currentDepth == targetDepth)
            {
                result.Add(current);
                return; // 타깃 깊이에 도달했으므로 하위로 더 탐색하지 않음
            }

            foreach (Transform child in current)
            {
                AddChildrenAtDepth(child, targetDepth, currentDepth + 1, result);
            }
        }


        public static bool TryGetComponentInChildren<T>(this Transform tr, bool includeInactive, string childName, out T component) where T : UnityEngine.Object
        {
            component = default;
            if (tr.childCount == 0) return false;

            for (int i = 0; i < tr.childCount; i++)
            {
                if (includeInactive == false && tr.GetChild(i).gameObject.activeInHierarchy == false)
                    continue;

                if (tr.GetChild(i).name.Equals(childName) == false)
                    continue;

                if (tr.GetChild(i).TryGetComponent(out component))
                {
                    return true;
                }
            }

            return false;
        }
        public static T GetComponentAtDepth<T>(this GameObject go, int targetDepth) where T : Component
        {
            return GetComponentAtDepth<T>(go.transform, targetDepth, 0);
        }
        private static T GetComponentAtDepth<T>(Transform trans, int targetDepth, int currentDepth) where T : Component
        {
            if (targetDepth == currentDepth)
            {
                return trans.GetComponent<T>();
            }

            if (targetDepth < currentDepth)
            {
                return null;
            }

            foreach (Transform child in trans)
            {
                T component = GetComponentAtDepth<T>(child, targetDepth, currentDepth + 1);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }
        public static List<T> GetComponentsAtDepth<T>(this GameObject go, int targetDepth) where T : Component
        {
            List<T> result = new List<T>();
            GetComponentsAtDepth(go.transform, targetDepth, 0, result);
            return result;
        }
        private static void GetComponentsAtDepth<T>(Transform trans, int targetDepth, int currentDepth, List<T> result) where T : Component
        {
            if (targetDepth == currentDepth)
            {
                T component = trans.GetComponent<T>();
                if (component != null)
                {
                    result.Add(component);
                }
                return;
            }

            if (targetDepth < currentDepth)
            {
                return;
            }

            foreach (Transform child in trans)
            {
                GetComponentsAtDepth(child, targetDepth, currentDepth + 1, result);
            }
        }

        #endregion

        #region Component
        public static T GetComponent<T>(this GameObject gameObject, bool addIfNotFound) where T : Component
        {
            T component = gameObject.GetComponent<T>();

            if (addIfNotFound && component == null)
            {
                component = gameObject.AddComponent<T>();
            }

            return component;
        }
        #endregion

    }

}