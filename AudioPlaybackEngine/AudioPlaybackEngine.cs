﻿using System;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace AudioPlaybackEngine {
    class AudioPlaybackEngine : IDisposable {
        private readonly IWavePlayer outputDevice;
        private readonly MixingSampleProvider mixer;

        public int AudioSampleRate { get; private set; }

        public AudioPlaybackEngine(int sampleRate = 44100, int channelCount = 2) {
            AudioSampleRate = sampleRate;
            outputDevice = new WaveOutEvent();
            mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount));
            mixer.ReadFully = true;
            outputDevice.Init(mixer);
            outputDevice.Play();
        }

        public void PlaySound(string fileName) {
            if (fileName == null) return;
            var input = new AudioFileReader(fileName);
            AddMixerInput(new AutoDisposeFileReader(input));
        }

        private ISampleProvider ConvertToRightChannelCount(ISampleProvider input) {
            if (input.WaveFormat.Channels == mixer.WaveFormat.Channels) {
                return input;
            }
            if (input.WaveFormat.Channels == 1 && mixer.WaveFormat.Channels == 2) {
                return new MonoToStereoSampleProvider(input);
            }
            throw new NotImplementedException("Not yet implemented this channel count conversion");
        }

        public void PlaySound(CachedSound sound) {
            if (sound == null) return;
            AddMixerInput(new CachedSoundSampleProvider(sound));
        }

        public void PlaySound(ISampleProvider sound) {
            if (sound == null) return;
            AddMixerInput(sound);
        }

        public void Stop() {
            mixer.RemoveAllMixerInputs();
        }

        private void AddMixerInput(ISampleProvider input) {
            mixer.AddMixerInput(ConvertToRightChannelCount(input));
        }

        public void Dispose() {
            outputDevice.Dispose();
        }

        public static readonly AudioPlaybackEngine Instance = new AudioPlaybackEngine(44100, 2);
    }
}
