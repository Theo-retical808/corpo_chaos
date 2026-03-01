using CorporateChaos.Models;

namespace CorporateChaos.Systems
{
    /// <summary>
    /// Manages the distribution of story content across all 120 quarters to ensure
    /// consistent narrative engagement without overwhelming players or creating gaps.
    /// Integrates with NarrativeEngine and EmotionalBeatManager for balanced pacing.
    /// </summary>
    public class StoryContentDistributor
    {
        private ExtendedStoryModeData storyData;
        private Company company;
        private NarrativeEngine narrativeEngine;
        private EmotionalBeatManager emotionalBeatManager;
        private Random random = new Random();

        // Content distribution parameters
        private const int MIN_EVENTS_PER_QUARTER = 0;  // Some quarters can have no events
        private const int MAX_EVENTS_PER_QUARTER = 3;  // Never more than 3 events
        private const int TARGET_EVENTS_PER_10_QUARTERS = 8; // Average 0.8 events per quarter
        
        // Event frequency by narrative act
        private const double TUTORIAL_EVENT_FREQUENCY = 1.0;      // Every quarter (handled by existing system)
        private const double RISING_ACTION_FREQUENCY = 0.7;       // 70% of quarters
        private const double CLIMAX_FREQUENCY = 0.9;              // 90% of quarters
        private const double RESOLUTION_FREQUENCY = 0.85;         // 85% of quarters
        
        // Gap management
        private const int MAX_QUARTERS_WITHOUT_CONTENT = 3;       // Never more than 3 quarters without content
        private const int MIN_QUARTERS_BETWEEN_MAJOR_EVENTS = 2;  // Space out major events

        // Content type distribution targets (percentages)
        private const double CHARACTER_CONTENT_RATIO = 0.35;      // 35% character-focused
        private const double BUSINESS_CONTENT_RATIO = 0.30;       // 30% business-focused
        private const double MILESTONE_CONTENT_RATIO = 0.20;      // 20% milestones
        private const double MIXED_CONTENT_RATIO = 0.15;          // 15% mixed/other

        public StoryContentDistributor(
            ExtendedStoryModeData storyData, 
            Company company, 
            NarrativeEngine narrativeEngine,
            EmotionalBeatManager emotionalBeatManager)
        {
            this.storyData = storyData;
            this.company = company;
            this.narrativeEngine = narrativeEngine;
            this.emotionalBeatManager = emotionalBeatManager;
        }

        /// <summary>
        /// Ensures story content is available for the specified quarter with appropriate pacing
        /// </summary>
        public List<NarrativeEvent> GetDistributedContentForQuarter(int quarter)
        {
            // Tutorial phase (Q1-10) is handled by existing StoryScript system
            if (quarter <= 10)
            {
                return new List<NarrativeEvent>();
            }

            // Generate events using NarrativeEngine
            var generatedEvents = narrativeEngine.GenerateEventsForQuarter(quarter);

            // Apply distribution rules to ensure proper pacing
            var distributedEvents = ApplyDistributionRules(generatedEvents, quarter);

            // Validate content coverage and fill gaps if needed
            distributedEvents = EnsureContentCoverage(distributedEvents, quarter);

            return distributedEvents;
        }

        /// <summary>
        /// Applies distribution rules to balance event frequency and prevent overwhelming
        /// </summary>
        private List<NarrativeEvent> ApplyDistributionRules(List<NarrativeEvent> events, int quarter)
        {
            var act = StoryScript.GetNarrativeActForQuarter(quarter);
            var targetFrequency = GetTargetFrequencyForAct(act);

            // Check if we should have content this quarter based on frequency
            if (random.NextDouble() > targetFrequency && events.Count == 0)
            {
                // No content this quarter - check if this creates a gap
                if (GetQuartersSinceLastContent(quarter) >= MAX_QUARTERS_WITHOUT_CONTENT)
                {
                    // Force generate at least one event to prevent gaps
                    events = GenerateFallbackContent(quarter);
                }
            }

            // Limit events per quarter
            if (events.Count > MAX_EVENTS_PER_QUARTER)
            {
                events = PrioritizeAndLimitEvents(events, quarter);
            }

            // Check for proper spacing of major events
            events = EnforceMajorEventSpacing(events, quarter);

            return events;
        }

