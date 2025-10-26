using UnityEngine;

namespace Infra
{
    public class AutoRotation : MonoBehaviour
    {
        public enum RotationAxis
        {
            X,
            Y,
            Z,
            Custom
        }

        [Header("Rotation Settings")] public RotationAxis axis = RotationAxis.Z;
        public float rotationSpeed = 50f;

        [Tooltip("If Custom is selected, use this vector for rotation.")]
        public Vector3 customAxis = Vector3.zero;

        [Header("Use Unscaled Time (for UI or paused states)")]
        public bool useUnscaledTime;

        public bool stop;

        private void Awake()
        {
            stop = true;
        }

        private void Update()
        {
            if (stop)
            {
                return;
            }

            float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            Vector3 rotationVector = Vector3.zero;

            switch (axis)
            {
                case RotationAxis.X:
                    rotationVector = Vector3.right * rotationSpeed;
                    break;
                case RotationAxis.Y:
                    rotationVector = Vector3.up * rotationSpeed;
                    break;
                case RotationAxis.Z:
                    rotationVector = Vector3.forward * rotationSpeed;
                    break;
                case RotationAxis.Custom:
                    rotationVector = customAxis.normalized * rotationSpeed;
                    break;
            }

            transform.Rotate(rotationVector * deltaTime, Space.Self);
        }
    }
}