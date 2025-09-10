/////////////////////////////////////////////////////////////////////////////////
//
//	vp_RandomSpawner.cs
//	© Opsive. All Rights Reserved.
//	https://twitter.com/Opsive
//	http://www.opsive.com
//
//	description:	spawns a random object from a user populated list
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]

public class vp_RandomSpawner : MonoBehaviour
{

	// sound
	
	AudioSource m_Audio = null;
	public AudioClip Sound = null;
	public float SoundMinPitch = 0.8f;
	public float SoundMaxPitch = 1.2f;
	public bool RandomAngle = true;
	[SerializeField] float _timetoFinishRappelfromhole = 2.5f;
	public bool isSpawnfromRappel;
	[SerializeField] Animator[] findedonRappel_marines;
	public Transform _OnrappelSpwnPos, _OnrappelSpwnrot, finishtherappel_pos = null;
	public List<GameObject> SpawnObjects = null;
	[SerializeField] RopeSim RopeSim = null;

    /// <summary>
    /// 
    /// </summary>
    /// 

    private void SpawnRandomObject()
    {
        if (SpawnObjects == null || SpawnObjects.Count == 0) return;

        int index = Random.Range(0, SpawnObjects.Count);
        var spawnObject = Instantiate(SpawnObjects[index], transform.position, RandomAngle ? Quaternion.Euler(Random.rotation.eulerAngles) : transform.rotation);

      
		if (isSpawnfromRappel)
		{

		
			RopeSim.transform.parent.position = _OnrappelSpwnPos.transform.position;
			RopeSim.transform.parent.rotation = _OnrappelSpwnrot.transform.rotation;

			findedonRappel_marines[index].SetBool("OnRappel", true);



		}
		else Time.fixedDeltaTime = _timetoFinishRappelfromhole * Time.timeScale;
		{


			
			findedonRappel_marines[index].SetBool("OnRappel", false);

			InitiateRappel(spawnObject.transform);
			// it'is needed or not ?

			
		}
    }

    void Awake()
	{
		findedonRappel_marines = FindObjectsByType<Animator>(FindObjectsSortMode.InstanceID);
		if (SpawnObjects == null)
			return;

		SetupAudio();
        SpawnRandomObject();
        RopeSim = GetComponent<RopeSim>();
		int i = (int)Random.Range(0, (SpawnObjects.Count));

		if (SpawnObjects[i] == null)
			return;
		_timetoFinishRappelfromhole = Time.time;
		GameObject obj = (GameObject)vp_Utility.Instantiate(SpawnObjects[i], transform.position, transform.rotation);


		GameObject obj_ = (GameObject)vp_Utility.Instantiate(SpawnObjects[i], _OnrappelSpwnPos.position, _OnrappelSpwnrot.rotation);





       
        obj.transform.Rotate(Random.rotation.eulerAngles);

		obj_.transform.Rotate(transform.rotation.eulerAngles);
		m_Audio = GetComponent<AudioSource>();
		m_Audio.playOnAwake = true;





		// play sound
		if (Sound != null)
		{

			m_Audio.rolloffMode = AudioRolloffMode.Linear;
			m_Audio.clip = Sound;
			m_Audio.pitch = Random.Range(SoundMinPitch, SoundMaxPitch) * Time.timeScale;
			m_Audio.Play();
		}





	}

	private void InitiateRappel(Transform objectTransform)
	{
		// Example logic to position the object and start rappelling
		objectTransform.SetPositionAndRotation(_OnrappelSpwnPos.position, _OnrappelSpwnrot.rotation);
		// Assume RopeSim.StartRappel() handles starting the rappel animation and physics
		RopeSim.start.SetPositionAndRotation(objectTransform.position, objectTransform.rotation);
	}

	private void SetupAudio()
	{
		if (Sound != null)
		{
			m_Audio.playOnAwake = false;
			m_Audio.rolloffMode = AudioRolloffMode.Linear;
			m_Audio.clip = Sound;
			m_Audio.pitch = Random.Range(SoundMinPitch, SoundMaxPitch);
			m_Audio.Play();
		}
	}
}


