        /// <summary>
        /// Ensures content coverage across the timeline and fills gaps
        /// </summary>
        private List<NarrativeEvent> EnsureContentCoverage(List<NarrativeEvent> events, int quarter)
        {
            // Check if we're in a content gap
            var quartersSinceLastContent = GetQuartersSinceLastContent(quarter);
            
            if (quartersSinceLastContent >= MAX_QUARTERS_WITHOUT_CONTENT && events.Count == 0)
            {
                // Generate fallback content to fill the gap
                events = GenerateFallbackContent(quarter);
            }

            // Validate content type distribution over recent quarters
            ValidateContentTypeDistribution(quarter);

            return events;
        }

        /// <summary>
        /// Gets the target event frequency for a narrative act
        /// </summary>
        private double GetTargetFrequencyForAct(NarrativeAct act)
        {
            return act switch
            {
                NarrativeAct.Tutorial => TUTORIAL_EVENT_FREQUENCY,
                NarrativeAct.RisingAction => RISING_ACTION_FREQUENCY,
                NarrativeAct.Climax => CLIMAX_FREQUENCY,
                NarrativeAct.Resolution => RESOLUTION_FREQUENCY,
                _ => 0.5
            };
        }

        /// <summary>
        /// Calculates how many quarters have passed since the last story content
        /// </summary>
        private int GetQuartersSinceLastContent(int currentQuarter)
        {
            // Look back through completed story events
            var recentEvents = storyData.CompletedStoryEvents
                .Where(e => e.Contains("_Q"))
                .Select(e => ExtractQuarterFromEventId(e))
                .Where(q => q > 0 && q < currentQuarter)
                .OrderByDescending(q => q)
                .FirstOrDefault();

            if (recentEvents == 0)
            {
                // No recent events found, check emotional beats
                var lastBeat = storyData.EmotionalBeats
                    .Where(b => b.Quarter < currentQuarter)
                    .OrderByDescending(b => b.Quarter)
                    .FirstOrDefault();

                if (lastBeat != null)
                {
                    return currentQuarter - lastBeat.Quarter;
                }

                // Default to 0 if no history
                return 0;
            }

            return currentQuarter - recentEvents;
        }

        /// <summary>
        /// Extracts quarter number from event ID
        /// </summary>
        private int ExtractQuarterFromEventId(string eventId)
        {
            var parts = eventId.Split('_');
            foreach (var part in parts)
            {
                if (part.StartsWith("Q") && int.TryParse(part.Substring(1), out int quarter))
                {
                    return quarter;
                }
            }
            return 0;
        }

        /// <summary>
        /// Generates fallback content when gaps are detected
        /// </summary>
        private List<NarrativeEvent> GenerateFallbackContent(int quarter)
        {
            var events = new List<NarrativeEvent>();

            // Determine what type of fallback content to generate
            var contentType = DetermineFallbackContentType(quarter);

            switch (contentType)
            {
                case "character_interaction":
                    events.Add(GenerateCharacterCheckIn(quarter));
                    break;

                case "business_update":
                    events.Add(GenerateBusinessUpdateEvent(quarter));
                    break;

                case "milestone_reflection":
                    events.Add(GenerateMilestoneReflection(quarter));
                    break;

                default:
                    events.Add(GenerateGenericJoanDialogue(quarter));
                    break;
            }

            return events;
        }

        /// <summary>
        /// Determines the best type of fallback content based on recent history
        /// </summary>
        private string DetermineFallbackContentType(int quarter)
        {
            var recentContentTypes = GetRecentContentTypes(quarter, 10);

            // Balance content types
            if (recentContentTypes.Count(t => t == "character") < 3)
                return "character_interaction";

            if (recentContentTypes.Count(t => t == "business") < 3)
                return "business_update";

            if (quarter % 10 == 0)
                return "milestone_reflection";

            return "generic_dialogue";
        }

