#if UNITY_2020_2_OR_NEWER && TMPRO

using TMPro;
using Unity.Profiling;
using UnityEngine;

namespace JustAssets.AtlasMapPacker
{
    public class StatsDisplay : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _textFPS;
        [SerializeField]
        private TMP_Text _textDrawCalls;
        [SerializeField]
        private TMP_Text _textSetPasses;

        private ProfilerRecorder _drawCallsTimeRecorder;

        private ProfilerRecorder _setPassCallsTimeRecorder;

        private static double GetRecorderFrameAverage(ProfilerRecorder recorder)
        {
            var samplesCount = recorder.Capacity;
            if (samplesCount == 0)
                return 0;

            double r = 0;
            long c = 0;
            unsafe
            {
                var samples = stackalloc ProfilerRecorderSample[samplesCount];
                recorder.CopyTo(samples, samplesCount);
                for (var i = 0; i < samplesCount; ++i)
                {
                    if (samples[i].Value > 0)
                    {
                        r += samples[i].Value * samples[i].Count;
                        c += samples[i].Count;
                    }
                }
                r /= c;
            }

            return r;
        }

        private void OnDisable()
        {
            _setPassCallsTimeRecorder.Dispose();
            _drawCallsTimeRecorder.Dispose();
        }

        private void OnEnable()
        {
            _setPassCallsTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count", 15);
            _drawCallsTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count", 15);
        }

        private void Update()
        {
            _textFPS.text = $"{(int) (1f / Time.smoothDeltaTime)} ({Time.smoothDeltaTime * 1000:F1} ms)";
            _textDrawCalls.text = $"{GetRecorderFrameAverage(_drawCallsTimeRecorder):F0}";
            _textSetPasses.text = $"{GetRecorderFrameAverage(_setPassCallsTimeRecorder):F0}";
        }
    }
}
#endif