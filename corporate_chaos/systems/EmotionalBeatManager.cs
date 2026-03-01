using CorporateChaos.Models;

namespace CorporateChaos.Systems
{
    /// <summary>
    /// Manages emotional pacing throughout the story to ensure a balanced mix of positive and negative moments.
    /// Tracks emotional beats across the story timeline to avoid overwhelming players or creating monotony.
    /// </summary>
    public class EmotionalBeatManager
    {
        private ExtendedStoryModeData storyData;
        private Company company;
        private Random random = new Random();

        // Emotional balance parameters
        private const int EMOTIONAL_WINDOW_QUARTERS = 10; // Look back window for balance checking
        private const double TARGET_POSITIVE_RATIO = 0.55; // Target 55% positive moments
        private const double TARGET_NEGATIVE_RATIO = 0.30; // Target 30% negative moments
        private const double TARGET_MIXED_RATIO = 0.15; // Target 15% bittersweet/surprise moments
        private const double BALANCE_TOLERANCE = 0.15; // Allow 15% deviation from targets

        // Intensity thresholds
        private const double HIGH_INTENSITY_THRESHOLD = 0.7;
        private const double LOW_INTENSITY_THRESHOLD = 0.3;
        private const int MIN_QUARTERS_BETWEEN_HIGH_INTENSITY = 3;

        public EmotionalBeatManager(ExtendedStoryModeData storyData, Company company)
        {
            this.storyData = storyData;
            this.company = company;
        }

        /// <summary>
        /// Tags a narrative event with an appropriate emotional beat category
        /// </summary>
        public void TagEventWithEmotionalBeat(NarrativeEvent narrativeEvent, int quarter)
        {
            var category = DetermineEmotionalCategory(narrativeEvent);
            var intensity = CalculateEmotionalIntensity(narrativeEvent, category);

            var emotionalBeat = new EmotionalBeatData
            {
                BeatId = $"beat_{narrativeEvent.EventId}",
                Category = category,
                Quarter = quarter,
                Intensity = intensity,
                Description = narrativeEvent.Title,
                InvolvedCharacters = narrativeEvent.InvolvedCharacters,
                EventId = narrativeEvent.EventId
            };

            storyData.EmotionalBeats.Add(emotionalBeat);
        }

        /// <summary>
        /// Determines the emotional category for a narrative event based on its type and context
        /// </summary>
        private EmotionalBeatCategory DetermineEmotionalCategory(NarrativeEvent narrativeEvent)
        {
            return narrativeEvent.EventType switch
            {
                NarrativeEventType.EmotionalBeat => DetermineFromEventContent(narrativeEvent),
                NarrativeEventType.CharacterIntroduction => EmotionalBeatCategory.Surprise,
                NarrativeEventType.RelationshipMilestone => EmotionalBeatCategory.Triumph,
                NarrativeEventType.PersonalChallenge => EmotionalBeatCategory.Challenge,
                NarrativeEventType.BusinessConflict => EmotionalBeatCategory.Challenge,
                NarrativeEventType.ChoiceConsequence => DetermineFromConsequence(narrativeEvent),
                NarrativeEventType.ActTransition => EmotionalBeatCategory.Bittersweet,
                NarrativeEventType.EndingSetup => EmotionalBeatCategory.Bittersweet,
                _ => EmotionalBeatCategory.Surprise
            };
        }