        /// <summary>
        /// Gets recent content types for distribution analysis
        /// </summary>
        private List<string> GetRecentContentTypes(int currentQuarter, int lookbackQuarters)
        {
            var contentTypes = new List<string>();

            var recentBeats = storyData.EmotionalBeats
                .Where(b => b.Quarter >= currentQuarter - lookbackQuarters && b.Quarter < currentQuarter)
                .ToList();

            foreach (var beat in recentBeats)
            {
                // Categorize based on event ID patterns
                if (beat.EventId.Contains("character") || beat.EventId.Contains("relationship"))
                    contentTypes.Add("character");
                else if (beat.EventId.Contains("business") || beat.EventId.Contains("conflict"))
                    contentTypes.Add("business");
                else if (beat.EventId.Contains("milestone"))
                    contentTypes.Add("milestone");
                else
                    contentTypes.Add("other");
            }

            return contentTypes;
        }

        /// <summary>
        /// Generates a character check-in event for fallback content
        /// </summary>
        private NarrativeEvent GenerateCharacterCheckIn(int quarter)
        {
            // Find an available character to check in with
            var availableCharacters = StoryScript.Characters.Values
                .Where(c => quarter >= c.IntroductionQuarter && c.CharacterId != "joan")
                .ToList();

            var character = availableCharacters.Any() 
                ? availableCharacters[random.Next(availableCharacters.Count)]
                : StoryScript.Characters["joan"];

            return new NarrativeEvent
            {
                EventId = $"checkin_{character.CharacterId}_Q{quarter}",
                EventType = NarrativeEventType.CharacterIntroduction,
                TriggerQuarter = quarter,
                InvolvedCharacters = new List<string> { character.CharacterId },
                Title = $"Catching Up with {character.Name}",
                Description = $"{character.Name} stops by to discuss recent developments.",
                Dialogue = new List<string>
                {
                    $"I wanted to touch base about how things are progressing.",
                    "There are a few matters worth discussing.",
                    "How would you like to proceed?"
                },
                Choices = CreateGenericCheckInChoices(character.CharacterId),
                GameplayEffects = new Dictionary<string, object>
                {
                    ["fallback_content"] = true,
                    ["character_interaction"] = character.CharacterId
                }
            };
        }

        /// <summary>
        /// Generates a business update event for fallback content
        /// </summary>
        private NarrativeEvent GenerateBusinessUpdateEvent(int quarter)
        {
            return new NarrativeEvent
            {
                EventId = $"business_update_Q{quarter}",
                EventType = NarrativeEventType.EmotionalBeat,
                TriggerQuarter = quarter,
                InvolvedCharacters = new List<string> { "joan" },
                Title = "Business Performance Review",
                Description = "Joan provides an update on the company's current position.",
                Dialogue = new List<string>
                {
                    $"We're now in Quarter {quarter} of your leadership journey.",
                    GetBusinessPerformanceComment(),
                    "Let's discuss our strategic priorities moving forward."
                },
                Choices = CreateBusinessUpdateChoices(),
                GameplayEffects = new Dictionary<string, object>
                {
                    ["fallback_content"] = true,
                    ["business_update"] = true
                }
            };
        }

        /// <summary>
        /// Generates a milestone reflection event for fallback content
        /// </summary>
        private NarrativeEvent GenerateMilestoneReflection(int quarter)
        {
            var years = quarter / 4;
            
            return new NarrativeEvent
            {
                EventId = $"reflection_Q{quarter}",
                EventType = NarrativeEventType.EmotionalBeat,
                TriggerQuarter = quarter,
                InvolvedCharacters = new List<string> { "joan" },
                Title = $"Reflecting on {years} Years",
                Description = $"A moment to reflect on {years} years of corporate leadership.",
                Dialogue = new List<string>
                {
                    $"It's been {years} years since you took over this company.",
                    "Looking back, we've accomplished quite a lot together.",
                    "What aspects of our journey stand out most to you?"
                },
                Choices = CreateReflectionChoices(),
                GameplayEffects = new Dictionary<string, object>
                {
                    ["fallback_content"] = true,
                    ["milestone_reflection"] = true,
                    ["years"] = years
                }
            };
        }

