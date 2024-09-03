using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace UI
{
    public class AudioSettings : MonoBehaviour
    {
        public AudioMixer audioMixer; // Reference to the Audio Mixer
        public Slider masterSlider;
        public Slider musicSlider;
        public Slider sfxSlider;
        public Image masterImage;
        public Image musicImage;
        public Image sfxImage;
        public Texture2D onTexture;
        public Texture2D offTexture;

        private Sprite onSprite;
        private Sprite offSprite;

        void Start()
        {
            onSprite = Sprite.Create(onTexture, new Rect(0, 0, onTexture.width, onTexture.height), Vector2.zero);
            offSprite = Sprite.Create(offTexture, new Rect(0, 0, offTexture.width, offTexture.height), Vector2.zero);

            // Get initial volume settings
            audioMixer.GetFloat("MasterVolume", out float masterVolume);
            audioMixer.GetFloat("MusicVolume", out float musicVolume);
            audioMixer.GetFloat("SFXVolume", out float sfxVolume);
            
            // Set sliders' values based on AudioMixer values
            masterSlider.value = VolumeToSliderValue(masterVolume);
            musicSlider.value = VolumeToSliderValue(musicVolume);
            sfxSlider.value = VolumeToSliderValue(sfxVolume);

            // Add listeners to sliders
            masterSlider.onValueChanged.AddListener(SetMasterVolume);
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        float VolumeToSliderValue(float volume)
        {
            // Convert AudioMixer volume to slider value
            if (volume == -80) return 0; // Muted
            return Mathf.Pow(10, volume / 20); // Convert log scale to linear
        }

        float SliderValueToVolume(float sliderValue)
        {
            // Convert slider value to AudioMixer volume
            if (sliderValue == 0) return -80; // Mute
            return Mathf.Log10(sliderValue) * 20; // Convert linear to log scale
        }

        public void SetMasterVolume(float volume)
        {
            if (volume == 0)
            {
                masterImage.sprite = offSprite;
                audioMixer.SetFloat("MasterVolume", -80); // Mute
            }
            else
            {
                masterImage.sprite = onSprite;
                audioMixer.SetFloat("MasterVolume", SliderValueToVolume(volume)); // Apply volume
                PlayerPrefs.SetFloat("MasterVolume", volume); // Save settings
            }
        }

        public void SetMusicVolume(float volume)
        {
            if (volume == 0)
            {
                musicImage.sprite = offSprite;
                audioMixer.SetFloat("MusicVolume", -80); // Mute
            }
            else
            {
                musicImage.sprite = onSprite;
                audioMixer.SetFloat("MusicVolume", SliderValueToVolume(volume)); // Apply volume
                PlayerPrefs.SetFloat("MusicVolume", volume);
            }
        }

        public void SetSFXVolume(float volume)
        {
            if (volume == 0)
            {
                sfxImage.sprite = offSprite;
                audioMixer.SetFloat("SFXVolume", -80); // Mute
            }
            else
            {
                sfxImage.sprite = onSprite;
                audioMixer.SetFloat("SFXVolume", SliderValueToVolume(volume)); // Apply volume
                PlayerPrefs.SetFloat("SFXVolume", volume);
            }
        }
    }
}