        /// <summary>
        /// Determines emotional category from event content and gameplay effects
        /// </summary>
        private EmotionalBeatCategory DetermineFromEventContent(NarrativeEvent narrativeEvent)
        {
            // Check for milestone achievements (triumph)
            if (narrativeEvent.GameplayEffects.ContainsKey("milestone_type") ||
                narrativeEvent.GameplayEffects.ContainsKey("celebration_opportunity"))
            {
                return EmotionalBeatCategory.Triumph;
            }

            // Check for crisis or conflict (challenge)
            if (narrativeEvent.GameplayEffects.ContainsKey("crisis_type") ||
                narrativeEvent.GameplayEffects.ContainsKey("leadership_test"))
            {
                return EmotionalBeatCategory.Challenge;
            }

            // Check for reflection or time passage (bittersweet)
            if (narrativeEvent.GameplayEffects.ContainsKey("reflection_opportunity") ||
                narrativeEvent.GameplayEffects.ContainsKey("anniversary"))
            {
                return EmotionalBeatCategory.Bittersweet;
            }

            // Default to surprise for unexpected events
            return EmotionalBeatCategory.Surprise;
        }

        /// <summary>
        /// Determines emotional category from choice consequences
        /// </summary>
        private EmotionalBeatCategory DetermineFromConsequence(NarrativeEvent narrativeEvent)
        {
            // Analyze consequence flags to determine emotional tone
            if (narrativeEvent.GameplayEffects.ContainsKey("consequence_flag"))
            {
                var flag = narrativeEvent.GameplayEffects["consequence_flag"]?.ToString() ?? "";
                
                if (flag.Contains("success") || flag.Contains("reward"))
                    return EmotionalBeatCategory.Triumph;
                
                if (flag.Contains("failure") || flag.Contains("conflict"))
                    return EmotionalBeatCategory.Challenge;
                
                if (flag.Contains("mixed") || flag.Contains("tradeoff"))
                    return EmotionalBeatCategory.Bittersweet;
            }

            return EmotionalBeatCategory.Surprise;
        }

        /// <summary>
        /// Calculates the emotional intensity of an event (0.0 to 1.0)
        /// </summary>
        private double CalculateEmotionalIntensity(NarrativeEvent narrativeEvent, EmotionalBeatCategory category)
        {
            double baseIntensity = 0.5;

            // Adjust based on event type
            baseIntensity += narrativeEvent.EventType switch
            {
                NarrativeEventType.ActTransition => 0.3,
                NarrativeEventType.EndingSetup => 0.4,
                NarrativeEventType.RelationshipMilestone => 0.2,
                NarrativeEventType.BusinessConflict => 0.25,
                _ => 0.0
            };

            // Adjust based on company performance for context
            if (category == EmotionalBeatCategory.Triumph)
            {
                // Triumphs feel more intense when company is doing well
                if (company.MarketShare > 40) baseIntensity += 0.1;
                if (company.Capital > 500000000) baseIntensity += 0.1;
            }
            else if (category == EmotionalBeatCategory.Challenge)
            {
                // Challenges feel more intense when company is struggling
                if (company.ConsecutiveNegativeQuarters > 0) baseIntensity += 0.15;
                if (company.Morale < 40) baseIntensity += 0.1;
            }

            // Adjust based on number of involved characters (more characters = more emotional weight)
            baseIntensity += narrativeEvent.InvolvedCharacters.Count * 0.05;

            // Clamp to valid range
            return Math.Clamp(baseIntensity, 0.0, 1.0);
        }

        /// <summary>
        /// Checks if the emotional balance is healthy within the recent quarter window
        /// </summary>
        public bool IsEmotionalBalanceHealthy(int currentQuarter)
        {
            var recentBeats = GetRecentEmotionalBeats(currentQuarter, EMOTIONAL_WINDOW_QUARTERS);
            
            if (recentBeats.Count < 3)
                return true; // Not enough data to assess balance

            var distribution = CalculateEmotionalDistribution(recentBeats);
            
            // Check if distribution is within acceptable ranges
            bool positiveBalanced = Math.Abs(distribution[EmotionalBeatCategory.Triumph] - TARGET_POSITIVE_RATIO) <= BALANCE_TOLERANCE;
            bool negativeBalanced = Math.Abs(distribution[EmotionalBeatCategory.Challenge] - TARGET_NEGATIVE_RATIO) <= BALANCE_TOLERANCE;
            
            return positiveBalanced && negativeBalanced;
        }