        /// <summary>
        /// Generates generic Joan dialogue for fallback content
        /// </summary>
        private NarrativeEvent GenerateGenericJoanDialogue(int quarter)
        {
            return new NarrativeEvent
            {
                EventId = $"joan_dialogue_Q{quarter}",
                EventType = NarrativeEventType.EmotionalBeat,
                TriggerQuarter = quarter,
                InvolvedCharacters = new List<string> { "joan" },
                Title = "Joan's Insights",
                Description = "Joan shares her thoughts on the current situation.",
                Dialogue = new List<string>
                {
                    "I've been reviewing our recent progress.",
                    GetJoanContextualComment(quarter),
                    "Your leadership continues to shape our company's direction."
                },
                Choices = CreateGenericDialogueChoices(),
                GameplayEffects = new Dictionary<string, object>
                {
                    ["fallback_content"] = true,
                    ["generic_dialogue"] = true
                }
            };
        }

        /// <summary>
        /// Gets a contextual comment from Joan based on company performance
        /// </summary>
        private string GetJoanContextualComment(int quarter)
        {
            if (company.MarketShare > 50)
                return "Our market position is remarkably strong.";
            
            if (company.Capital > 500000000)
                return "The financial growth has been impressive.";
            
            if (company.Morale > 70)
                return "The team's morale and dedication are outstanding.";
            
            if (company.ConsecutiveNegativeQuarters > 0)
                return "We're facing some challenges, but I believe we can overcome them.";
            
            return "We're making steady progress toward our goals.";
        }

        /// <summary>
        /// Gets a business performance comment based on current metrics
        /// </summary>
        private string GetBusinessPerformanceComment()
        {
            if (company.MarketShare > 40)
                return $"Our {company.MarketShare:F1}% market share puts us in a strong competitive position.";
            
            if (company.Capital > 750000000)
                return $"With ${company.Capital / 1000000:F0}M in capital, we have significant resources.";
            
            if (company.EmployeeCount > 40)
                return $"Our team of {company.EmployeeCount} employees is our greatest asset.";
            
            return "The company continues to evolve and adapt to market conditions.";
        }

        /// <summary>
        /// Prioritizes and limits events to prevent overwhelming the player
        /// </summary>
        private List<NarrativeEvent> PrioritizeAndLimitEvents(List<NarrativeEvent> events, int quarter)
        {
            // Sort by priority (act transitions > character introductions > others)
            var prioritized = events.OrderByDescending(e => GetEventPriority(e)).ToList();

            // Take only the top events up to the maximum
            var limited = prioritized.Take(MAX_EVENTS_PER_QUARTER).ToList();

            return limited;
        }

        /// <summary>
        /// Gets the priority value for an event type
        /// </summary>
        private int GetEventPriority(NarrativeEvent evt)
        {
            return evt.EventType switch
            {
                NarrativeEventType.ActTransition => 100,
                NarrativeEventType.CharacterIntroduction => 90,
                NarrativeEventType.EmotionalBeat => 80,
                NarrativeEventType.BusinessConflict => 70,
                NarrativeEventType.RelationshipMilestone => 60,
                NarrativeEventType.PersonalChallenge => 50,
                NarrativeEventType.ChoiceConsequence => 40,
                _ => 30
            };
        }

