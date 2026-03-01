using CorporateChaos.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CorporateChaos.Systems
{
    /// <summary>
    /// Tracks and calculates ending probabilities based on character advice and player responses
    /// Implements Requirements 13.1, 13.2, 13.5
    /// </summary>
    public class EndingProbabilityTracker
    {
        private readonly ExtendedStoryModeData storyData;
        private readonly Company company;
        private readonly Dictionary<string, List<AdviceRecord>> adviceHistory;

        public EndingProbabilityTracker(ExtendedStoryModeData storyData, Company company)
        {
            this.storyData = storyData;
            this.company = company;
            this.adviceHistory = new Dictionary<string, List<AdviceRecord>>();
        }

        /// <summary>
        /// Records when a character provides advice to the player
        /// </summary>
        public void RecordCharacterAdvice(string characterId, string adviceType, int quarter)
        {
            if (!adviceHistory.ContainsKey(characterId))
                adviceHistory[characterId] = new List<AdviceRecord>();

            adviceHistory[characterId].Add(new AdviceRecord
            {
                CharacterId = characterId,
                AdviceType = adviceType,
                Quarter = quarter,
                WasFollowed = null // Not yet determined
            });
        }

        /// <summary>
        /// Records whether the player followed or ignored character advice
        /// </summary>
        public void RecordAdviceResponse(string characterId, string adviceType, bool followed, int quarter)
        {
            if (!adviceHistory.ContainsKey(characterId))
                return;

            var advice = adviceHistory[characterId]
                .Where(a => a.AdviceType == adviceType && a.Quarter == quarter)
                .OrderByDescending(a => a.Quarter)
                .FirstOrDefault();

            if (advice != null)
            {
                advice.WasFollowed = followed;
                UpdateEndingProbabilitiesFromAdvice(characterId, adviceType, followed);
            }
        }

        /// <summary>
        /// Calculates comprehensive ending probabilities based on company state and character interactions
        /// </summary>
        public Dictionary<EndingType, double> CalculateEndingProbabilities()
        {
            var probabilities = new Dictionary<EndingType, double>();

            // Base probabilities from company state
            probabilities[EndingType.MarketDominance] = CalculateMarketDominanceProbability();
            probabilities[EndingType.ConglomerateBuyout] = CalculateConglomerateBuyoutProbability();
            probabilities[EndingType.BankruptcyFailure] = CalculateBankruptcyProbability();
            probabilities[EndingType.LostManpowerFailure] = CalculateLostManpowerProbability();
            probabilities[EndingType.HealthRetirement] = CalculateHealthRetirementProbability();
            probabilities[EndingType.GracefulRetirement] = CalculateGracefulRetirementProbability();

            // Apply character-specific modifiers
            ApplyCharacterAdviceModifiers(probabilities);

            // Normalize probabilities to ensure they sum to a reasonable range
            NormalizeProbabilities(probabilities);

            // Update story data
            UpdateEndingPathData(probabilities);

            return probabilities;
        }

        private double CalculateMarketDominanceProbability()
        {
            double baseProbability = 0.0;

            // Market share is the primary factor
            if (company.MarketShare >= 65)
                baseProbability = 0.9;
            else if (company.MarketShare >= 50)
                baseProbability = 0.5 + (company.MarketShare - 50) / 30.0;
            else if (company.MarketShare >= 35)
                baseProbability = 0.2 + (company.MarketShare - 35) / 50.0;

            // Lucy Vale (PR/Marketing) advice impact
            if (adviceHistory.ContainsKey("lucinda_vale"))
            {
                var followedAdvice = adviceHistory["lucinda_vale"].Count(a => a.WasFollowed == true);
                var totalAdvice = adviceHistory["lucinda_vale"].Count(a => a.WasFollowed != null);
                if (totalAdvice > 0)
                {
                    double followRate = (double)followedAdvice / totalAdvice;
                    baseProbability += followRate * 0.15; // Up to 15% boost
                }
            }

            // Vincent Duro (Rival CEO) competitive response impact
            if (adviceHistory.ContainsKey("vincent_duro"))
            {
                var ignoredAdvice = adviceHistory["vincent_duro"].Count(a => a.WasFollowed == false);
                baseProbability -= ignoredAdvice * 0.05; // Penalty for ignoring competitive warnings
            }

            return Math.Clamp(baseProbability, 0.0, 1.0);
        }

        private double CalculateConglomerateBuyoutProbability()
        {
            double baseProbability = 0.0;

            // Capital is the primary factor
            if (company.Capital >= 1000000000)
                baseProbability = 0.8;
            else if (company.Capital >= 750000000)
                baseProbability = 0.4 + (company.Capital - 750000000) / 625000000.0;
            else if (company.Capital >= 500000000)
                baseProbability = 0.1 + (company.Capital - 500000000) / 833333333.0;

            // Selena Park (Venture Capitalist) advice impact
            if (adviceHistory.ContainsKey("selena_park"))
            {
                var followedAdvice = adviceHistory["selena_park"].Count(a => a.WasFollowed == true);
                baseProbability += followedAdvice * 0.08; // Each followed advice increases probability
            }

            // Marcus Vey (CFO) high-risk investment impact
            if (adviceHistory.ContainsKey("marcus_vey"))
            {
                var followedRiskyAdvice = adviceHistory["marcus_vey"]
                    .Count(a => a.WasFollowed == true && a.AdviceType.Contains("aggressive"));
                baseProbability += followedRiskyAdvice * 0.05; // Risky investments can accelerate growth
            }

            return Math.Clamp(baseProbability, 0.0, 1.0);
        }

        private double CalculateBankruptcyProbability()
        {
            double baseProbability = 0.0;

            // Consecutive negative quarters is the primary factor
            if (company.ConsecutiveNegativeQuarters >= 3)
                baseProbability = 0.9;
            else if (company.ConsecutiveNegativeQuarters >= 2)
                baseProbability = 0.6;
            else if (company.ConsecutiveNegativeQuarters >= 1)
                baseProbability = 0.3;

            // Capital depletion
            if (company.Capital < 10000)
                baseProbability += 0.3;

            // Harold Finch (Legal Counsel) advice impact
            if (adviceHistory.ContainsKey("harold_finch"))
            {
                var followedAdvice = adviceHistory["harold_finch"].Count(a => a.WasFollowed == true);
                baseProbability -= followedAdvice * 0.1; // Legal advice reduces bankruptcy risk
            }

            // Marcus Vey risky advice impact
            if (adviceHistory.ContainsKey("marcus_vey"))
            {
                var followedRiskyAdvice = adviceHistory["marcus_vey"]
                    .Count(a => a.WasFollowed == true && a.AdviceType.Contains("aggressive"));
                baseProbability += followedRiskyAdvice * 0.08; // Risky investments can backfire
            }

            return Math.Clamp(baseProbability, 0.0, 1.0);
        }

        private double CalculateLostManpowerProbability()
        {
            double baseProbability = 0.0;

            // Employee count is the primary factor
            if (company.EmployeeCount <= 1)
                baseProbability = 0.95;
            else if (company.EmployeeCount <= 3)
                baseProbability = 0.5;
            else if (company.EmployeeCount <= 5)
                baseProbability = 0.2;

            // Morale impact
            if (company.Morale < 20)
                baseProbability += 0.2;

            // Evelyn Cross (HR Head) advice impact
            if (adviceHistory.ContainsKey("evelyn_cross"))
            {
                var followedAdvice = adviceHistory["evelyn_cross"].Count(a => a.WasFollowed == true);
                baseProbability -= followedAdvice * 0.12; // HR advice significantly reduces manpower loss risk
            }

            return Math.Clamp(baseProbability, 0.0, 1.0);
        }

        private double CalculateHealthRetirementProbability()
        {
            double baseProbability = 0.0;

            // High stress factors
            int stressFactors = 0;
            if (company.Risk > 70) stressFactors++;
            if (company.ConsecutiveNegativeQuarters > 0) stressFactors++;
            if (company.Morale < 30) stressFactors++;
            if (company.EmployeeCount < 5) stressFactors++;

            baseProbability = stressFactors * 0.05;

            // Joan's relationship impact (she cares about player wellbeing)
            if (storyData.CharacterRelationships.ContainsKey("joan"))
            {
                var joanRelationship = storyData.CharacterRelationships["joan"];
                if (joanRelationship.PersonalConnection > 60)
                {
                    // Strong personal connection with Joan increases awareness of health issues
                    baseProbability += 0.05;
                }
            }

            return Math.Clamp(baseProbability, 0.0, 0.3); // Cap at 30% as it's a rare ending
        }

        private double CalculateGracefulRetirementProbability()
        {
            double baseProbability = 0.3; // Default baseline

            // Stable company indicators
            if (company.ConsecutiveNegativeQuarters == 0)
                baseProbability += 0.2;
            if (company.Morale > 50)
                baseProbability += 0.1;
            if (company.EmployeeCount >= 10)
                baseProbability += 0.1;
            if (company.Capital > 100000)
                baseProbability += 0.1;

            // Greg Shaw (Operations) advice impact - operational stability
            if (adviceHistory.ContainsKey("gregory_shaw"))
            {
                var followedAdvice = adviceHistory["gregory_shaw"].Count(a => a.WasFollowed == true);
                baseProbability += followedAdvice * 0.05; // Operational efficiency supports stable retirement
            }

            return Math.Clamp(baseProbability, 0.0, 1.0);
        }

        private void ApplyCharacterAdviceModifiers(Dictionary<EndingType, double> probabilities)
        {
            // Sophie Kim (Junior Analyst) - data insights provide hidden bonuses
            if (adviceHistory.ContainsKey("sophie_kim"))
            {
                var followedAdvice = adviceHistory["sophie_kim"].Count(a => a.WasFollowed == true);
                if (followedAdvice >= 3)
                {
                    // Boost all positive endings slightly
                    probabilities[EndingType.MarketDominance] *= 1.1;
                    probabilities[EndingType.ConglomerateBuyout] *= 1.1;
                    probabilities[EndingType.GracefulRetirement] *= 1.05;
                }
            }

            // Overall relationship quality impact
            var averageRelationshipHealth = CalculateAverageRelationshipHealth();
            if (averageRelationshipHealth > 60)
            {
                // Good relationships reduce failure probabilities
                probabilities[EndingType.BankruptcyFailure] *= 0.9;
                probabilities[EndingType.LostManpowerFailure] *= 0.9;
            }
            else if (averageRelationshipHealth < 30)
            {
                // Poor relationships increase failure probabilities
                probabilities[EndingType.BankruptcyFailure] *= 1.1;
                probabilities[EndingType.LostManpowerFailure] *= 1.1;
            }
        }

        private void UpdateEndingProbabilitiesFromAdvice(string characterId, string adviceType, bool followed)
        {
            // This method is called immediately when advice is followed/ignored
            // It can trigger immediate story flags or consequences

            if (!followed)
            {
                // Track ignored advice as a potential story flag
                string flag = $"ignored_{characterId}_advice_{adviceType}";
                if (!storyData.StoryFlags.Contains(flag))
                {
                    storyData.StoryFlags.Add(flag);
                }
            }
            else
            {
                // Track followed advice
                string flag = $"followed_{characterId}_advice_{adviceType}";
                if (!storyData.StoryFlags.Contains(flag))
                {
                    storyData.StoryFlags.Add(flag);
                }
            }
        }

        private void NormalizeProbabilities(Dictionary<EndingType, double> probabilities)
        {
            // Ensure all probabilities are between 0 and 1
            foreach (var key in probabilities.Keys.ToList())
            {
                probabilities[key] = Math.Clamp(probabilities[key], 0.0, 1.0);
            }

            // If multiple high probabilities exist, adjust to make them more realistic
            var highProbEndings = probabilities.Where(p => p.Value > 0.7).ToList();
            if (highProbEndings.Count > 1)
            {
                // Reduce all high probabilities slightly to reflect uncertainty
                foreach (var ending in highProbEndings)
                {
                    probabilities[ending.Key] *= 0.85;
                }
            }
        }

        private void UpdateEndingPathData(Dictionary<EndingType, double> probabilities)
        {
            storyData.EndingProgression.EndingProbabilities = probabilities;
            storyData.EndingProgression.ViableEndings = probabilities
                .Where(p => p.Value > 0.1)
                .Select(p => p.Key)
                .ToList();

            // Update requirements met
            storyData.EndingProgression.EndingRequirementsMet.Clear();
            if (company.MarketShare >= 65)
                storyData.EndingProgression.EndingRequirementsMet.Add("market_dominance_threshold");
            if (company.Capital >= 1000000000)
                storyData.EndingProgression.EndingRequirementsMet.Add("billion_dollar_capital");
            if (company.ConsecutiveNegativeQuarters >= 3)
                storyData.EndingProgression.EndingRequirementsMet.Add("bankruptcy_threshold");
            if (company.EmployeeCount <= 1)
                storyData.EndingProgression.EndingRequirementsMet.Add("lost_manpower_threshold");

            // Update blockers
            storyData.EndingProgression.EndingBlockers.Clear();
            if (company.MarketShare < 35)
                storyData.EndingProgression.EndingBlockers.Add("insufficient_market_share");
            if (company.Capital < 100000)
                storyData.EndingProgression.EndingBlockers.Add("insufficient_capital");
            if (company.EmployeeCount < 3)
                storyData.EndingProgression.EndingBlockers.Add("critically_low_employees");
        }

        private double CalculateAverageRelationshipHealth()
        {
            if (storyData.CharacterRelationships.Count == 0)
                return 50.0; // Neutral default

            double totalHealth = 0;
            foreach (var relationship in storyData.CharacterRelationships.Values)
            {
                // Health is average of trust, respect, and connection (normalized to 0-100)
                double health = (relationship.TrustLevel + relationship.ProfessionalRespect + relationship.PersonalConnection + 300) / 6.0;
                totalHealth += health;
            }

            return totalHealth / storyData.CharacterRelationships.Count;
        }

        /// <summary>
        /// Gets the most likely ending based on current probabilities
        /// </summary>
        public EndingType GetMostLikelyEnding()
        {
            var probabilities = CalculateEndingProbabilities();
            return probabilities.OrderByDescending(p => p.Value).First().Key;
        }

        /// <summary>
        /// Gets advice follow rate for a specific character
        /// </summary>
        public double GetAdviceFollowRate(string characterId)
        {
            if (!adviceHistory.ContainsKey(characterId))
                return 0.0;

            var totalAdvice = adviceHistory[characterId].Count(a => a.WasFollowed != null);
            if (totalAdvice == 0)
                return 0.0;

            var followedAdvice = adviceHistory[characterId].Count(a => a.WasFollowed == true);
            return (double)followedAdvice / totalAdvice;
        }
    }

    /// <summary>
    /// Records a single instance of character advice
    /// </summary>
    public class AdviceRecord
    {
        public string CharacterId { get; set; } = "";
        public string AdviceType { get; set; } = "";
        public int Quarter { get; set; }
        public bool? WasFollowed { get; set; } // null = not yet determined
    }
}
