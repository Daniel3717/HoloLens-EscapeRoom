using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Clues
{
    public class BallDrop : Clue
    {
        Vector3 originalPosition;
        Quaternion originalRotation;
        public GameObject FailObject;
        public GameObject SuccessObject;
        public GameObject ObjectToTrigger;

        // Use this for initialization
        void Start()
        {
            SetProperty("enabled", true);
            // Grab the original local position of the sphere when the app starts.
            originalPosition = this.transform.localPosition;
            originalRotation = this.transform.localRotation;
            AddAction("OnSuccess", ObjectToTrigger, "Trigger");
        }

        void OnDrop()
        {
            // If the sphere has no Rigidbody component, add one to enable physics.
            if (GetProperty<bool>("enabled") && !this.GetComponent<Rigidbody>())
            {
                var rigidbody = this.gameObject.AddComponent<Rigidbody>();
                rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }
        }

        void OnLeft()
        {
            if (GetProperty<bool>("enabled") && transform.position.x > -0.45)
            {
                transform.Translate(-0.05f, 0, 0);
            }
        }

        void OnRight()
        {
            if (GetProperty<bool>("enabled") && transform.position.x < 0.45)
            {
                transform.Translate(0.05f, 0, 0);
            }
        }

        void OnActivate()
        {
            SetProperty("active", true);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                OnRight();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                OnLeft();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                OnDrop();
            }
        }

        void OnReset()
        {
            // If the sphere has a Rigidbody component, remove it to disable physics.
            var rigidbody = this.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                Destroy(rigidbody);
            }

            // Put the sphere back into its original local position.
            this.transform.localPosition = originalPosition;
            this.transform.localRotation = originalRotation;
        }

        void OnCollisionEnter(Collision collision)
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