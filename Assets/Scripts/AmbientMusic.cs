using UnityEngine;

// Música ambiente gerada por código (sem precisar de arquivo de áudio):
// um "pad" suave em loop com uma progressão de 4 acordes (Am - F - C - G),
// clima calmo/tenso que combina com o treinamento de resgate.
// Como usar: adicione num objeto da cena (ex: GameSystems). Toca sozinho.
public class AmbientMusic : MonoBehaviour
{
    [Range(0f, 1f)] public float volume = 0.18f;

    const int SAMPLE_RATE = 44100;
    const float CHORD_SECONDS = 4f;

    void Start()
    {
        // Frequências (Hz) de cada acorde: fundamental + terça + quinta (+ oitava)
        float[][] chords =
        {
            new float[] { 220.00f, 261.63f, 329.63f, 440.00f }, // Am
            new float[] { 174.61f, 220.00f, 261.63f, 349.23f }, // F
            new float[] { 130.81f, 164.81f, 196.00f, 261.63f }, // C
            new float[] { 196.00f, 246.94f, 293.66f, 392.00f }, // G
        };

        int chordSamples = (int)(SAMPLE_RATE * CHORD_SECONDS);
        int totalSamples = chordSamples * chords.Length;
        float[] data = new float[totalSamples];

        for (int c = 0; c < chords.Length; c++)
        {
            for (int i = 0; i < chordSamples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                // Envelope: sobe e desce suave dentro do acorde (evita estalos na troca)
                float env = Mathf.Sin(Mathf.PI * i / (float)chordSamples);
                env *= env; // curva mais suave

                float s = 0f;
                foreach (float freq in chords[c])
                    s += Mathf.Sin(2f * Mathf.PI * freq * t) / chords[c].Length;

                // Vibrato/ondulação lenta para dar vida de "pad"
                s *= 0.85f + 0.15f * Mathf.Sin(2f * Mathf.PI * 0.25f * t);

                data[c * chordSamples + i] = s * env;
            }
        }

        AudioClip clip = AudioClip.Create("AmbientPad", totalSamples, 1, SAMPLE_RATE, false);
        clip.SetData(data, 0);

        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = true;
        source.volume = volume;
        source.spatialBlend = 0f; // 2D: toca igual em qualquer lugar do mapa
        source.Play();
    }
}