        /// <summary>
        /// Gets emotional beats from recent quarters
        /// </summary>
        private List<EmotionalBeatData> GetRecentEmotionalBeats(int currentQuarter, int windowSize)
        {
            int startQuarter = Math.Max(1, currentQuarter - windowSize);
            
            return storyData.EmotionalBeats
                .Where(beat => beat.Quarter >= startQuarter && beat.Quarter <= currentQuarter)
                .ToList();
        }

        /// <summary>
        /// Calculates the distribution of emotional beat categories
        /// </summary>
        private Dictionary<EmotionalBeatCategory, double> CalculateEmotionalDistribution(List<EmotionalBeatData> beats)
        {
            var distribution = new Dictionary<EmotionalBeatCategory, double>();
            
            if (beats.Count == 0)
            {
                foreach (EmotionalBeatCategory category in Enum.GetValues<EmotionalBeatCategory>())
                {
                    distribution[category] = 0.0;
                }
                return distribution;
            }

            var categoryCounts = beats.GroupBy(b => b.Category)
                                     .ToDictionary(g => g.Key, g => g.Count());

            foreach (EmotionalBeatCategory category in Enum.GetValues<EmotionalBeatCategory>())
            {
                distribution[category] = categoryCounts.ContainsKey(category) 
                    ? (double)categoryCounts[category] / beats.Count 
                    : 0.0;
            }

            return distribution;
        }

        /// <summary>
        /// Suggests an emotional beat category to balance the recent emotional arc
        /// </summary>
        public EmotionalBeatCategory SuggestBalancingCategory(int currentQuarter)
        {
            var recentBeats = GetRecentEmotionalBeats(currentQuarter, EMOTIONAL_WINDOW_QUARTERS);
            
            if (recentBeats.Count < 2)
            {
                // Early in the story, prefer positive or surprise moments
                return random.NextDouble() < 0.6 ? EmotionalBeatCategory.Triumph : EmotionalBeatCategory.Surprise;
            }

            var distribution = CalculateEmotionalDistribution(recentBeats);
            
            // Find which category is most underrepresented
            var deficits = new Dictionary<EmotionalBeatCategory, double>
            {
                [EmotionalBeatCategory.Triumph] = TARGET_POSITIVE_RATIO - distribution[EmotionalBeatCategory.Triumph],
                [EmotionalBeatCategory.Challenge] = TARGET_NEGATIVE_RATIO - distribution[EmotionalBeatCategory.Challenge],
                [EmotionalBeatCategory.Bittersweet] = TARGET_MIXED_RATIO / 2 - distribution[EmotionalBeatCategory.Bittersweet],
                [EmotionalBeatCategory.Surprise] = TARGET_MIXED_RATIO / 2 - distribution[EmotionalBeatCategory.Surprise]
            };

            // Return the category with the largest deficit
            return deficits.OrderByDescending(kvp => kvp.Value).First().Key;
        }

        /// <summary>
        /// Checks if a high-intensity emotional beat can be placed at this quarter
        /// </summary>
        public bool CanPlaceHighIntensityBeat(int currentQuarter)
        {
            var recentBeats = GetRecentEmotionalBeats(currentQuarter, MIN_QUARTERS_BETWEEN_HIGH_INTENSITY);
            
            // Check if any recent beats were high intensity
            return !recentBeats.Any(beat => beat.Intensity >= HIGH_INTENSITY_THRESHOLD);
        }

