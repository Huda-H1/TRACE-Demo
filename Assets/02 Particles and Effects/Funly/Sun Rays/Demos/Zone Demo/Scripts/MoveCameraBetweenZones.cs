using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SunRays
{
  public class MoveCameraBetweenZones : MonoBehaviour
  {	
		public float speed = 10.0f;
		
		private Vector3 m_From;
		private Vector3 m_To;

		void Start() {
			m_From = transform.position;
			m_To = transform.position + new Vector3(21, 0, 0);
		}

		void Update() {
			float rawPercent = (Mathf.Cos(Time.time * speed) + 1.0f) / 2.0f;
			float t = Mathf.SmoothStep(0, 1, rawPercent);
			transform.position = Vector3.Lerp(m_To, m_From, t);
		}
  }

}
