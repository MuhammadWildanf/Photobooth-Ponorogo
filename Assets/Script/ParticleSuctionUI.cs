using UnityEngine;

public class ParticleSuctionUI : MonoBehaviour
{
    public RectTransform suctionPoint; // Suction point dalam Canvas (UI)
    public Canvas canvas; // Canvas tempat UI berada
    public float suctionSpeed = 5f;
    public float suctionDelay = 1.5f;

    private ParticleSystem particleSystem;
    private ParticleSystem.Particle[] particles;
    private Camera uiCamera; // Buat Screen Space - Camera

    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = null; // Overlay ga butuh kamera
        }
        else
        {
            uiCamera = canvas.worldCamera; // World Space atau Screen Space - Camera
        }
    }

    void Update()
    {
        int particleCount = particleSystem.particleCount;
        if (particleCount == 0) return;

        if (particles == null || particles.Length < particleCount)
        {
            particles = new ParticleSystem.Particle[particleCount];
        }

        particleSystem.GetParticles(particles);

        Vector3 suctionWorldPosition = GetWorldPositionFromUI(suctionPoint);

        for (int i = 0; i < particleCount; i++)
        {
            Vector3 directionToTarget = (suctionWorldPosition - particles[i].position).normalized;
            particles[i].velocity = Vector3.Lerp(particles[i].velocity, directionToTarget * suctionSpeed, Time.deltaTime * 2f);
        }

        particleSystem.SetParticles(particles, particleCount);
    }

    Vector3 GetWorldPositionFromUI(RectTransform uiElement)
    {
        Vector3 worldPosition;
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // Overlay: langsung pakai posisi layar
            worldPosition = uiElement.position;
        }
        else
        {
            // ScreenSpace-Camera atau WorldSpace
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                uiElement,
                RectTransformUtility.WorldToScreenPoint(uiCamera, uiElement.position),
                uiCamera,
                out worldPosition
            );
        }
        return worldPosition;
    }
}