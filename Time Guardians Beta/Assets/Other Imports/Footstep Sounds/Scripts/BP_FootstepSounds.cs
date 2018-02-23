using UnityEngine;
using System.Collections;

[System.Serializable]
public class TextureGroup
{
	// Variables specific to the texture footstep sounds array.
    public Texture2D texture;                               // The texture itself which will be tied to a specific AudioClip for playback while walking on that texture.
    public AudioClip stepSound;                             // The sound played while walking on the respective texture for this element in the array.
    public float pitchMin = 0.75f;                          // Minimum pitch applied to the 'random.range' for variation in the footstep sound for the specific texture.
    public float pitchMax = 1.0f;                           // Same as above but for the maximum value in the 'random.range'.
}

public class BP_FootstepSounds : MonoBehaviour
{
	// General configuration variables for the footstep sounds.
	private GameObject footstepSource;						// The AudioSource which will be used to play back the footstep sounds.
    private GameObject footstepSource2;                     // Secondary AudioSource used so that fast-paced footsteps will not interrupt one already playing.
    private bool playOnSource1 = true;                      // Used to alternate between the 2 AudioSources so that faster-paced footstep sounds will not interrupt each other.
	public float audioMinDistance = 15f;					// Controls the minimum distance of the 3D sound played through the audio source created on the character.
    public float audioPositionAdjust = -0.5f;               // Used to tweak the local y position of the AudioSources which get spawned and attached to the character.
	public float soundDelay = 0.25f;						// The delay used between playback of the footstep sounds.
	public float movementBuffer = 0.05f;					// The percentage difference used while comparing the characters current position to their current one; see documentation for more info.
	public float raycastLength = 1f;						// Alters the raycast length used to determine if our character is close enough to move on the ground.
    public float raycastPositionAdjust = 0f;                // Adjusts the y position of the source of the Raycast used to gather texture data.
    public AudioClip defaultFootstepSound;                  // Play the following if we are walking on a texture that is not assigned a footstep sound.
	public float defaultPitchMin = 0.75f;					// Minimum pitch applied to the 'random.range' for variation in the footstep sound.
	public float defaultPitchMax = 1.0f;					// Same as above but for the maximum value in the 'random.range'.
	private bool playMovingSound = true;					// Used to space out the playback rate at which the footstep sounds play.
    public TextureGroup[] textureFootstepSounds;            // Array used to assign specific textures a unique footstep sound to play in place of the default one.

	// Utility functionality.
	private Vector3 currentPosition;						// Holds the characters current position which will be used to determine if they have moved or not.

	void Awake()
	{
		footstepSource = new GameObject();
		footstepSource.transform.parent = gameObject.transform;
        footstepSource.transform.localPosition = new Vector3(0f, audioPositionAdjust, 0f);
		footstepSource.name = "Footstep Audio Source";
		footstepSource.AddComponent<AudioSource>();
		footstepSource.GetComponent<AudioSource>().playOnAwake = false;

        footstepSource2 = new GameObject();
        footstepSource2.transform.parent = gameObject.transform;
        footstepSource2.transform.localPosition = new Vector3(0f, audioPositionAdjust, 0f);
        footstepSource2.name = "Footstep Audio Source 2";
        footstepSource2.AddComponent<AudioSource>();
        footstepSource2.GetComponent<AudioSource>().playOnAwake = false;

		currentPosition = gameObject.transform.position;
	}

    void Update()
	{
		// Check to see if the character has moved since the last frame and only proceed if they have.
		if ((currentPosition - transform.position).sqrMagnitude >= (currentPosition * movementBuffer).sqrMagnitude && playMovingSound)
		{
            // Perform the Raycast.
			RaycastHit hit;

            // Adjust the position of the Raycast origin
            Vector3 pos = new Vector3(transform.position.x, transform.position.y + raycastPositionAdjust, transform.position.z);

            // If the Raycast hits a surface, call the CheckTextureSurface method.
            if (Physics.Raycast(pos, Vector3.down, out hit, raycastLength))
                CheckTextureSurface(hit);

            // Update the currentPosition variable with the new position.
			currentPosition = transform.position;
		}
	}

