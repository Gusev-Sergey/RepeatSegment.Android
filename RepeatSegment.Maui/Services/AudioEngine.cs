using Android.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RepeatSegment.App;

/// <summary>
/// Android Audio engine — load, analyse, playback via MediaExtractor + AudioTrack.
/// Public API mirrors WPF AudioEngine.cs for compatibility with shared business logic.
/// </summary>
public class AudioEngine : IDisposable
{
    // ── Properties (matching WPF AudioEngine) ──────────────────────
    public float[]? Samples { get; private set; }
    public float[]? SamplesSmall { get; private set; }
    public int SampleRate { get; private set; } = 44100;
    public int SampleRateSmall { get; private set; } = 1000;
    public TimeSpan Duration { get; private set; } = TimeSpan.Zero;
    public string FilePath { get; private set; } = "";
    public bool IsDisposed { get; private set; }

    private float _volume = 1.0f;
    public float Volume
    {
        get => _volume;
        set => _volume = Math.Clamp(value, 0f, 1f);
    }

    public double PlaybackSpeed { get; set; } = 1.0;
    public int Mp3BitrateKbps { get; set; } = 128;

    // ── Playback internals ─────────────────────────────────────────
    private AudioTrack? _audioTrack;
    private int _playbackHeadSample;   // sample index where current playback segment starts in Samples
    private long _playbackStartNanos;  // System nanoTime when playback of current segment started
    private bool _isPaused;

    // Buffer for the current playback segment (stretched PCM shorts)
    private short[]? _playbackBuffer;

    // ── Load ───────────────────────────────────────────────────────

