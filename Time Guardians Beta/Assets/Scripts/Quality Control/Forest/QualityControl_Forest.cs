using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

public class QualityControl_Forest : MonoBehaviour {

    public  int[] forestTreeBillboardDistances = new int[] { 50, 50, 50, 75, 100 };

    public  float[] forestWindTurbulance = new float[] { 0, 0, 0.5f, 1, 2 };

    public Terrain[] terrains;

    public bool[] testQuality = new bool[5];

    public WindZone wind;

    public GameObject gameCamera;
    public PostProcessingBehaviour postPro;

	void Start () {
        postPro = gameCamera.GetComponent<PostProcessingBehaviour>();
        for (int i = 0; i < testQuality.Length; i++ )
        {
            if (testQuality[i])
            {
                QualitySettings.SetQualityLevel(i);
            }
        }





        for (int i = 0; i < forestTreeBillboardDistances.Length; i++)
        {
            if (QualitySettings.GetQualityLevel() == i)
            {
                for (int j = 0; j < terrains.Length; j++)
                {
                    terrains[j].treeBillboardDistance = forestTreeBillboardDistances[i];
                    
                }
                wind.windTurbulence = forestWindTurbulance[i];
            }

            if (QualitySettings.GetQualityLevel() == 0)
            {
                postPro.enabled = false;
            }
        }
		
        
    }
	
	
}
