using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CHJ.SimpleAniEventTool
{
    public class SimpleAniEvent : MonoBehaviour
    {
        public virtual void ActiveChildObj(string objName) 
        {
            Transform child = transform.Find(objName);
            if (child != null)
            {
                child.gameObject.SetActive(true);
            }
        }

        public virtual void DeactiveChildObj(string objName)
        {
            Transform child = transform.Find(objName);
            if (child != null)
            {
                child.gameObject.SetActive(false);
            }
        }
    }
}