    /// <summary>Load an MP3 or WAV file via MediaExtractor. Returns true on success.</summary>
    public bool Load(string filePath, Action<string>? onStatus = null)
    {
        if (!File.Exists(filePath))
        {
            Log.Error($"[ERROR] File not found: {filePath}");
            return false;
        }

        StopPlaybackInternal();

        try
        {
            float[]? allSamples = null;

            // 1. Try full sample cache
            if (LoadSamplesCache(filePath, out var cachedSamples, out var cachedRate))
            {
                onStatus?.Invoke("Loading from cache...");
                allSamples = cachedSamples;
                SampleRate = cachedRate;
                FilePath = filePath;
                Duration = TimeSpan.FromSeconds((double)allSamples.Length / SampleRate);
                Samples = allSamples;
                Log.Info($"[INFO] Loaded full samples from cache: {filePath}");
            }
            else
            {
                // 2. Decode from audio file
                onStatus?.Invoke("Decoding audio...");
                var result = DecodeAudioFile(filePath);
                if (result == null)
                    return false;

                allSamples = result.Value.Samples;
                int channels = result.Value.Channels;

                // Mono conversion: average channels if needed
                if (channels > 1)
                {
                    int monoLen = allSamples.Length / channels;
                    var monoSamples = new float[monoLen];
                    for (int i = 0; i < monoLen; i++)
                    {
                        float sum = 0;
                        for (int ch = 0; ch < channels; ch++)
                            sum += allSamples[i * channels + ch];
                        monoSamples[i] = sum / channels;
                    }
                    allSamples = monoSamples;
                }

                FilePath = filePath;
                SampleRate = result.Value.SampleRate;
                Duration = TimeSpan.FromSeconds((double)allSamples.Length / SampleRate);

                // Normalize to peak 1.0
                onStatus?.Invoke("Normalizing...");
                float peak = 0;
                foreach (var s in allSamples)
                {
                    var abs = Math.Abs(s);
                    if (abs > peak) peak = abs;
                }
                if (peak > 0)
                {
                    for (int i = 0; i < allSamples.Length; i++)
                        allSamples[i] /= peak;
                }
                Samples = allSamples;

                // Save full sample cache for next time
                onStatus?.Invoke("Saving cache...");
                SaveSamplesCache(filePath, allSamples!);
            }

            // 3. Waveform (SamplesSmall) — separate lightweight cache
            if (LoadWaveformCache(filePath))
            {
                onStatus?.Invoke("Loading waveform cache...");
                Log.Info($"[INFO] Loaded waveform from cache: {filePath}");
            }
            else
            {
                onStatus?.Invoke("Building waveform...");
                BuildSamplesSmall(allSamples!);
                SaveWaveformCache(filePath);
            }

            Log.Info($"[INFO] Loaded {filePath}, duration={Duration.TotalSeconds:F1}s, sr={SampleRate}, sr_small={SampleRateSmall}");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"[ERROR] Error loading {filePath}: {ex.Message}");
            return false;
        }
    }

    // ── Waveform cache ──────────────────────────────────────────────

    private const int WAVEFORM_CACHE_RATE = 200; // Hz — enough for visual waveform

    private string GetCachePath(string audioPath)
    {
        string dir = Path.GetDirectoryName(audioPath) ?? ".";
        string name = Path.GetFileNameWithoutExtension(audioPath);
        return Path.Combine(dir, $"{name}.waveform");
    }

    /// <summary>Try to load SamplesSmall from cache file. Returns true on success.</summary>
    private bool LoadWaveformCache(string audioPath)
    {
        string cacheFile = GetCachePath(audioPath);
        if (!File.Exists(cacheFile)) return false;

        // Stale check: audio file modified after cache?
        if (File.GetLastWriteTimeUtc(cacheFile) < File.GetLastWriteTimeUtc(audioPath))
        {
            try { File.Delete(cacheFile); } catch { }
            return false;
        }

        try
        {
            using var fs = new FileStream(cacheFile, FileMode.Open, FileAccess.Read);
            using var br = new BinaryReader(fs);

            int count = br.ReadInt32();
            int rate = br.ReadInt32();
            if (count <= 0 || rate <= 0) return false;

            var small = new float[count];
            for (int i = 0; i < count; i++)
                small[i] = br.ReadSingle();

            SamplesSmall = small;
            SampleRateSmall = rate;
            return true;
        }
        catch
        {
            try { File.Delete(cacheFile); } catch { }
            return false;
        }
    }

    /// <summary>Save SamplesSmall to binary cache file.</summary>
    private void SaveWaveformCache(string audioPath)
    {
        if (SamplesSmall == null) return;
        try
        {
            string cacheFile = GetCachePath(audioPath);
            using var fs = new FileStream(cacheFile, FileMode.Create, FileAccess.Write);
            using var bw = new BinaryWriter(fs);

            bw.Write(SamplesSmall.Length);
            bw.Write(SampleRateSmall);
            for (int i = 0; i < SamplesSmall.Length; i++)
                bw.Write(SamplesSmall[i]);

            Log.Info($"[INFO] Waveform cache saved: {cacheFile} ({SamplesSmall.Length} samples)");
        }
        catch (Exception ex)
        {
            Log.Warn($"[WARN] Failed to save waveform cache: {ex.Message}");
        }
    }

    /// <summary>Build SamplesSmall from full-resolution Samples.</summary>
    private void BuildSamplesSmall(float[] allSamples)
    {
        SampleRateSmall = Math.Min(SampleRate, WAVEFORM_CACHE_RATE);
        if (SampleRate > WAVEFORM_CACHE_RATE)
        {
            int factor = SampleRate / SampleRateSmall;
            int smallLen = (allSamples.Length + factor - 1) / factor;
            var small = new float[smallLen];
            for (int i = 0; i < smallLen; i++)
                small[i] = allSamples[Math.Min(i * factor, allSamples.Length - 1)];
            SamplesSmall = small;
            SampleRateSmall = SampleRate / factor;
        }
        else
        {
            SamplesSmall = (float[])allSamples.Clone();
        }
    }

    // ── Full sample cache ──────────────────────────────────────────

    private string GetSamplesCachePath(string audioPath)
    {
        string dir = Path.GetDirectoryName(audioPath) ?? ".";
        string name = Path.GetFileNameWithoutExtension(audioPath);
        return Path.Combine(dir, $"{name}.samples");
    }

    /// <summary>Try to load full Samples from binary cache. Returns true on success.</summary>
    private bool LoadSamplesCache(string audioPath, out float[] samples, out int sampleRate)
    {
        samples = Array.Empty<float>();
        sampleRate = 0;
        string cacheFile = GetSamplesCachePath(audioPath);
        if (!File.Exists(cacheFile)) return false;

        // Stale check
        if (File.GetLastWriteTimeUtc(cacheFile) < File.GetLastWriteTimeUtc(audioPath))
        {
            try { File.Delete(cacheFile); } catch { }
            return false;
        }

        try
        {
            using var fs = new FileStream(cacheFile, FileMode.Open, FileAccess.Read);
            using var br = new BinaryReader(fs);

            int count = br.ReadInt32();
            sampleRate = br.ReadInt32();
            if (count <= 0 || sampleRate <= 0) return false;

            samples = new float[count];
            for (int i = 0; i < count; i++)
                samples[i] = br.ReadSingle();

            return true;
        }
        catch
        {
            try { File.Delete(cacheFile); } catch { }
            return false;
        }
    }

    /// <summary>Save full Samples to binary cache file.</summary>
    private void SaveSamplesCache(string audioPath, float[] samples)
    {
        try
        {
            string cacheFile = GetSamplesCachePath(audioPath);
            using var fs = new FileStream(cacheFile, FileMode.Create, FileAccess.Write);
            using var bw = new BinaryWriter(fs);

            bw.Write(samples.Length);
            bw.Write(SampleRate);
            for (int i = 0; i < samples.Length; i++)
                bw.Write(samples[i]);

            Log.Info($"[INFO] Full samples cache saved: {cacheFile} ({samples.Length} samples)");
        }
        catch (Exception ex)
        {
            Log.Warn($"[WARN] Failed to save samples cache: {ex.Message}");
        }
    }

    /// <summary>Returns the full-quality sample array.</summary>
    public float[]? GetSamples() => Samples;

    // ── Decode audio using MediaExtractor + MediaCodec ─────────────

    private struct DecodeResult
    {
        public float[] Samples;
        public int SampleRate;
        public int Channels;
    }

    private DecodeResult? DecodeAudioFile(string filePath)
    {
        MediaExtractor? extractor = null;
        MediaCodec? codec = null;
        try
        {
            extractor = new MediaExtractor();
            extractor.SetDataSource(filePath);

            // Find audio track
            int audioTrackIndex = -1;
            MediaFormat? format = null;
            for (int i = 0; i < extractor.TrackCount; i++)
            {
                var fmt = extractor.GetTrackFormat(i);
                string? mime = fmt.GetString(MediaFormat.KeyMime);
                if (mime != null && mime.StartsWith("audio/"))
                {
                    audioTrackIndex = i;
                    format = fmt;
                    break;
                }
            }

            if (audioTrackIndex < 0 || format == null)
            {
                Log.Error("[ERROR] No audio track found in file");
                return null;
            }

            extractor.SelectTrack(audioTrackIndex);

            string mimeType = format.GetString(MediaFormat.KeyMime)!;
            codec = MediaCodec.CreateDecoderByType(mimeType);
            codec.Configure(format, null, null, MediaCodecConfigFlags.None);
            codec.Start();

            var bufferInfo = new MediaCodec.BufferInfo();
            var sampleList = new List<float>();
            int sampleRate = format.GetInteger(MediaFormat.KeySampleRate);
            int channels = format.GetInteger(MediaFormat.KeyChannelCount);
            bool inputDone = false;
            bool outputDone = false;

            while (!outputDone)
            {
                // Feed input
                if (!inputDone)
                {
                    int inputIndex = codec.DequeueInputBuffer(10000); // 10ms timeout
                    if (inputIndex >= 0)
                    {
                        var inputBuffer = codec.GetInputBuffer(inputIndex)!;
                        int size = extractor.ReadSampleData(inputBuffer, 0);
                        if (size < 0)
                        {
                            // End of stream
                            codec.QueueInputBuffer(inputIndex, 0, 0, 0, MediaCodecBufferFlags.EndOfStream);
                            inputDone = true;
                        }
                        else
                        {
                            long pts = extractor.SampleTime;
                            codec.QueueInputBuffer(inputIndex, 0, size, pts, MediaCodecBufferFlags.None);
                            extractor.Advance();
                        }
                    }
                }

                // Get output
                int outputIndex = codec.DequeueOutputBuffer(bufferInfo, 10000);
                if (outputIndex >= 0)
                {
                    if ((bufferInfo.Flags & MediaCodecBufferFlags.EndOfStream) != 0)
                    {
                        outputDone = true;
                    }

                    if (bufferInfo.Size > 0)
                    {
                        var outputBuffer = codec.GetOutputBuffer(outputIndex);
                        if (outputBuffer != null)
                        {
                            // Convert 16-bit PCM bytes → float via ByteBuffer.Get()
                            int numShorts = bufferInfo.Size / 2;
                            var floats = new float[numShorts];
                            var bytes = new byte[bufferInfo.Size];
                            outputBuffer.Get(bytes, 0, bufferInfo.Size);
                            for (int i = 0; i < numShorts; i++)
                            {
                                short pcm = (short)(bytes[i * 2] | (bytes[i * 2 + 1] << 8));
                                floats[i] = pcm / 32768f;
                            }
                            sampleList.AddRange(floats);
                        }
                    }
                    codec.ReleaseOutputBuffer(outputIndex, false);
                }
                else if (outputIndex == (int)MediaCodecInfoState.OutputFormatChanged)
                {
                    // Format changed, update (handled automatically)
                }
            }

            return new DecodeResult
            {
                Samples = sampleList.ToArray(),
                SampleRate = sampleRate,
                Channels = channels
            };
        }
        catch (Exception ex)
        {
            Log.Error($"[ERROR] Decoding failed: {ex.Message}");
            return null;
        }
        finally
        {
            codec?.Stop();
            codec?.Release();
            extractor?.Release();
        }
    }

    // ── Fast WSOLA time-stretch (pitch-preserving) ────────────────
    // Same as WPF version — pure math, no platform dependencies.

    public static float[] StretchSola(float[] input, double speed)
    {
        if (Math.Abs(speed - 1.0) < 0.005 || input.Length < 4096)
            return (float[])input.Clone();

        const int frameSize = 4096;
        const int outputHop = 2048;
        const int corrLen = 2048;
        const int searchRadius = 32;

        var window = new float[frameSize];
        for (int i = 0; i < frameSize; i++)
            window[i] = 1f - Math.Abs(2f * i / (frameSize - 1f) - 1f);

        double inputHop = outputHop * speed;
        int maxFrames = (int)((input.Length - frameSize) / inputHop) + 2;
        if (maxFrames < 1) maxFrames = 1;
        int outputLength = maxFrames * outputHop + frameSize;
        var output = new float[outputLength];

        double inputPos = 0;
        for (int outFrame = 0; outFrame < maxFrames; outFrame++)
        {
            int outCenter = outFrame * outputHop + frameSize / 2;
            if (outCenter >= outputLength) break;

            int baseInCenter = (int)Math.Round(inputPos + frameSize / 2.0);
            int inCenter = baseInCenter;

            if (outFrame > 0)
            {
                int overlapOutStart = outCenter - frameSize / 2;
                float bestCorr = float.NegativeInfinity;
                int bestOffset = 0;

                for (int offset = -searchRadius; offset <= searchRadius; offset++)
                {
                    int trialCenter = baseInCenter + offset;
                    int trialStart = trialCenter - frameSize / 2;
                    if (trialStart + corrLen >= input.Length || trialStart < 0)
                        continue;

                    float corr = 0;
                    for (int s = 0; s < corrLen; s++)
                        corr += output[overlapOutStart + s] * input[trialStart + s];

                    if (corr > bestCorr)
                    {
                        bestCorr = corr;
                        bestOffset = offset;
                    }
                }
                inCenter = baseInCenter + bestOffset;
            }

            int inStart = inCenter - frameSize / 2;
            int outStart = outCenter - frameSize / 2;

            for (int k = 0; k < frameSize; k++)
            {
                int inIdx = inStart + k;
                int outIdx = outStart + k;
                if (outIdx < 0 || outIdx >= outputLength) continue;
                float sample = (inIdx >= 0 && inIdx < input.Length) ? input[inIdx] : 0f;
                output[outIdx] += sample * window[k];
            }

            inputPos += inputHop;
            if (inputPos + frameSize > input.Length) break;
        }

        // Clamp and trim trailing silence
        int validEnd = outputLength;
        for (int i = outputLength - 1; i >= 0; i--)
        {
            float v = output[i];
            if (v < -1f) v = -1f;
            if (v > 1f) v = 1f;
            output[i] = v;
            if (Math.Abs(v) > 0.0001f) { validEnd = i + 1; break; }
        }

        if (validEnd > 0 && validEnd < outputLength)
        {
            var trimmed = new float[validEnd];
            Array.Copy(output, trimmed, validEnd);
            return trimmed;
        }

        return output;
    }

    /// <summary>Naive linear-interpolation time-stretch — O(N), no pitch preservation.
    /// Used as a lightweight alternative to WSOLA for performance testing.</summary>
    public static float[] StretchNaive(float[] input, double speed)
    {
        if (Math.Abs(speed - 1.0) < 0.005)
            return (float[])input.Clone();

        int outputLen = (int)(input.Length / speed);
        var output = new float[outputLen];

        for (int i = 0; i < outputLen; i++)
        {
            double srcIdx = i * speed;
            int idx = (int)srcIdx;
            float frac = (float)(srcIdx - idx);

            float a = input[idx];
            float b = (idx + 1 < input.Length) ? input[idx + 1] : a;
            output[i] = a + (b - a) * frac;
        }

        return output;
    }

    // ── Playback control (AudioTrack) ──────────────────────────────

    private short[] FloatsToPcm16(float[] samples)
    {
        var pcm = new short[samples.Length];
        for (int i = 0; i < samples.Length; i++)
        {
            float s = Math.Clamp(samples[i], -1f, 1f);
            pcm[i] = (short)(s * 32767);
        }
        return pcm;
    }

    private byte[] ShortsToBytes(short[] shorts)
    {
        var bytes = new byte[shorts.Length * 2];
        for (int i = 0; i < shorts.Length; i++)
        {
            bytes[i * 2] = (byte)(shorts[i] & 0xFF);
            bytes[i * 2 + 1] = (byte)((shorts[i] >> 8) & 0xFF);
        }
        return bytes;
    }

    /// <summary>Start playback from the given position in seconds.</summary>
    public void Play(double positionSeconds = 0.0)
    {
        if (Samples == null || IsDisposed)
            return;

        StopPlaybackInternal();

        int startSample = (int)(positionSeconds * SampleRate);
        if (startSample >= Samples.Length)
            startSample = 0;

        var segmentSamples = new float[Samples.Length - startSample];
        Array.Copy(Samples, startSample, segmentSamples, 0, segmentSamples.Length);

        float[] played = StretchSola(segmentSamples, PlaybackSpeed);
        short[] pcm = FloatsToPcm16(played);
        byte[] pcmBytes = ShortsToBytes(pcm);

        int bufferSize = AudioTrack.GetMinBufferSize(SampleRate, ChannelOut.Mono, Encoding.Pcm16bit);
        if (bufferSize < pcmBytes.Length) bufferSize = pcmBytes.Length;

        var trackBuilder = new AudioTrack.Builder();
        var attrBuilder = new AudioAttributes.Builder();
        attrBuilder.SetUsage(AudioUsageKind.Media);
        attrBuilder.SetContentType(AudioContentType.Music);
        trackBuilder.SetAudioAttributes(attrBuilder.Build()!);
        var fmtBuilder = new AudioFormat.Builder();
        fmtBuilder.SetEncoding(Encoding.Pcm16bit);
        fmtBuilder.SetSampleRate(SampleRate);
        fmtBuilder.SetChannelMask(ChannelOut.Mono);
        trackBuilder.SetAudioFormat(fmtBuilder.Build()!);
        trackBuilder.SetBufferSizeInBytes(bufferSize);
        trackBuilder.SetTransferMode(AudioTrackMode.Static);
        _audioTrack = trackBuilder.Build()!;

        _playbackBuffer = pcm;
        _playbackHeadSample = startSample;
        _playbackStartNanos = Java.Lang.JavaSystem.NanoTime();
        _isPaused = false;

        _audioTrack.Write(pcmBytes, 0, pcmBytes.Length);
        _audioTrack.Play();

        Log.Info($"[INFO] Playback started from {positionSeconds:F1}s, speed={PlaybackSpeed:F2}");
    }

    /// <summary>Play a pre-extracted float[] segment directly.</summary>
    public void PlaySegment(float[] segmentSamples)
    {
        if (Samples == null || IsDisposed)
            return;

        StopPlaybackInternal();

        float[] played = Math.Abs(PlaybackSpeed - 1.0) < 0.005
            ? (float[])segmentSamples.Clone()
            : StretchSola(segmentSamples, PlaybackSpeed);
        short[] pcm = FloatsToPcm16(played);
        byte[] pcmBytes = ShortsToBytes(pcm);

        int bufferSize = AudioTrack.GetMinBufferSize(SampleRate, ChannelOut.Mono, Encoding.Pcm16bit);
        if (bufferSize < pcmBytes.Length) bufferSize = pcmBytes.Length;

        var trackBuilder = new AudioTrack.Builder();
        var attrBuilder = new AudioAttributes.Builder();
        attrBuilder.SetUsage(AudioUsageKind.Media);
        attrBuilder.SetContentType(AudioContentType.Music);
        trackBuilder.SetAudioAttributes(attrBuilder.Build()!);
        var fmtBuilder = new AudioFormat.Builder();
        fmtBuilder.SetEncoding(Encoding.Pcm16bit);
        fmtBuilder.SetSampleRate(SampleRate);
        fmtBuilder.SetChannelMask(ChannelOut.Mono);
        trackBuilder.SetAudioFormat(fmtBuilder.Build()!);
        trackBuilder.SetBufferSizeInBytes(bufferSize);
        // MODE_STATIC: сначала данные, потом Play — надёжнее для коротких сегментов
        trackBuilder.SetTransferMode(AudioTrackMode.Static);
        _audioTrack = trackBuilder.Build()!;

        _playbackBuffer = pcm;
        _playbackHeadSample = 0;
        _playbackStartNanos = Java.Lang.JavaSystem.NanoTime();
        _isPaused = false;

        _audioTrack.Write(pcmBytes, 0, pcmBytes.Length);
        _audioTrack.Play();

        Log.Info($"[INFO] PlaySegment started, {segmentSamples.Length} samples, speed={PlaybackSpeed:F2}, stretched={played.Length}");
    }

    /// <summary>Pause playback, keeping position.</summary>
    public void Pause()
    {
        if (_audioTrack == null || IsDisposed)
            return;

        _isPaused = true;
        _audioTrack.Pause();
        Log.Info("[INFO] Playback paused");
    }

    /// <summary>Resume playback after Pause().</summary>
    public void Resume()
    {
        if (_audioTrack == null || IsDisposed)
            return;
        if (!_isPaused)
            return;

        _isPaused = false;
        _audioTrack.Play();
        Log.Info("[INFO] Playback resumed");
    }

    /// <summary>Stop playback and reset.</summary>
    public void Stop()
    {
        _isPaused = false;
        StopPlaybackInternal();
        Log.Info("[INFO] Playback stopped");
    }

    /// <summary>Seek to a new position without restarting playback.</summary>
    public void Seek(double positionSeconds)
    {
        if (Samples == null || IsDisposed)
            return;

        bool wasPlaying = _audioTrack != null && _audioTrack.PlayState == PlayState.Playing;

        StopPlaybackInternal();

        int startSample = (int)(positionSeconds * SampleRate);
        if (startSample < 0) startSample = 0;
        if (startSample >= Samples.Length) startSample = Samples.Length - 1;

        var segmentSamples = new float[Samples.Length - startSample];
        Array.Copy(Samples, startSample, segmentSamples, 0, segmentSamples.Length);

        float[] played = StretchSola(segmentSamples, PlaybackSpeed);
        short[] pcm = FloatsToPcm16(played);
        byte[] pcmBytes = ShortsToBytes(pcm);

        int bufferSize = AudioTrack.GetMinBufferSize(SampleRate, ChannelOut.Mono, Encoding.Pcm16bit);
        if (bufferSize < pcmBytes.Length) bufferSize = pcmBytes.Length;

        var trackBuilder = new AudioTrack.Builder();
        var attrBuilder = new AudioAttributes.Builder();
        attrBuilder.SetUsage(AudioUsageKind.Media);
        attrBuilder.SetContentType(AudioContentType.Music);
        trackBuilder.SetAudioAttributes(attrBuilder.Build()!);
        var fmtBuilder = new AudioFormat.Builder();
        fmtBuilder.SetEncoding(Encoding.Pcm16bit);
        fmtBuilder.SetSampleRate(SampleRate);
        fmtBuilder.SetChannelMask(ChannelOut.Mono);
        trackBuilder.SetAudioFormat(fmtBuilder.Build()!);
        trackBuilder.SetBufferSizeInBytes(bufferSize);
        trackBuilder.SetTransferMode(AudioTrackMode.Static);
        _audioTrack = trackBuilder.Build()!;

        _playbackBuffer = pcm;
        _playbackHeadSample = startSample;
        _playbackStartNanos = Java.Lang.JavaSystem.NanoTime();
        _isPaused = false;

        _audioTrack.Write(pcmBytes, 0, pcmBytes.Length);
        _audioTrack.Play();

        if (wasPlaying)
            _audioTrack.Play();

        Log.Info($"[INFO] Seek to {positionSeconds:F1}s, speed={PlaybackSpeed:F2}");
    }

    /// <summary>Get current playback position in seconds.</summary>
    public double GetCurrentPosition()
    {
        if (Samples == null || IsDisposed || _playbackBuffer == null)
            return 0.0;

        if (_isPaused || _audioTrack == null)
            return _playbackHeadSample / (double)SampleRate;

        long elapsedNanos = Java.Lang.JavaSystem.NanoTime() - _playbackStartNanos;
        double playedDurationSec = elapsedNanos / 1_000_000_000.0;
        // Reverse time-stretch: actual source samples played = playedDuration * SampleRate / speed
        double currentSamplePos = _playbackHeadSample + (playedDurationSec * SampleRate / PlaybackSpeed);

        if (currentSamplePos >= Samples.Length)
            currentSamplePos = Samples.Length;

        double position = currentSamplePos / SampleRate;
        if (position >= Duration.TotalSeconds)
            position = Duration.TotalSeconds;

        return position;
    }

    // ── Segment extraction (from in-memory Samples) ────────────────

    public float[]? GetPlaySamples(double t1, double t2, int? samplingRate = null)
    {
        if (Samples == null) return null;
        int sr = samplingRate ?? SampleRate;
        int start = (int)(t1 * sr);
        int end = (int)(t2 * sr);
        end = Math.Min(end, Samples.Length);
        if (start >= end) return null;
        var result = new float[end - start];
        Array.Copy(Samples, start, result, 0, result.Length);
        return result;
    }

    public float[]? GetPlaySamplesToEnd(double t1, int? samplingRate = null)
    {
        if (Samples == null) return null;
        int sr = samplingRate ?? SampleRate;
        int start = (int)(t1 * sr);
        if (start >= Samples.Length) return null;
        var result = new float[Samples.Length - start];
        Array.Copy(Samples, start, result, 0, result.Length);
        return result;
    }

    /// <summary>Extract a chunk of audio to a temporary WAV file for transcription.</summary>
    public string ExtractChunk(double t1Sec, double t2Sec)
    {
        if (Samples == null)
            throw new InvalidOperationException("Audio not loaded");

        int start = (int)(t1Sec * SampleRate);
        int end = (int)(t2Sec * SampleRate);
        end = Math.Min(end, Samples.Length);
        int length = end - start;
        if (length <= 0)
            throw new ArgumentException("Invalid segment bounds");

        string tmpPath = Path.Combine(Path.GetTempPath(), $"chunk_{Guid.NewGuid()}.wav");
        WriteWavFile(tmpPath, Samples, start, length, 48000);
        return tmpPath;
    }

    public string SaveSnippetWav(double t1, double t2)
    {
        if (Samples == null)
            throw new InvalidOperationException("Audio not loaded");

        int start = (int)((t1 - 0.02) * SampleRate);
        int end = (int)((t2 + 0.02) * SampleRate);
        if (start < 0) start = 0;
        end = Math.Min(end, Samples.Length);
        int length = end - start;
        if (length <= 0)
            throw new ArgumentException("Invalid snippet bounds");

        string snippetDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RepeatSegment", "decks", "media");
        Directory.CreateDirectory(snippetDir);
        string path = Path.Combine(snippetDir, $"snippet_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.wav");

        var chunkSamples = new float[length];
        Array.Copy(Samples, start, chunkSamples, 0, length);

        int fadeInSamples = (int)(0.005 * SampleRate);
        int fadeOutSamples = (int)(0.005 * SampleRate);
        for (int i = 0; i < length; i++)
        {
            float gain = 1f;
            if (i < fadeInSamples) gain = (float)i / fadeInSamples;
            else if (i >= length - fadeOutSamples) gain = (float)(length - 1 - i) / fadeOutSamples;
            chunkSamples[i] *= gain;
        }

        WriteWavFile(path, chunkSamples, 0, length, SampleRate);
        return path;
    }

    /// <summary>Save snippet as MP3. On Android with NAudio unavailable, saves WAV instead.</summary>
    public string SaveSnippetMp3(double t1, double t2)
    {
        return SaveSnippetWav(t1, t2);
    }

    public float[]? GetPlotSamples(double t1Sec, double t2Sec)
    {
        if (SamplesSmall == null) return null;
        int sr = SampleRateSmall;
        int start = (int)(t1Sec * sr);
        int end = (int)(t2Sec * sr);
        end = Math.Min(end, SamplesSmall.Length);
        if (start >= end) return null;
        var result = new float[end - start];
        Array.Copy(SamplesSmall, start, result, 0, result.Length);
        return result;
    }

    // ── WAV file writer (no NAudio dependency) ────────────────────

    private void WriteWavFile(string path, float[] samples, int offset, int count, int targetSampleRate)
    {
        int sourceRate = SampleRate;
        // Resample if needed
        float[] outputSamples;
        int outputSampleRate;
        if (targetSampleRate != 0 && targetSampleRate != sourceRate)
        {
            outputSampleRate = targetSampleRate;
            double ratio = (double)sourceRate / targetSampleRate;
            int dstLen = (int)(count / ratio);
            outputSamples = new float[dstLen];
            for (int i = 0; i < dstLen; i++)
            {
                double srcIdx = i * ratio;
                int srcI = (int)srcIdx;
                float frac = (float)(srcIdx - srcI);
                float s0 = samples[offset + srcI];
                float s1 = (srcI + 1 < offset + count) ? samples[offset + srcI + 1] : s0;
                outputSamples[i] = s0 + (s1 - s0) * frac;
            }
        }
        else
        {
            outputSampleRate = sourceRate;
            outputSamples = new float[count];
            Array.Copy(samples, offset, outputSamples, 0, count);
        }

        using var fs = new FileStream(path, FileMode.Create);
        using var bw = new BinaryWriter(fs);

        int dataSize = outputSamples.Length * 2; // 16-bit mono
        int chunkSize = 36 + dataSize;

        // RIFF header
        bw.Write(new[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' });
        bw.Write(chunkSize);
        bw.Write(new[] { (byte)'W', (byte)'A', (byte)'V', (byte)'E' });

        // fmt chunk
        bw.Write(new[] { (byte)'f', (byte)'m', (byte)'t', (byte)' ' });
        bw.Write(16); // chunk size
        bw.Write((short)1); // PCM
        bw.Write((short)1); // mono
        bw.Write(outputSampleRate);
        bw.Write(outputSampleRate * 2); // byte rate
        bw.Write((short)2); // block align
        bw.Write((short)16); // bits per sample

        // data chunk
        bw.Write(new[] { (byte)'d', (byte)'a', (byte)'t', (byte)'a' });
        bw.Write(dataSize);

        for (int i = 0; i < outputSamples.Length; i++)
        {
            float s = Math.Clamp(outputSamples[i], -1f, 1f);
            short pcm = (short)(s * 32767);
            bw.Write(pcm);
        }
    }

    // ── Dispose ────────────────────────────────────────────────────

    public void Dispose()
    {
        IsDisposed = true;
        StopPlaybackInternal();
        Samples = null;
        SamplesSmall = null;
    }

    // ── Private helpers ────────────────────────────────────────────

    private void StopPlaybackInternal()
    {
        if (_audioTrack != null)
        {
            try
            {
                if (_audioTrack.PlayState == PlayState.Playing)
                    _audioTrack.Stop();
                _audioTrack.Release();
            }
            catch { }
            _audioTrack = null;
        }
        _playbackBuffer = null;
        _isPaused = false;
    }
}

/// <summary>
/// Minimal logger, mirrors Python logging.getLogger('SEAB.audio').
/// Shared with WPF Log class — both in namespace RepeatSegment.App.
/// On Android, writes to app data directory.
/// </summary>
internal static class Log
{
    private static readonly string LogFilePath;
    private static readonly object _lock = new();

    static Log()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        LogFilePath = Path.Combine(appData, "RepeatSegment", "repeat_segment.log");
        Directory.CreateDirectory(Path.GetDirectoryName(LogFilePath)!);
    }

    public static void Info(string message) => WriteLine(message, "INFO");
    public static void Error(string message) => WriteLine(message, "ERROR");
    public static void Warn(string message) => WriteLine(message, "WARN");

    private static void WriteLine(string message, string level)
    {
        string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}";
        System.Diagnostics.Debug.WriteLine(line);
        try
        {
            lock (_lock)
            {
                File.AppendAllText(LogFilePath, line + Environment.NewLine);
            }
        }
        catch { /* best-effort */ }
    }
}
