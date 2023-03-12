using UnityEngine;
using System.Collections;

namespace UI
{
    public class WorldCanvas : MonoBehaviour
    {
        public static WorldCanvas singleton;

        public Transform coverText;
        public Transform vaultText;

        void Awake()
        {
            singleton = this;
        }
    }
}
