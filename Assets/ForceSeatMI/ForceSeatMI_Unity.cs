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

using System.Runtime.InteropServices;

namespace MotionSystems
{
	class ForceSeatMI_Unity
	{
		public struct ExtraParameters
		{
			public float yaw;
			public float pitch;
			public float roll;
			public float right; 
			public float up;
			public float forward;
		}

		private ForceSeatMI                     m_api             = null;
		private FSMI_TelemetryACE               m_telemetry       = FSMI_TelemetryACE.Prepare();
		private ForceSeatMI_ITelemetryInterface m_telemetryObject = null;

		public ForceSeatMI_Unity()
		{
			m_api = new ForceSeatMI();

			m_telemetry.structSize = (byte)Marshal.SizeOf(m_telemetry);
			m_telemetry.state = FSMI_State.NO_PAUSE;
		}

		public bool ActivateProfile(string profileName)
		{
			return m_api.ActivateProfile(profileName);
		}
		
		public bool SetAppID(string appId)
		{
			return m_api.SetAppID(appId);
		}

		public void SetTelemetryObject(ForceSeatMI_ITelemetryInterface telemetryObject)
		{
			m_telemetryObject = telemetryObject;

			if (null == m_telemetryObject)
			{
				Pause(true);
			}
		}

		public void Begin()
		{
			m_api.BeginMotionControl();

			if (null != m_telemetryObject)
			{
				m_telemetryObject.Begin();
			}
		}

		public void End()
		{
			if (null  != m_telemetryObject)
			{
				m_telemetryObject.End();
			}

			if (m_api.IsLoaded())
			{
				m_api.EndMotionControl();
				m_api.Dispose();
			}
		}

		public void Update(float deltaTime)
		{
			if (null != m_telemetryObject)
			{
				m_telemetryObject.Update(deltaTime, ref m_telemetry);
			}

			m_api.SendTelemetryACE(ref m_telemetry);
		}

		public void Pause(bool paused)
		{
			if (null != m_telemetryObject)
			{
				m_telemetryObject.Pause(paused);
			}

			if (paused)
			{
				FSMI_TelemetryACE pauseTelemetry = FSMI_TelemetryACE.Prepare();
				pauseTelemetry.state             = paused ? (byte)FSMI_State.PAUSE : (byte)FSMI_State.NO_PAUSE;

				m_api.SendTelemetryACE(ref pauseTelemetry);
			}
		}

		public void AddExtra(ExtraParameters parameters)
		{
			m_telemetry.extraRotation[0].yaw        = parameters.yaw;
			m_telemetry.extraRotation[0].pitch      = parameters.pitch;
			m_telemetry.extraRotation[0].roll       = parameters.roll;
			m_telemetry.extraTranslation[0].right   = parameters.right;
			m_telemetry.extraTranslation[0].upward  = parameters.up;
			m_telemetry.extraTranslation[0].forward = parameters.forward;
		}
	}
}