        /// <summary>
        /// Enforces spacing between major events
        /// </summary>
        private List<NarrativeEvent> EnforceMajorEventSpacing(List<NarrativeEvent> events, int quarter)
        {
            // Check if there was a major event recently
            var recentMajorEvent = storyData.EmotionalBeats
                .Where(b => b.Quarter >= quarter - MIN_QUARTERS_BETWEEN_MAJOR_EVENTS && 
                           b.Quarter < quarter &&
                           b.Intensity >= 0.7)
                .Any();

            if (recentMajorEvent)
            {
                // Filter out high-intensity events
                events = events.Where(e => !IsMajorEvent(e)).ToList();
            }

            return events;
        }

        /// <summary>
        /// Determines if an event is considered major
        /// </summary>
        private bool IsMajorEvent(NarrativeEvent evt)
        {
            return evt.EventType == NarrativeEventType.ActTransition ||
                   evt.EventType == NarrativeEventType.CharacterIntroduction ||
                   (evt.EventType == NarrativeEventType.EmotionalBeat && 
                    evt.GameplayEffects.ContainsKey("milestone_type"));
        }

        /// <summary>
        /// Validates content type distribution over recent quarters
        /// </summary>
        private void ValidateContentTypeDistribution(int quarter)
        {
            var recentContentTypes = GetRecentContentTypes(quarter, 20);
            
            if (recentContentTypes.Count < 5)
                return; // Not enough data to validate

            var characterRatio = recentContentTypes.Count(t => t == "character") / (double)recentContentTypes.Count;
            var businessRatio = recentContentTypes.Count(t => t == "business") / (double)recentContentTypes.Count;
            var milestoneRatio = recentContentTypes.Count(t => t == "milestone") / (double)recentContentTypes.Count;

            // Log warnings if distribution is significantly off target
            if (Math.Abs(characterRatio - CHARACTER_CONTENT_RATIO) > 0.2)
            {
                System.Diagnostics.Debug.WriteLine($"Q{quarter}: Character content ratio {characterRatio:P0} deviates from target {CHARACTER_CONTENT_RATIO:P0}");
            }

            if (Math.Abs(businessRatio - BUSINESS_CONTENT_RATIO) > 0.2)
            {
                System.Diagnostics.Debug.WriteLine($"Q{quarter}: Business content ratio {businessRatio:P0} deviates from target {BUSINESS_CONTENT_RATIO:P0}");
            }
        }

        /// <summary>
        /// Gets a summary of content distribution across the timeline
        /// </summary>
        public ContentDistributionSummary GetDistributionSummary(int currentQuarter)
        {
            var summary = new ContentDistributionSummary
            {
                CurrentQuarter = currentQuarter,
                TotalEventsGenerated = storyData.EmotionalBeats.Count,
                QuartersWithContent = storyData.EmotionalBeats.Select(b => b.Quarter).Distinct().Count(),
                QuartersWithoutContent = currentQuarter - storyData.EmotionalBeats.Select(b => b.Quarter).Distinct().Count(),
                AverageEventsPerQuarter = currentQuarter > 0 ? storyData.EmotionalBeats.Count / (double)currentQuarter : 0,
                LongestGap = CalculateLongestGap(currentQuarter),
                ContentTypeDistribution = CalculateContentTypeDistribution(currentQuarter)
            };

            return summary;
        }

        /// <summary>
        /// Calculates the longest gap between story content
        /// </summary>
        private int CalculateLongestGap(int currentQuarter)
        {
            var quartersWithContent = storyData.EmotionalBeats
                .Select(b => b.Quarter)
                .Distinct()
                .OrderBy(q => q)
                .ToList();

            if (quartersWithContent.Count < 2)
                return 0;

            int longestGap = 0;
            for (int i = 1; i < quartersWithContent.Count; i++)
            {
                int gap = quartersWithContent[i] - quartersWithContent[i - 1] - 1;
                longestGap = Math.Max(longestGap, gap);
            }

            return longestGap;
        }

