using UnityEngine;

namespace Particles
{
    public class Outside
    {
        private static Outside _instance = new Outside();

        private Vector3 gravity = Vector3.zero;
        private Vector3 wind = Vector3.zero;

        protected Outside() {}

        public static Outside getInstance()
        {
            return _instance;
        }

        public Vector3 Gravity
        {
            get { return gravity; }
            set { gravity = value; }
        }

        public Vector3 Wind
        {
            get { return wind; }
            set { wind = value; }
        }
    }
}