	// Check the surface to find the correct texture that the character is moving on.
    void CheckTextureSurface(RaycastHit hit)
    {
        string textureName;

		// Check if we are walking on terrain and if so, get the dominant texture.
        if (hit.collider.GetComponent<TerrainCollider>())
        {
            Terrain t = hit.collider.GetComponent<Terrain>();
            TerrainData d = t.terrainData;
            int mapX = (int)(((transform.position.x - hit.transform.position.x) / d.size.x) * d.alphamapWidth);
            int mapZ = (int)(((transform.position.z - hit.transform.position.z) / d.size.z) * d.alphamapHeight);
            float[, ,] splatMaps = d.GetAlphamaps(mapX, mapZ, 1, 1);
            float[] weights = new float[splatMaps.GetUpperBound(2) + 1];
            for (int n = 0; n < weights.Length; n++)
                weights[n] = splatMaps[0, 0, n];

            float maxWeight = 0;
            int maxIndex = 0;

            for (int n = 0; n < weights.Length; n++)
            {
                if (weights[n] > maxWeight)
                {
                    maxIndex = n;
                    maxWeight = weights[n];
                }
            }

            textureName = d.splatPrototypes[maxIndex].texture.name;
        }

		// If we are not walking on terrain, find the texture name to later compare to our array of assigned textures/footstep sounds.
		// If no texture is assigned to a material, we return the "Default" name to play the default footstep sound.
        else
        {
            if (hit.collider.GetComponent<Renderer>())
            {
                Renderer r = hit.collider.GetComponent<Renderer>();
                if (r.material.mainTexture)
                    textureName = r.material.mainTexture.name;

                else
                    textureName = "Default";
            }

            else
                textureName = "Default";
        }

		// Call the following method with the texture name to find and play the correct footstep sound.
        PlayAppropriateFootstepSound(textureName);
    }

	// Determine the appropriate footstep sound to play (if the texture is assigned in the array).
    void PlayAppropriateFootstepSound(string texName)
    {
        AudioClip soundToPlay = defaultFootstepSound;

        // Apply the default footstep sound pitch min/max here. These will be overridden if a texture is found in the array.
        if (playOnSource1)
            footstepSource.GetComponent<AudioSource>().pitch = Random.Range(defaultPitchMin, defaultPitchMax);
        else
            footstepSource2.GetComponent<AudioSource>().pitch = Random.Range(defaultPitchMin, defaultPitchMax);

		// Find and return the correct footstep sound to play (if assigned).
        for (int i = 0; i < textureFootstepSounds.Length; i++)
        {
            if (textureFootstepSounds[i].texture.name == texName)
			{
				// Get the appropriate footstep sound from the array to play back.
                soundToPlay = textureFootstepSounds[i].stepSound;

				// Pull the pitch min/max from the array and apply it to the correct AudioSource.
                if (playOnSource1)
				    footstepSource.GetComponent<AudioSource>().pitch = Random.Range (textureFootstepSounds[i].pitchMin, textureFootstepSounds[i].pitchMax);
                else
                    footstepSource2.GetComponent<AudioSource>().pitch = Random.Range(textureFootstepSounds[i].pitchMin, textureFootstepSounds[i].pitchMax);
			}
        }

		// Alternate playback of the appropriate footstep sound between the AudioSources.
        // This is done so that faster-paced footstep sounds have less of a chance to interrupt the one before it.
        // Play the footstep sound back on AudioSource 1 and then set the bool to play it back on AudioSource 2.
        if (playOnSource1)
        {
            footstepSource.GetComponent<AudioSource>().clip = soundToPlay;
            footstepSource.GetComponent<AudioSource>().volume = 1f;
            footstepSource.GetComponent<AudioSource>().minDistance = audioMinDistance;
            footstepSource.GetComponent<AudioSource>().Play();
            playOnSource1 = false;
        }

        // Play the footstep sound here and then set the bool to play the next one on AudioSource 1 (again).
        else
        {
            footstepSource2.GetComponent<AudioSource>().clip = soundToPlay;
            footstepSource2.GetComponent<AudioSource>().volume = 1f;
            footstepSource2.GetComponent<AudioSource>().minDistance = audioMinDistance;
            footstepSource2.GetComponent<AudioSource>().Play();
            playOnSource1 = true;
        }
		
		// Wait the appropriate amount of time before playing the next footstep sound.
		StartCoroutine(WaitForMovingSound(soundDelay));
    }

	// Used to space out and control the footstep sounds.
	IEnumerator WaitForMovingSound(float soundLength)
	{
		playMovingSound = false;
		yield return new WaitForSeconds(soundLength);
		playMovingSound = true;
	}
}