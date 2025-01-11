using UnityEngine;
using UnityEngine.Audio;

// Master
// - Dialog
// - Music
// - Effected
//   - SFX
//   - NPC

public class ModRandomSound : Random_Snd_scr
{
    public LMixerChannels channel;

	private void Awake()
	{
		if (channel == LMixerChannels.None)
			return;

		string channelName = channel.ToString();

		if (channel != LMixerChannels.Effected)
			channelName = channelName.ToUpper();

		AudioMixer mixer = Resources.Load<AudioMixer>("Mixer");

		if (mixer == null)
		{
			Debug.LogWarning("Could not find audio mixer");
			return;
		}

		AudioMixerGroup[] groups = mixer.FindMatchingGroups(channelName);

		if (groups == null || groups.Length == 0)
		{
			Debug.LogWarning("Could not find audio group with name " + channelName);
			return;
		}
			
		if (!TryGetComponent<AudioSource>(out var source))
		{
			Debug.LogWarning("Missing audio source on " + name);
			return;
		}

		source.outputAudioMixerGroup = groups[0];
	}
}
