using UnityEngine;
using System.Collections;

public class BP_GUIButtons : MonoBehaviour
{
	void OnGUI()
	{
		if (GUI.Button (new Rect (10, 10, 150, 30), "First-Person Scene"))
		{
			Application.LoadLevel ("ExampleFirstPersonScene");
		}
			
		if (GUI.Button (new Rect (10, 40, 150, 30), "Third-Person Scene"))
		{
			Application.LoadLevel ("ExampleThirdPersonScene");
		}
	}
}