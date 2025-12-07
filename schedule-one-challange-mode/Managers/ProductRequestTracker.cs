using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MelonLoader;

#if MONO
using ScheduleOne.Economy;
using ScheduleOne.GameTime;
using ScheduleOne.Messaging;
using ScheduleOne.NPCs;
using ScheduleOne.Product;
using ScheduleOne.Effects;
using ScheduleOne.DevUtilities;
#endif

namespace challange_mode.Managers
{
    /// <summary>
    /// Tracks failed product requests per customer and manages messaging cooldowns
    /// </summary>
    public static class ProductRequestTracker
    {
        // Track last message time per customer ID
        private static Dictionary<string, DateTime> lastMessageTime = new Dictionary<string, DateTime>();
        
        // Track failed request counts since last message
        private static Dictionary<string, int> failedRequestCounts = new Dictionary<string, int>();

        private static readonly string[] MessageTemplates = new string[]
        {
            "Hey, been looking for good product lately but can't find what I need. You got anything?",
            "Yo, I need some gear but nothing's matching what I'm after. Can you help me out?",
            "Been searching for something specific. You carrying what I need?",
            "I'm looking for product but what you got isn't quite right. Got anything else?",
            "Hey man, I need to restock but I'm not seeing what I want. What else you got?",
            "I've been trying to find the right stuff but no luck. You holding?",
            "Need some product that matches my preferences. Can you sort me out?"
        };

        private static readonly string[] EffectSpecificTemplates = new string[]
        {
            "I'm after something with {0} properties. Can you get that for me?",
            "Looking for gear with {0} effects. You got any?",
            "Need product that's {0}. Can you help?",
            "I want something more {0}. What you got?"
        };

        private static readonly string[] QualityFocusedTemplates = new string[]
        {
            "Need {0} grade gear. What you got for me?",
            "I only want {0} quality stuff. You carrying any?",
            "Looking for {0} standard product. Can you provide?",
            "I'm after {0} tier quality. Got anything like that?"
        };

        private static readonly string[] DrugTypeTemplates = new string[]
        {
            "Looking for some quality {0}. Hit me up if you have any.",
            "Need to get some {0}. You got stock?",
            "I'm after {0} specifically. Can you hook me up?",
            "Got any {0}? That's what I'm looking for."
        };

        /// <summary>
        /// Record a failed product search attempt for a customer
        /// </summary>
        public static void RecordFailedRequest(Customer customer)
        {
            if (customer == null || customer.NPC == null)
                return;

            if (!ChallengeConfig.ENABLE_PRODUCT_REQUEST_MESSAGES)
                return;

            string customerId = customer.NPC.ID;
            
            if (!failedRequestCounts.ContainsKey(customerId))
                failedRequestCounts[customerId] = 0;
            failedRequestCounts[customerId]++;

            if (ShouldSendMessage(customerId))
            {
                MelonLogger.Msg($"[ProductRequestTracker] Sending message to {customer.NPC.fullName} after {failedRequestCounts[customerId]} failures");
                SendProductRequestMessage(customer);
                lastMessageTime[customerId] = DateTime.Now;
                failedRequestCounts[customerId] = 0;
            }
        }

        private static bool ShouldSendMessage(string customerId)
        {
            if (!failedRequestCounts.ContainsKey(customerId) || 
                failedRequestCounts[customerId] < ChallengeConfig.MIN_FAILED_REQUESTS_BEFORE_MESSAGE)
                return false;

            if (!lastMessageTime.ContainsKey(customerId))
                return true;

            var daysSinceLastMessage = (DateTime.Now - lastMessageTime[customerId]).TotalDays;
            return daysSinceLastMessage >= ChallengeConfig.MESSAGE_COOLDOWN_DAYS;
        }

