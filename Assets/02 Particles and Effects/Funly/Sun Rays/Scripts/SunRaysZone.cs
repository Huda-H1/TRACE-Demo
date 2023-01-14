// Sun Rays by Funly, LLC
// Website: https://funly.io
// Author: Jason Ederle - jason@funly.io

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SunRays {
	public class SunRaysZone : MonoBehaviour {
		[Tooltip("Reference to the Sun Rays script on a camera. If left empty it will search for the image effect on your main camera")]
		public SunRaysImageEffect sunRaysEffect;

		[Tooltip("Settings for this zone, that take effect when camera moves into it.")]
		public SunRaysSettings zoneSettings = new SunRaysSettings();

		private float m_MinDelta = .05f;
		private State m_State = State.Outside;
		private Vector3 m_LastCheckPosition = Vector3.zero;

		// State that indicates if the camera is inside our outside this zone.
		public enum State {
			Inside,
			Outside
		}

		void Start() {
			if (!sunRaysEffect) {
				DiscoverSunRaysEffect();
			}

			CheckForZoneEffectIntersection();
		}
		
		void Update() {
			if (sunRaysEffect == null) {
				return;
			}	
			
			// Prevent jitter if we're sitting right on a intersection boundary.
			Vector3 cameraPosition = sunRaysEffect.transform.position;
			float delta = Vector3.Distance(sunRaysEffect.transform.position, m_LastCheckPosition);
			if (delta < m_MinDelta) {
				return;
			}

			m_LastCheckPosition = cameraPosition;
			CheckForZoneEffectIntersection();
		}

		void CheckForZoneEffectIntersection() {
			if (sunRaysEffect == null) {
				return;
			}
			
			bool isInsideBool = IsPointInsideZone(sunRaysEffect.transform.position);
			State updatedState = isInsideBool ? State.Inside : State.Outside;
			
			if (updatedState == m_State) {
				return;
			}
			
			m_State = updatedState;

			if (m_State == State.Inside) {
				sunRaysEffect.EnteredZone(this);
			} else {
				sunRaysEffect.ExitedZone(this);
			}
		}

		void DiscoverSunRaysEffect() {
			Camera mainCam = Camera.main;
			if (!mainCam) {
				Debug.LogError("You haven't assigned the SunRays script on the zone, and there is no main camera tagged to discover it.");
				return;
			}

			sunRaysEffect = mainCam.GetComponent<SunRaysImageEffect>();

			if (!sunRaysEffect) {
				Debug.LogError("You have a sun ray zone, however there is no main camera with a SunRays image effect assigned.");
			}
		}

		void OnDrawGizmosSelected() {
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(transform.position, transform.lossyScale);
		}

		public bool IsPointInsideZone(Vector3 worldPosition) {
			Bounds b = new Bounds(transform.position, transform.lossyScale);
			return b.Contains(worldPosition);
		}

		void OnDisable() {
			if (!sunRaysEffect) {
				return;
			}

			sunRaysEffect.ExitedZone(this);
		}
	}
}