        /// <summary>
        /// Calculates content type distribution
        /// </summary>
        private Dictionary<string, double> CalculateContentTypeDistribution(int currentQuarter)
        {
            var contentTypes = GetRecentContentTypes(currentQuarter, currentQuarter);
            var total = contentTypes.Count;

            if (total == 0)
                return new Dictionary<string, double>();

            return new Dictionary<string, double>
            {
                ["character"] = contentTypes.Count(t => t == "character") / (double)total,
                ["business"] = contentTypes.Count(t => t == "business") / (double)total,
                ["milestone"] = contentTypes.Count(t => t == "milestone") / (double)total,
                ["other"] = contentTypes.Count(t => t == "other") / (double)total
            };
        }

        #region Choice Creation Helper Methods

        private List<DialogueChoice> CreateGenericCheckInChoices(string characterId)
        {
            return new List<DialogueChoice>
            {
                new DialogueChoice
                {
                    ChoiceId = "checkin_discuss",
                    ChoiceText = "Let's discuss the current situation in detail.",
                    Tone = ChoiceTone.Professional,
                    RelationshipImpact = new RelationshipImpact
                    {
                        PrimaryCharacter = characterId,
                        RespectChange = 2,
                        TrustChange = 1
                    }
                },
                new DialogueChoice
                {
                    ChoiceId = "checkin_brief",
                    ChoiceText = "Give me the highlights - I trust your judgment.",
                    Tone = ChoiceTone.Supportive,
                    RelationshipImpact = new RelationshipImpact
                    {
                        PrimaryCharacter = characterId,
                        TrustChange = 3
                    }
                }
            };
        }

        private List<DialogueChoice> CreateBusinessUpdateChoices()
        {
            return new List<DialogueChoice>
            {
                new DialogueChoice
                {
                    ChoiceId = "update_strategic",
                    ChoiceText = "Let's focus on strategic priorities for the next phase.",
                    Tone = ChoiceTone.Professional
                },
                new DialogueChoice
                {
                    ChoiceId = "update_operational",
                    ChoiceText = "What operational improvements should we prioritize?",
                    Tone = ChoiceTone.Professional
                }
            };
        }

        private List<DialogueChoice> CreateReflectionChoices()
        {
            return new List<DialogueChoice>
            {
                new DialogueChoice
                {
                    ChoiceId = "reflection_achievements",
                    ChoiceText = "I'm proud of what we've built together.",
                    Tone = ChoiceTone.Personal,
                    RelationshipImpact = new RelationshipImpact
                    {
                        PrimaryCharacter = "joan",
                        PersonalConnectionChange = 4,
                        TrustChange = 2
                    }
                },
                new DialogueChoice
                {
                    ChoiceId = "reflection_future",
                    ChoiceText = "The best is yet to come. Let's keep pushing forward.",
                    Tone = ChoiceTone.Professional,
                    RelationshipImpact = new RelationshipImpact
                    {
                        PrimaryCharacter = "joan",
                        RespectChange = 3
                    }
                }
            };
        }

        private List<DialogueChoice> CreateGenericDialogueChoices()
        {
            return new List<DialogueChoice>
            {
                new DialogueChoice
                {
                    ChoiceId = "generic_acknowledge",
                    ChoiceText = "Thank you for the update, Joan.",
                    Tone = ChoiceTone.Professional
                },
                new DialogueChoice
                {
                    ChoiceId = "generic_discuss",
                    ChoiceText = "Let's discuss this further.",
                    Tone = ChoiceTone.Professional
                }
            };
        }

        #endregion
    }

    /// <summary>
    /// Summary of content distribution across the story timeline
    /// </summary>
    public class ContentDistributionSummary
    {
        public int CurrentQuarter { get; set; }
        public int TotalEventsGenerated { get; set; }
        public int QuartersWithContent { get; set; }
        public int QuartersWithoutContent { get; set; }
        public double AverageEventsPerQuarter { get; set; }
        public int LongestGap { get; set; }
        public Dictionary<string, double> ContentTypeDistribution { get; set; } = new Dictionary<string, double>();
    }
}
