using UnityEngine;

namespace Clues.Base.BallDrop
{
    public class BallDrop : Clue
    {
        public bool Enabled = true;
        public GameObject FailObject;
        public GameObject ObjectToTrigger;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        public GameObject SuccessObject;
        public GameObject MagicSpeedObject;
        private bool _falling = false;
        private bool _magicCollide = false;

        // Use this for initialization
        private void Start()
        {
            if (!IsPropertySet("enabled"))
                SetProperty("enabled", Enabled);
            // Grab the original local position of the sphere when the app starts.
            originalPosition = transform.localPosition;
            originalRotation = transform.localRotation;
            if (ObjectToTrigger != null)
                AddAction("OnSuccess", ObjectToTrigger, "Trigger");

            Initialise();
        }

        private void OnDrop()
        {
            // If the sphere has no Rigidbody component, add one to enable physics.
            if (GetProperty<bool>("enabled") && !GetComponent<Rigidbody>() && !_falling)
            {
                var rigidbody = gameObject.AddComponent<Rigidbody>();
                rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }
            _falling = true;
            _magicCollide = false;
        }

        private void OnLeft()
        {
            if (GetProperty<bool>("enabled") && transform.localPosition.x > -0.45 && !_falling)
                transform.Translate(-0.05f, 0, 0);
        }

        private void OnRight()
        {
            if (GetProperty<bool>("enabled") && transform.localPosition.x < 0.45 && !_falling)
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
            else if (Input.GetKeyDown(KeyCode.Space))
                OnReset(true);

            var rigidbody = GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.velocity += new Vector3(0, Time.deltaTime * 8f, 0);
            }
    }

        private void OnReset(bool resetPosition)
        {
           
            // If the sphere has a Rigidbody component, remove it to disable physics.
            var rigidbody = GetComponent<Rigidbody>();
            if (rigidbody != null)
                Destroy(rigidbody);

            if (resetPosition)
            {
                // Put the sphere back into its original local position.
                transform.localPosition = originalPosition;
                transform.localRotation = originalRotation;
                _falling = false;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.Equals(FailObject))
            {
                Trigger("OnFail");
                OnReset(true);
            }
            else if (collision.gameObject.Equals(SuccessObject))
            {
                Trigger("OnSuccess");
                OnReset(false);
            }
            else if (collision.gameObject.Equals(MagicSpeedObject))
            {
                var rigidbody = GetComponent<Rigidbody>();
                if (rigidbody != null && !_magicCollide)
                {
                    rigidbody.velocity += new Vector3(-0.1f, -0.2f, 0);
                    _magicCollide = true;
                }
            }
        }
    }
}
