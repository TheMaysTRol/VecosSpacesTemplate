#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Photon.Voice.Unity.UtilityScripts
{
    [RequireComponent(typeof(Recorder))]
    public class SimpleAmplifier : VoiceComponent
    {
        [SerializeField]
        private int amplificationFactor = 1;

        public int AmplificationFactor
        {
            get { return this.amplificationFactor; }
            set
            {
                if (this.amplificationFactor == value)
                {
                    return;
                }
                this.amplificationFactor = value;
                if (this.floatProcessor != null)
                {
                    this.floatProcessor.AmplificationFactor = this.amplificationFactor;
                }
                if (this.shortProcessor != null)
                {
                    this.shortProcessor.AmplificationFactor = (short)this.amplificationFactor;
                }
            }
        }

        private SimpleAmplifierFloatProcessor floatProcessor;
        private SimpleAmplifierShortProcessor shortProcessor;

        // Message sent by Recorder
        private void PhotonVoiceCreated(Recorder.PhotonVoiceCreatedParams p)
        {
            if (p.Voice is LocalVoiceAudioFloat)
            {
                LocalVoiceAudioFloat v = p.Voice as LocalVoiceAudioFloat;
                this.floatProcessor = new SimpleAmplifierFloatProcessor(this.amplificationFactor);
                v.AddPostProcessor(this.floatProcessor);
            }
            else if (p.Voice is LocalVoiceAudioShort)
            {
                LocalVoiceAudioShort v = p.Voice as LocalVoiceAudioShort;
                this.shortProcessor = new SimpleAmplifierShortProcessor((short)this.amplificationFactor);
                v.AddPostProcessor(this.shortProcessor);
            }
            else if (this.Logger.IsErrorEnabled)
            {
                this.Logger.LogError("LocalVoice object has unexpected value/type: {0}", p.Voice == null ? "null" : p.Voice.GetType().ToString());
            }
        }
    }

    public class SimpleAmplifierFloatProcessor : Voice.IProcessor<float>
    {
        public int AmplificationFactor { get; set; }

        public SimpleAmplifierFloatProcessor(int amplificationFactor)
        {
            this.AmplificationFactor = amplificationFactor;
        }

        public float[] Process(float[] buf)
        {
            for (int i = 0; i < buf.Length; i++)
            {
                buf[i] *= this.AmplificationFactor;
            }
            return buf;
        }

        public void Dispose()
        {

        }
    }

    public class SimpleAmplifierShortProcessor : Voice.IProcessor<short>
    {
        public short AmplificationFactor { get; set; }

        public SimpleAmplifierShortProcessor(short amplificationFactor)
        {
            this.AmplificationFactor = amplificationFactor;
        }

        public short[] Process(short[] buf)
        {
            for (int i = 0; i < buf.Length; i++)
            {
                buf[i] *= this.AmplificationFactor;
            }
            return buf;
        }

        public void Dispose()
        {

        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SimpleAmplifier))]
    public class SimpleAmplifierEditor : Editor
    {
        private SimpleAmplifier simpleAmplifier;

        private void OnEnable()
        {
            this.simpleAmplifier = this.target as SimpleAmplifier;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            this.simpleAmplifier.AmplificationFactor = EditorGUILayout.IntField(new GUIContent("AmplificationFactor", "Amplification Factor"),
                this.simpleAmplifier.AmplificationFactor);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
#endif
}