using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class PlayerSounds : NetworkBehaviour {

    public SoundInfo[] soundInfos;
    public SoundType[] soundTypes;

    public GameObject audioParent;
    public GameObject audioExample;
    
	void Start ()
    {
		if (audioParent != null && audioExample != null)
        {
            for (int i = 0; i < soundTypes.Length; i++)
            {
                for (int a = 0; a < soundTypes[i].audioSources.Length; a++)
                {
                    GameObject newAudio = Instantiate(audioExample, audioParent.transform);
                    soundTypes[i].audioSources[a] = newAudio.GetComponent<AudioSource>();
                }
            }
        }
	}
	
	void Update ()
    {
		
	}

    public void PlaySound(string soundName, Vector3 pos, Vector2 volume, Vector2 pitch, float maxDistance, bool networked)
    {
        if (networked)
        {
            CmdPlaySound(soundName, pos, volume, pitch, maxDistance);
        }
        else
        {
            AudioSource audioSource = null;

            for (int i = 0; i < soundInfos.Length; i++)
            {
                if (soundInfos[i].soundName == soundName)
                {
                    for (int a = 0; a < soundTypes.Length; a++)
                    {
                        soundTypes[a].currentIndex++;
                        if (soundTypes[a].currentIndex >= soundTypes[a].audioSources.Length)
                        {
                            soundTypes[a].currentIndex = 0;
                        }

                        audioSource = soundTypes[a].audioSources[soundTypes[a].currentIndex];
                        audioSource.clip = soundInfos[i].audioClips[Random.Range(0, soundInfos[i].audioClips.Length)];

                        // End
                        a = soundTypes.Length;
                    }
                    // End
                    i = soundInfos.Length;
                }
            }

            // If found sound
            if (audioSource != null)
            {
                //
                if (pos == Vector3.zero && audioSource.transform.parent != audioParent.transform)
                {
                    audioSource.transform.parent = audioParent.transform;
                    audioSource.transform.position = audioParent.transform.position;
                }
                if (pos != Vector3.zero)
                {
                    audioSource.transform.parent = null;
                    audioSource.transform.position = pos;
                }

                // Set other sound values
                audioSource.volume = Random.Range(volume.x, volume.y);
                audioSource.pitch = Random.Range(pitch.x, pitch.y);
                audioSource.maxDistance = maxDistance;
                audioSource.Play();
            }
        }
    }

    [Command]
    void CmdPlaySound(string soundName, Vector3 pos, Vector2 volume, Vector2 pitch, float maxDistance)
    {
        RpcPlaySound(soundName, pos, volume, pitch, maxDistance);
    }

    [ClientRpc]
    void RpcPlaySound(string soundName, Vector3 pos, Vector2 volume, Vector2 pitch, float maxDistance)
    {
        PlaySound(soundName, pos, volume, pitch, maxDistance, false);
    }

}
