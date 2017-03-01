using UnityEngine;

namespace Clues.Base.BallDrop
{
    public class BallDrop : Clue
    {
        public GameObject FailObject;
        public GameObject ObjectToTrigger;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        public GameObject SuccessObject;

        // Use this for initialization
        private void Start()
        {
            SetProperty("enabled", true);
            // Grab the original local position of the sphere when the app starts.
            originalPosition = transform.localPosition;
            originalRotation = transform.localRotation;
            AddAction("OnSuccess", ObjectToTrigger, "Trigger");

            Initialise();
        }

        private void OnDrop()
        {
            // If the sphere has no Rigidbody component, add one to enable physics.
            if (GetProperty<bool>("enabled") && !GetComponent<Rigidbody>())
            {
                var rigidbody = gameObject.AddComponent<Rigidbody>();
                rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }
        }

        private void OnLeft()
        {
            if (GetProperty<bool>("enabled") && transform.position.x > -0.45)
                transform.Translate(-0.05f, 0, 0);
        }

        private void OnRight()
        {
            if (GetProperty<bool>("enabled") && transform.position.x < 0.45)
                transform.Translate(0.05f, 0, 0);
        }

        private void OnActivate()
        {
            SetProperty("active", true);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
                OnRight();
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                OnLeft();
            else if (Input.GetKeyDown(KeyCode.DownArrow))
                OnDrop();
        }

        private void OnReset()
        {
            // If the sphere has a Rigidbody component, remove it to disable physics.
            var rigidbody = GetComponent<Rigidbody>();
            if (rigidbody != null)
                Destroy(rigidbody);

            // Put the sphere back into its original local position.
            transform.localPosition = originalPosition;
            transform.localRotation = originalRotation;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.Equals(FailObject))
            {
                OnReset();
            }
            else if (collision.gameObject.Equals(SuccessObject))
            {
                Trigger("OnSuccess");
                OnReset();
            }
        }
    }
}