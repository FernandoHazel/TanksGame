using System.Collections;
using UnityEngine;
using UnityEngine.UI;
namespace Tanks.Complete
{
    public class WallHealth : MonoBehaviour
    {
        public float startingHealth = 100f;               // The amount of health each beam starts with.
        public Slider sliderReference;                             // The slider to represent how much health the beam currently has.
        public Slider sliderPrefab;
        public Image fillImage;                           // The image component of the slider.
        public Color m_FullHealthColor = Color.green;    // The color the health bar will be when on full health.
        public Color m_ZeroHealthColor = Color.red;      // The color the health bar will be when on no health.
        public GameObject m_ExplosionPrefab;                // A prefab that will be instantiated in Awake, then used whenever the beam dies.

        private AudioSource m_ExplosionAudio;               // The audio source to play when the beam explodes.
        private ParticleSystem m_ExplosionParticles;        // The particle system the will play when the tank is destroyed.
        private float m_CurrentHealth;                      // How much health the tank currently has.
        private bool m_Dead;                                // Has the tank been reduced beyond zero health yet?
        private void Awake ()
        {
            // Instantiate the explosion prefab and get a reference to the particle system on it.
            m_ExplosionParticles = Instantiate (m_ExplosionPrefab).GetComponent<ParticleSystem> ();

            // Get a reference to the audio source on the instantiated prefab.
            m_ExplosionAudio = m_ExplosionParticles.GetComponent<AudioSource> ();

            // Disable the prefab so it can be activated when it's required.
            m_ExplosionParticles.gameObject.SetActive (false);
        }

        private void OnDestroy()
        {
            if(m_ExplosionParticles != null)
                Destroy(m_ExplosionParticles.gameObject);
        }

        private void OnEnable()
        {
            // When the tank is enabled, reset the tank's health and whether or not it's dead.
            m_CurrentHealth = startingHealth;
            m_Dead = false;
        }

        public void TakeDamage (float amount)
        {
            if (sliderReference == null)
            {
                // Get the canvas and set the UI element
                GameObject canvas = GameObject.Find("ScreenSpaceCameraCanvas");
                if (canvas != null)
                {
                    createUIElement(canvas);
                    // Change the UI elements appropriately.
                    SetHealthUI(amount);
                }
                else
                {
                    Debug.LogError("No canvas with render mode ScreenSpaceCamera found in the scene. Please add one and set its render mode to ScreenSpaceCamera.");
                }
            } else {
                // Change the UI elements appropriately.
                SetHealthUI(amount);
            }
        }

        private void SetHealthUI (float amount)
        {
            if (fillImage != null && sliderReference != null)
            {
                StartCoroutine(hideHealtSlider());

                // Reduce current health by the amount of damage done.
                m_CurrentHealth -= amount;
                
                // Set the slider's value appropriately.
                sliderReference.value = m_CurrentHealth;

                // Interpolate the color of the bar between the choosen colours based on the current percentage of the starting health.
                fillImage.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / startingHealth);

                // If the current health is at or below zero and it has not yet been registered, call OnDeath.
                if (m_CurrentHealth <= 0f && !m_Dead)
                {
                    OnDeath();
                }

            }
        }

        private void createUIElement(GameObject cameraSpaceCanvas)
        {
            // Instantiate prefab
            Slider sliderInstance = Instantiate(sliderPrefab);
            // Set the target of that UI element to this transform
            HealthUIElement healtUIelementRef = sliderInstance.GetComponent<HealthUIElement>();
            healtUIelementRef.setTarget(transform);
            // Parent to the canvas
            sliderInstance.transform.SetParent(cameraSpaceCanvas.transform, false);
            // Reference to this beam
            sliderReference = sliderInstance;
            // Set the slider max value to the max health the tank can have
            sliderReference.maxValue = startingHealth;
            fillImage = healtUIelementRef.getFillImage();
            sliderReference.gameObject.SetActive(true);
        }

        private void OnDeath ()
        {
            // Set the flag so that this function is only called once.
            m_Dead = true;

            // Move the instantiated explosion prefab to the tank's position and turn it on.
            m_ExplosionParticles.transform.position = transform.position;
            m_ExplosionParticles.gameObject.SetActive (true);

            // Play the particle system of the tank exploding.
            m_ExplosionParticles.Play ();

            // Play the tank explosion sound effect.
            m_ExplosionAudio.Play();

            // Turn the tank off.
            gameObject.SetActive (false);
        }

        IEnumerator hideHealtSlider()
        {
            yield return new WaitForSeconds(2f);
            // Activate the health slider
            sliderReference.gameObject.SetActive(false);
        }
    }
}