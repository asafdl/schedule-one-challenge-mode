using HarmonyLib;
using UnityEngine;
using TMPro;
using System.Text;

#if MONO
using ScheduleOne.UI.Handover;
using ScheduleOne.Economy;
using ScheduleOne.Product;
#endif

namespace challange_mode.Patches
{
    [HarmonyPatch(typeof(HandoverScreenDetailPanel), "Open")]
    public class HandoverScreenDetailPanel_Open_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(HandoverScreenDetailPanel __instance, Customer customer)
        {
            if (customer == null || customer.CustomerData == null) return;

            var customerData = customer.CustomerData;
            if (customerData == null || customerData.DefaultAffinityData == null)
                return;

            var affinityData = customerData.DefaultAffinityData;
            if (affinityData.ProductAffinities == null || affinityData.ProductAffinities.Count == 0)
                return;

            // Use EffectsLabel directly - it's where preferred properties are shown
            var effectsLabel = __instance.EffectsLabel;
            if (effectsLabel == null) return;

            // Check if we already added the affinity section (prevent duplicates)
            if (effectsLabel.text.Contains("Drug Affinities:"))
                return;

            StringBuilder affinitySection = new StringBuilder("\n\n<b>Drug Affinities:</b>");

            // Display each drug type with its affinity
            foreach (var affinityEntry in affinityData.ProductAffinities)
            {
                float affinity = affinityEntry.Affinity;
                string drugName = affinityEntry.DrugType.ToString();
                
                // Use simple text indicators based on affinity (-1 to 1)
                string indicator;
                string colorHex;
                
                if (affinity > 0.6f)
                {
                    indicator = "[+++]";
                    colorHex = "4CAF50"; // Green
                }
                else if (affinity > 0.2f)
                {
                    indicator = "[++]";
                    colorHex = "8BC34A"; // Light green
                }
                else if (affinity > -0.2f)
                {
                    indicator = "[+]";
                    colorHex = "FFC107"; // Yellow
                }
                else if (affinity > -0.6f)
                {
                    indicator = "[-]";
                    colorHex = "FF9800"; // Orange
                }
                else
                {
                    indicator = "[--]";
                    colorHex = "F44336"; // Red
                }
                
                affinitySection.Append($"\n<color=#{colorHex}>\u2022  {drugName} {indicator}</color>");
            }

            // Append to existing effects text
            effectsLabel.text += affinitySection.ToString();
        }
    }
}

