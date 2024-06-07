﻿// Copyright Elliot Bentine, 2018-
#if (UNITY_EDITOR) 

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProPixelizer.Tools
{
    [CreateAssetMenu(fileName = "steppedAnimation", menuName = "ProPixelizer/Stepped Animation")]
    public class SteppedAnimation : ScriptableObject
    {
        [SerializeField]
        [Tooltip("The source clips to use.")]
        public List<AnimationClip> SourceClips;

        public string AutoGeneratedName(string clipName) => clipName + "_stepped";

        [SerializeField]
        [Tooltip("How should generated keyframes be placed?")]
        public StepMode KeyframeMode = StepMode.FixedRate;

        [SerializeField]
        [Tooltip("Rate at which automatic keyframes occur.")]
        [Range(0.1f, 120f)]
        public float KeyframeRate;

        //Note - what about when the clip length * rate is not an integer?

        [SerializeField]
        [Tooltip("Times to generate keyframes for.")]
        public List<float> ManualKeyframeTimes;

        public enum StepMode
        {
            FixedRate,
            Manual,
            FixedTimeDelay
        }

        public float FixedTimeDelay = 0.1f;

        public List<float> GetKeyframeTimes(AnimationClip source)
        {
            int number;
            List<float> times;
            switch (KeyframeMode)
            {
                case StepMode.Manual:
                    return ManualKeyframeTimes;
                case StepMode.FixedRate:
                    number = (int)(source.length * KeyframeRate);
                    times = new List<float>();
                    for (int i = 0; i <= number; i++)
                        times.Add(i / KeyframeRate);
                    return times;
                case StepMode.FixedTimeDelay:
                    number = (int)(source.length / FixedTimeDelay);
                    times = new List<float>();
                    for (int i = 0; i <= number; i++)
                        times.Add(i * FixedTimeDelay);
                    return times;
                default:
                    throw new System.Exception("Unhandled mode");
            }
        }

        public void Generate()
        {
            foreach (AnimationClip clip in SourceClips)
            {
                if (clip == null)
                    continue;
                Generate(clip);
            }
        }

        public const string STEPPED = "Stepped";

        public void Generate(AnimationClip sourceClip)
        {
            // Duplicate clip.
            // Note that we need to extract the animation asset and save it to a new file,
            // rather than just copy the source asset. This is because some animations are
            // embedded in other formats, eg fbx.
            
            var sourcePath = AssetDatabase.GetAssetPath(sourceClip);
            var stepPath = AssetDatabase.GetAssetPath(this);
            var outputDir = stepPath.Substring(0, stepPath.LastIndexOf('/'));
            var outputPath = outputDir + "/" + AutoGeneratedName(sourceClip.name) + ".anim";

            // We only instantiate a new asset if one does not yet exist - because this generates a new UUID.
            // If an asset already exists, copy and paste the source asset over it and reload.
            if (AssetDatabase.LoadAssetAtPath<AnimationClip>(outputPath) == null)
                AssetDatabase.CreateAsset(Instantiate(sourceClip), outputPath);
            else
                File.Copy(sourcePath, outputPath, true);

            AnimationClip generatedClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(outputPath);

            var labels = new List<string>(AssetDatabase.GetLabels(generatedClip));
            if (!labels.Contains(STEPPED))
                labels.Add(STEPPED);
            AssetDatabase.SetLabels(generatedClip, labels.ToArray());

            var sampleTimes = GetKeyframeTimes(sourceClip);

            // Recreate each source curve, but resampled and without interpolation.
            foreach (var binding in AnimationUtility.GetCurveBindings(sourceClip))
            {
                var sourceCurve = AnimationUtility.GetEditorCurve(sourceClip, binding);
                var generatedCurve = AnimationUtility.GetEditorCurve(generatedClip, binding);

                // Clear out keys.
                for (int i = generatedCurve.length-1; i >= 0; i--)
                    generatedCurve.RemoveKey(i);

                for (int index = 0; index < sampleTimes.Count; index++)
                {
                    var time = sampleTimes[index];
                    time = Mathf.Clamp(time, 0f, sourceClip.length);

                    Keyframe key = new Keyframe(
                        sampleTimes[index],
                        sourceCurve.Evaluate(time),
                        float.NegativeInfinity,
                        float.PositiveInfinity,
                        0f,
                        0f
                        );
                    generatedCurve.AddKey(key);
                }

                AnimationUtility.SetEditorCurve(generatedClip, binding, generatedCurve);
            }
            
        }
    }
}

 #endif