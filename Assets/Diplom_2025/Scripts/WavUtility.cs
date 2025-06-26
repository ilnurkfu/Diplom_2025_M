using System;
using System.IO;
using System.Text;
using UnityEngine;

public static class WavUtility
{
    public static byte[] FromAudioClip(AudioClip clip)
    {
        // Получаем массив флоатов из AudioClip
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        // Конвертация float -> Int16
        Int16[] intData = new Int16[samples.Length];
        byte[] bytesData = new byte[samples.Length * 2];
        const int rescaleFactor = 32767;

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * rescaleFactor);
            byte[] byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }

        // Пишем WAV-заголовок + данные
        using (var stream = new MemoryStream())
        using (var writer = new BinaryWriter(stream))
        {
            WriteWavHeader(writer, clip, bytesData.Length);
            writer.Write(bytesData);
            return stream.ToArray();
        }
    }

    private static void WriteWavHeader(BinaryWriter writer, AudioClip clip, int dataLength)
    {
        int hz = clip.frequency;
        int channels = clip.channels;
        int byteRate = hz * channels * 2;

        // RIFF header
        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(36 + dataLength);
        writer.Write(Encoding.ASCII.GetBytes("WAVE"));

        // fmt subchunk
        writer.Write(Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16);                  // Subchunk1Size
        writer.Write((ushort)1);           // AudioFormat = PCM
        writer.Write((ushort)channels);
        writer.Write(hz);
        writer.Write(byteRate);
        writer.Write((ushort)(channels * 2)); // BlockAlign
        writer.Write((ushort)16);             // BitsPerSample

        // data subchunk
        writer.Write(Encoding.ASCII.GetBytes("data"));
        writer.Write(dataLength);
    }
}