        private static void SendProductRequestMessage(Customer customer)
        {
            try
            {
                var messagingManager = NetworkSingleton<MessagingManager>.Instance;
                if (messagingManager == null)
                {
                    Debug.LogWarning("[ProductRequestTracker] MessagingManager not found");
                    return;
                }

                var conversation = messagingManager.GetConversation(customer.NPC);
                if (conversation == null)
                {
                    Debug.LogWarning($"[ProductRequestTracker] No conversation found for {customer.NPC.fullName}");
                    return;
                }

                string messageText = GenerateMessageText(customer);
                
                var message = new Message(messageText, Message.ESenderType.Other, _endOfGroup: true);
                conversation.SendMessage(message, notify: true, network: true);

                MelonLogger.Msg($"[ProductRequestTracker] Successfully sent message to {customer.NPC.fullName}: \"{messageText}\"");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ProductRequestTracker] Error sending message: {ex.Message}");
            }
        }

        private static string GenerateMessageText(Customer customer)
        {
            var customerData = customer.CustomerData;
            if (customerData == null)
                return MessageTemplates[UnityEngine.Random.Range(0, MessageTemplates.Length)];

            // 40% chance for specific message type, 60% generic
            float messageTypeRoll = UnityEngine.Random.value;

            if (messageTypeRoll < 0.15f && customerData.PreferredProperties != null && 
                customerData.PreferredProperties.Count > 0)
            {
                var preferredEffect = customerData.PreferredProperties[
                    UnityEngine.Random.Range(0, customerData.PreferredProperties.Count)];
                string effectName = preferredEffect.name.Replace("Effect_", "").ToLower();
                string template = EffectSpecificTemplates[
                    UnityEngine.Random.Range(0, EffectSpecificTemplates.Length)];
                return string.Format(template, effectName);
            }

            if (messageTypeRoll < 0.30f)
            {
                string qualityName = customerData.Standards.GetName().ToLower();
                string template = QualityFocusedTemplates[
                    UnityEngine.Random.Range(0, QualityFocusedTemplates.Length)];
                return string.Format(template, qualityName);
            }

            if (messageTypeRoll < 0.45f && customerData.DefaultAffinityData != null)
            {
                var affinities = customerData.DefaultAffinityData.ProductAffinities;
                if (affinities != null && affinities.Count > 0)
                {
                    var topAffinity = affinities.OrderByDescending(a => a.Affinity).First();
                    string drugTypeName = topAffinity.DrugType.ToString().ToLower();
                    string template = DrugTypeTemplates[
                        UnityEngine.Random.Range(0, DrugTypeTemplates.Length)];
                    return string.Format(template, drugTypeName);
                }
            }

            return MessageTemplates[UnityEngine.Random.Range(0, MessageTemplates.Length)];
        }

        /// <summary>
        /// Reset failure count for a customer (e.g., after successful purchase)
        /// </summary>
        public static void ResetFailureCount(string customerId)
        {
            if (failedRequestCounts.ContainsKey(customerId))
                failedRequestCounts[customerId] = 0;
        }

        /// <summary>
        /// Clear all tracking data (for testing or reset purposes)
        /// </summary>
        public static void ClearAllData()
        {
            lastMessageTime.Clear();
            failedRequestCounts.Clear();
        }

        /// <summary>
        /// Get current failure count for debugging
        /// </summary>
        public static int GetFailureCount(string customerId)
        {
            return failedRequestCounts.ContainsKey(customerId) ? failedRequestCounts[customerId] : 0;
        }

        /// <summary>
        /// Save tracking data to persistent storage
        /// </summary>
        public static void SaveData()
        {
            ProductRequestPersistence.SaveData(lastMessageTime, failedRequestCounts);
        }

        /// <summary>
        /// Load tracking data from persistent storage
        /// </summary>
        public static void LoadData()
        {
            ProductRequestPersistence.LoadData(out lastMessageTime, out failedRequestCounts);
        }

        /// <summary>
        /// Get internal dictionaries for persistence (used by persistence system)
        /// </summary>
        internal static Dictionary<string, DateTime> GetLastMessageTimes()
        {
            return lastMessageTime;
        }

        internal static Dictionary<string, int> GetFailedRequestCounts()
        {
            return failedRequestCounts;
        }
    }
}

