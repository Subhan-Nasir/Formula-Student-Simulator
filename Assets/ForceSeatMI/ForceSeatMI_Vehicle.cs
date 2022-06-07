/*
* Copyright (C) 2012-2022 Motion Systems
*
* This file is part of ForceSeat motion system.
*
* www.motionsystems.eu
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

using UnityEngine;

namespace MotionSystems
{
	class ForceSeatMI_Vehicle : ForceSeatMI_ITelemetryInterface
	{
		const float FSMI_VT_ACC_LOW_PASS_FACTOR = 0.5f;
		const float FSMI_VT_ANGLES_SPEED_LOW_PASS_FACTOR = 0.5f;

		private bool m_firstCall         = true;
		private float m_prevForwardSpeed = 0;
		private float m_prevRightSpeed   = 0;
		private float m_prevUpSpeed      = 0;
		private Vector3 m_prevAngles     = new Vector3();
		private Rigidbody m_rb           = null;
		private sbyte m_gearNumber       = 0;

		public ForceSeatMI_Vehicle(Rigidbody rb)
		{
			m_rb = rb;
		}

		public virtual void Begin()
		{
			m_firstCall = true;
		}

		public virtual void End()
		{
			m_firstCall = true;
		}

		public virtual void Update(float deltaTime, ref FSMI_TelemetryACE telemetry)
		{
			var velocity      = m_rb.transform.InverseTransformDirection(m_rb.velocity);

			var forwardSpeed = velocity.z;
			var rightSpeed   = velocity.x;
			var upSpeed      = velocity.y;

			telemetry.rpm                    = 0;
			telemetry.maxRpm                 = 0;
			telemetry.vehicleForwardSpeed    = velocity.magnitude; // m/s

			if (m_firstCall)
			{
				m_firstCall = false;

				telemetry.bodyLinearAcceleration[0].forward = 0;
				telemetry.bodyLinearAcceleration[0].right   = 0;
				telemetry.bodyLinearAcceleration[0].upward  = 0;
				telemetry.bodyAngularVelocity[0].roll       = 0;
				telemetry.bodyAngularVelocity[0].pitch      = 0;
				telemetry.bodyAngularVelocity[0].yaw        = 0;
			}
			else
			{
				ForceSeatMI_Utils.LowPassFilter(ref telemetry.bodyLinearAcceleration[0].forward, (forwardSpeed - m_prevForwardSpeed) / deltaTime, FSMI_VT_ACC_LOW_PASS_FACTOR);
				ForceSeatMI_Utils.LowPassFilter(ref telemetry.bodyLinearAcceleration[0].right,   (rightSpeed   - m_prevRightSpeed)   / deltaTime, FSMI_VT_ACC_LOW_PASS_FACTOR);
				ForceSeatMI_Utils.LowPassFilter(ref telemetry.bodyLinearAcceleration[0].upward,  (upSpeed      - m_prevUpSpeed)      / deltaTime, FSMI_VT_ACC_LOW_PASS_FACTOR);

				var deltaAngles = new Vector3(
					Mathf.Deg2Rad * Mathf.DeltaAngle(m_rb.transform.eulerAngles.x, m_prevAngles.x),
					Mathf.Deg2Rad * Mathf.DeltaAngle(m_rb.transform.eulerAngles.y, m_prevAngles.y),
					Mathf.Deg2Rad * Mathf.DeltaAngle(m_rb.transform.eulerAngles.z, m_prevAngles.z)
				);

				ForceSeatMI_Utils.LowPassFilter(ref telemetry.bodyAngularVelocity[0].roll,  deltaAngles.z / deltaTime, FSMI_VT_ANGLES_SPEED_LOW_PASS_FACTOR);
				ForceSeatMI_Utils.LowPassFilter(ref telemetry.bodyAngularVelocity[0].pitch, deltaAngles.x / deltaTime, FSMI_VT_ANGLES_SPEED_LOW_PASS_FACTOR);
				ForceSeatMI_Utils.LowPassFilter(ref telemetry.bodyAngularVelocity[0].yaw,   deltaAngles.y / deltaTime, FSMI_VT_ANGLES_SPEED_LOW_PASS_FACTOR);
			}

			m_prevForwardSpeed = forwardSpeed;
			m_prevRightSpeed   = rightSpeed;
			m_prevUpSpeed      = upSpeed;
			m_prevAngles.x     = m_rb.transform.eulerAngles.x;
			m_prevAngles.y     = m_rb.transform.eulerAngles.y;
			m_prevAngles.z     = m_rb.transform.eulerAngles.z;

			telemetry.gearNumber = m_gearNumber;
		}

		public virtual void Pause(bool paused)
		{
			m_firstCall = true;
		}

		public void SetGearNumber(int gearNumber)
		{
			m_gearNumber = (sbyte)gearNumber;
		}
	}
}