        /// <summary>
        /// Gets a summary of emotional pacing for the story timeline
        /// </summary>
        public EmotionalPacingSummary GetEmotionalPacingSummary(int currentQuarter)
        {
            var allBeats = storyData.EmotionalBeats.Where(b => b.Quarter <= currentQuarter).ToList();
            var recentBeats = GetRecentEmotionalBeats(currentQuarter, EMOTIONAL_WINDOW_QUARTERS);
            
            return new EmotionalPacingSummary
            {
                TotalBeats = allBeats.Count,
                RecentBeats = recentBeats.Count,
                OverallDistribution = CalculateEmotionalDistribution(allBeats),
                RecentDistribution = CalculateEmotionalDistribution(recentBeats),
                AverageIntensity = allBeats.Any() ? allBeats.Average(b => b.Intensity) : 0.0,
                RecentAverageIntensity = recentBeats.Any() ? recentBeats.Average(b => b.Intensity) : 0.0,
                IsBalanced = IsEmotionalBalanceHealthy(currentQuarter),
                SuggestedNextCategory = SuggestBalancingCategory(currentQuarter),
                CanPlaceHighIntensity = CanPlaceHighIntensityBeat(currentQuarter)
            };
        }

        /// <summary>
        /// Adjusts the emotional intensity of an event based on recent pacing
        /// </summary>
        public double AdjustIntensityForPacing(double baseIntensity, int currentQuarter)
        {
            var recentBeats = GetRecentEmotionalBeats(currentQuarter, 5);
            
            if (recentBeats.Count == 0)
                return baseIntensity;

            double recentAverageIntensity = recentBeats.Average(b => b.Intensity);
            
            // If recent beats have been very intense, reduce intensity
            if (recentAverageIntensity > HIGH_INTENSITY_THRESHOLD)
            {
                return Math.Max(LOW_INTENSITY_THRESHOLD, baseIntensity - 0.2);
            }
            
            // If recent beats have been low intensity, allow higher intensity
            if (recentAverageIntensity < LOW_INTENSITY_THRESHOLD)
            {
                return Math.Min(1.0, baseIntensity + 0.2);
            }

            return baseIntensity;
        }

        /// <summary>
        /// Validates that an emotional beat fits well with the current story pacing
        /// </summary>
        public bool ValidateEmotionalBeatPlacement(EmotionalBeatCategory category, double intensity, int quarter)
        {
            // Check if high intensity beat is too soon after another
            if (intensity >= HIGH_INTENSITY_THRESHOLD && !CanPlaceHighIntensityBeat(quarter))
            {
                return false;
            }

            // Check if this category would create imbalance
            var recentBeats = GetRecentEmotionalBeats(quarter, EMOTIONAL_WINDOW_QUARTERS);
            var distribution = CalculateEmotionalDistribution(recentBeats);
            
            // Calculate what distribution would be after adding this beat
            var totalBeats = recentBeats.Count + 1;
            var categoryCount = recentBeats.Count(b => b.Category == category) + 1;
            var newRatio = (double)categoryCount / totalBeats;
            
            // Check if this would create severe imbalance
            double targetRatio = category switch
            {
                EmotionalBeatCategory.Triumph => TARGET_POSITIVE_RATIO,
                EmotionalBeatCategory.Challenge => TARGET_NEGATIVE_RATIO,
                _ => TARGET_MIXED_RATIO / 2
            };
            
            // Allow some flexibility, but prevent severe imbalance
            return Math.Abs(newRatio - targetRatio) <= BALANCE_TOLERANCE + 0.1;
        }
    }

    /// <summary>
    /// Summary of emotional pacing across the story timeline
    /// </summary>
    public class EmotionalPacingSummary
    {
        public int TotalBeats { get; set; }
        public int RecentBeats { get; set; }
        public Dictionary<EmotionalBeatCategory, double> OverallDistribution { get; set; } = new Dictionary<EmotionalBeatCategory, double>();
        public Dictionary<EmotionalBeatCategory, double> RecentDistribution { get; set; } = new Dictionary<EmotionalBeatCategory, double>();
        public double AverageIntensity { get; set; }
        public double RecentAverageIntensity { get; set; }
        public bool IsBalanced { get; set; }
        public EmotionalBeatCategory SuggestedNextCategory { get; set; }
        public bool CanPlaceHighIntensity { get; set; }
    }
}
