using UnityEngine;

public class ParticleLoopAttraction : MonoBehaviour
{
    public Transform suctionPoint; // Titik pusat penyedotan
    public float suctionSpeed = 5f; // Kecepatan tersedot
    public float spreadSpeed = 3f;  // Kecepatan menyebar
    public float suctionStartTime = 1f; // Mulai tersedot di detik ke berapa

    private ParticleSystem particleSystem;
    private ParticleSystem.Particle[] particles;

    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        int particleCount = particleSystem.particleCount;
        if (particleCount > 0)
        {
            if (particles == null || particles.Length < particleCount)
            {
                particles = new ParticleSystem.Particle[particleCount];
            }

            particleSystem.GetParticles(particles);

            Vector3 center = suctionPoint.position;

            for (int i = 0; i < particleCount; i++)
            {
                float normalizedLifetime = 1f - (particles[i].remainingLifetime / particles[i].startLifetime);

                // Awal partikel menyebar keluar
                if (normalizedLifetime < suctionStartTime / particles[i].startLifetime)
                {
                    Vector3 spreadDirection = (particles[i].position - center).normalized;
                    particles[i].velocity = spreadDirection * spreadSpeed;
                }
                // Setelah itu tersedot ke tengah
                else
                {
                    Vector3 directionToCenter = (center - particles[i].position).normalized;
                    particles[i].velocity = directionToCenter * suctionSpeed;
                }
            }

            particleSystem.SetParticles(particles, particleCount);
        }
    }
}
