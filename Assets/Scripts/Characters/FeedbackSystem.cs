using System.Collections.Generic;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace Characters
{
    /// <summary>
    /// Centralized system to play and stop feedbacks (VFX, SFX, screen shake, etc.)
    /// using predefined feedback keys mapped to MMF_Player instances.
    /// </summary>
    public class FeedbackSystem : SerializedMonoBehaviour
    {
        [PropertyTooltip("Dictionary mapping FeedbackKey enums to MMF_Player instances.")]
        [OdinSerialize] 
        private Dictionary<FeedbackKey, MMF_Player> feedbackDictionary;

        /// <summary>
        /// Plays the feedback associated with the given key, if it exists.
        /// </summary>
        /// <param name="key">The feedback key to play.</param>
        public void PlayFeedback(FeedbackKey key)
        {
            if (!feedbackDictionary.TryGetValue(key, out MMF_Player feedback)) return;
            feedback?.PlayFeedbacks();
        }

        /// <summary>
        /// Stops the feedback associated with the given key, if it exists.
        /// </summary>
        /// <param name="key">The feedback key to stop.</param>
        public void StopFeedback(FeedbackKey key)
        {
            if (!feedbackDictionary.TryGetValue(key, out MMF_Player feedback)) return;
            feedback?.StopFeedbacks();
        }
    }

    /// <summary>
    /// Enumeration of predefined feedback events for characters.
    /// Used as keys to trigger corresponding MMF_Player effects.
    /// </summary>
    public enum FeedbackKey
    {
        Jump = 0,        // Triggered when the character jumps
        Land = 1,        // Triggered when landing
        Attack = 2,      // Triggered on attack
        Guard = 3,       // Triggered on guard activation
        TakeDamage = 4,  // Triggered when taking damage
        Iframe = 5       // Triggered when entering invincible (iframe) state
    }
}