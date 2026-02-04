using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

namespace Supercyan.FreeSample
{
    public class SimpleSampleCharacterControl : MonoBehaviour
    {
        private enum ControlMode
        {
            /// <summary>
            /// Up moves the character forward, left and right turn the character gradually and down moves the character backwards
            /// </summary>
            Tank,
            /// <summary>
            /// Character freely moves in the chosen direction from the perspective of the camera
            /// </summary>
            Direct
        }

        [SerializeField] public SplineContainer roadSpline;
        [SerializeField] private float m_moveSpeed = 2;
        [SerializeField] private float m_turnSpeed = 20;
        [SerializeField] private float m_jumpForce = 4;
        [SerializeField] private float m_forwardSpeed = 4f;
        [SerializeField] private float m_lateralSpeed = 2f;

        [Header("Spline Movement")]
        [SerializeField] private float m_laneWidth = 2f;

        private float m_splineT = 0f;       // progress along spline
        private float m_lateralOffset = 0f; // left/right position

        [SerializeField] private Animator m_animator = null;
        [SerializeField] private Rigidbody m_rigidBody = null;

        [SerializeField] private ControlMode m_controlMode = ControlMode.Direct;

        private float m_currentV = 0;
        private float m_currentH = 0;

        private readonly float m_interpolation = 10;
        private readonly float m_walkScale = 0.33f;
        private readonly float m_backwardsWalkScale = 0.16f;
        private readonly float m_backwardRunScale = 0.66f;

        private bool m_wasGrounded;
        private Vector3 m_currentDirection = Vector3.zero;

        private float m_jumpTimeStamp = 0;
        private float m_minJumpInterval = 0.25f;
        private bool m_jumpInput = false;

        private bool m_isGrounded;

        private List<Collider> m_collisions = new List<Collider>();

        private void Awake()
        {
            if (!m_animator) m_animator = GetComponent<Animator>();
            if (!m_rigidBody) m_rigidBody = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            ContactPoint[] contactPoints = collision.contacts;
            for (int i = 0; i < contactPoints.Length; i++)
            {
                if (Vector3.Dot(contactPoints[i].normal, Vector3.up) > 0.5f)
                {
                    if (!m_collisions.Contains(collision.collider))
                    {
                        m_collisions.Add(collision.collider);
                    }
                    m_isGrounded = true;
                }
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            ContactPoint[] contactPoints = collision.contacts;
            bool validSurfaceNormal = false;
            for (int i = 0; i < contactPoints.Length; i++)
            {
                if (Vector3.Dot(contactPoints[i].normal, Vector3.up) > 0.5f)
                {
                    validSurfaceNormal = true; break;
                }
            }

            if (validSurfaceNormal)
            {
                m_isGrounded = true;
                if (!m_collisions.Contains(collision.collider))
                {
                    m_collisions.Add(collision.collider);
                }
            }
            else
            {
                if (m_collisions.Contains(collision.collider))
                {
                    m_collisions.Remove(collision.collider);
                }
                if (m_collisions.Count == 0) { m_isGrounded = false; }
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (m_collisions.Contains(collision.collider))
            {
                m_collisions.Remove(collision.collider);
            }
            if (m_collisions.Count == 0) { m_isGrounded = false; }
        }

        private void Update()
        {
            if (!m_jumpInput && Input.GetKey(KeyCode.Space))
            {
                m_jumpInput = true;
            }
        }

        private void FixedUpdate()
        {
            m_animator.SetBool("Grounded", m_isGrounded);

            switch (m_controlMode)
            {
                case ControlMode.Direct:
                    DirectUpdate();
                    break;

                case ControlMode.Tank:
                    TankUpdate();
                    break;

                default:
                    Debug.LogError("Unsupported state");
                    break;
            }

            m_wasGrounded = m_isGrounded;
            m_jumpInput = false;
        }

        private void TankUpdate()
        {
            float v = Input.GetAxis("Vertical");
            float h = Input.GetAxis("Horizontal");

            bool walk = Input.GetKey(KeyCode.LeftShift);

            if (v < 0)
            {
                if (walk) { v *= m_backwardsWalkScale; }
                else { v *= m_backwardRunScale; }
            }
            else if (walk)
            {
                v *= m_walkScale;
            }

            m_currentV = Mathf.Lerp(m_currentV, v, Time.deltaTime * m_interpolation);
            m_currentH = Mathf.Lerp(m_currentH, h, Time.deltaTime * m_interpolation);

            transform.position += transform.forward * m_currentV * m_moveSpeed * Time.deltaTime;
            transform.Rotate(0, m_currentH * m_turnSpeed * Time.deltaTime, 0);

            m_animator.SetFloat("MoveSpeed", m_currentV);

            JumpingAndLanding();
        }

        private void DirectUpdate()
        {
            // Horizontal input (left / right)
            float h = Input.GetAxis("Horizontal");
            m_currentH = Mathf.Lerp(m_currentH, h, Time.deltaTime * m_interpolation);

            // Move forward along spline
            float splineLength = roadSpline.CalculateLength();
            m_splineT += (m_forwardSpeed / splineLength) * Time.deltaTime;
            m_splineT = Mathf.Clamp01(m_splineT);

            // Evaluate spline
            roadSpline.Evaluate(
                m_splineT,
                out float3 splinePosF,
                out float3 splineTangentF,
                out float3 splineUpF
            );

            Vector3 splinePos = (Vector3)splinePosF;
            Vector3 splineTangent = (Vector3)splineTangentF;
            Vector3 splineUp = (Vector3)splineUpF;

            // Compute right direction relative to road
            Vector3 splineRight = Vector3.Cross(splineUp, splineTangent).normalized;

            // Update lateral offset
            m_lateralOffset += m_currentH * m_lateralSpeed * Time.deltaTime;
            m_lateralOffset = Mathf.Clamp(m_lateralOffset, -1f, 1f);

            // Final position
            Vector3 targetPosition =
                splinePos +
                splineRight * m_lateralOffset * m_laneWidth;

            m_rigidBody.MovePosition(targetPosition);

            // Face forward along road
            Quaternion targetRotation = Quaternion.LookRotation(splineTangent, splineUp);
            m_rigidBody.MoveRotation(targetRotation);

            // Animation
            m_animator.SetFloat("MoveSpeed", m_forwardSpeed);

            JumpingAndLanding();
        }

        private void JumpingAndLanding()
        {
            bool jumpCooldownOver = (Time.time - m_jumpTimeStamp) >= m_minJumpInterval;

            if (jumpCooldownOver && m_isGrounded && m_jumpInput)
            {
                m_jumpTimeStamp = Time.time;
                m_rigidBody.AddForce(Vector3.up * m_jumpForce, ForceMode.Impulse);
            }
        }

        public void SetForwardSpeed(float speed)
        {
            m_forwardSpeed = Mathf.Max(0f, speed); // safety: never backwards
        }
    }
}
