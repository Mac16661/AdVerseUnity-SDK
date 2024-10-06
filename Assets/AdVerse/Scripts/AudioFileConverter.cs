using System.IO;
using System.Text;
using UnityEngine;

public static class AudioFileConverter
{
    public static byte[] ConvertAudioClipToWav(AudioClip clip)
    {
        using MemoryStream stream = new();
        WriteWavFile(stream, clip);
        return stream.ToArray();
    }

    private static void WriteWavFile(Stream stream, AudioClip clip)
    {
        int sampleCount = clip.samples * clip.channels;
        int byteRate = clip.frequency * clip.channels * 2; // 16-bit audio
        const int headerSize = 44; // Standard WAV header size

        using BinaryWriter writer = new(stream);
        WriteWavHeader(writer, sampleCount, clip.channels, clip.frequency, byteRate);
        WriteAudioData(writer, clip, sampleCount);
    }

    private static void WriteWavHeader(BinaryWriter writer, int sampleCount, int channels, int frequency, int byteRate)
    {
        writer.Write(Encoding.UTF8.GetBytes("RIFF"));
        writer.Write(36 + sampleCount * 2);
        writer.Write(Encoding.UTF8.GetBytes("WAVE"));
        writer.Write(Encoding.UTF8.GetBytes("fmt "));
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)channels);
        writer.Write(frequency);
        writer.Write(byteRate);
        writer.Write((short)(channels * 2));
        writer.Write((short)16);
        writer.Write(Encoding.UTF8.GetBytes("data"));
        writer.Write(sampleCount * 2);
    }

    private static void WriteAudioData(BinaryWriter writer, AudioClip clip, int sampleCount)
    {
        float[] samples = new float[sampleCount];
        clip.GetData(samples, 0);
        foreach (float sample in samples)
        {
            short intSample = (short)(sample * short.MaxValue);
            writer.Write(intSample);
        }
    }
}