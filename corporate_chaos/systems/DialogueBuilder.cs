using CorporateChaos.Models;

namespace CorporateChaos.Systems
{
    /// <summary>
    /// Helper class for building enhanced dialogue nodes with multiple response options,
    /// tone indicators, and consequence tracking.
    /// </summary>
    public static class DialogueBuilder
    {
        /// <summary>
        /// Creates a sample branching dialogue node demonstrating the enhanced features.
        /// </summary>
        public static DialogueNode CreateSampleBranchingDialogue(string characterId, string context)
        {
            var node = new DialogueNode
            {
                NodeId = $"{characterId}_{context}_branch",
                CharacterId = characterId,
                DialogueText = GetContextualDialogue(characterId, context),
                EmotionalTone = EmotionalTone.Serious,
                MinimumChoices = 2,
                MaximumChoices = 4,
                ContextTags = new List<string> { context, "branching", "consequential" }
            };

            // Add adaptive text based on relationship levels
            node.AdaptiveText["relationship:joan:trust:50"] = "I trust you enough to be completely honest about this situation.";
            node.AdaptiveText["flag:first_meeting"] = "Since this is our first real conversation, let me be direct.";

            // Create multiple response options with different tones and consequences
            node.Choices = CreateResponseOptions(characterId, context);

            return node;
        }

        private static string GetContextualDialogue(string characterId, string context)
        {
            return characterId switch
            {
                "joan" => context switch
                {
                    "crisis_management" => "We're facing a significant challenge that requires careful consideration. How would you like to approach this?",
                    "performance_review" => "I've been analyzing our quarterly performance. There are some important decisions to make.",
                    "employee_concerns" => "Several employees have raised concerns that need your attention. What's your leadership approach here?",
                    _ => "I need to discuss something important with you. How would you like to handle this situation?"
                },
                "marcus_vey" => "The numbers don't lie - we need to make a strategic decision here. What's your risk tolerance?",
                "evelyn_cross" => "This decision will significantly impact our team. I want to make sure we consider all perspectives.",
                _ => "We need to discuss our next steps. What's your preferred approach?"
            };
        }

        private static List<DialogueChoice> CreateResponseOptions(string characterId, string context)
        {
            var choices = new List<DialogueChoice>();

            // Professional/Conservative Option
            choices.Add(new DialogueChoice
            {
                ChoiceId = "professional_approach",
                ChoiceText = "Let's analyze this systematically and follow established protocols.",
                Tone = ChoiceTone.Professional,
                ToneDescription = "Methodical and by-the-book approach",
                RiskLevel = ConsequenceRisk.Low,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = characterId,
                    RespectChange = 5,
                    TrustChange = 2,
                    ImpactDescription = "Demonstrates reliability and professionalism"
                },
                ImmediateConsequences = new List<ConsequencePreview>
                {
                    new ConsequencePreview
                    {
                        Description = "Maintains stability and reduces immediate risk",
                        Type = ConsequenceType.Business,
                        Severity = ConsequenceRisk.Low
                    }
                },
                ConsequenceFlags = new List<string> { "conservative_approach", "protocol_followed" }
            });

            // Supportive/Empathetic Option
            choices.Add(new DialogueChoice
            {
                ChoiceId = "supportive_approach",
                ChoiceText = "I want to make sure everyone feels heard and supported through this.",
                Tone = ChoiceTone.Supportive,
                ToneDescription = "Empathetic and team-focused",
                RiskLevel = ConsequenceRisk.Low,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = characterId,
                    PersonalConnectionChange = 8,
                    TrustChange = 5,
                    ImpactDescription = "Builds strong personal rapport and team loyalty",
                    SecondaryEffects = new Dictionary<string, int>
                    {
                        ["evelyn_cross"] = 10, // HR appreciates employee focus
                        ["all_employees"] = 5   // General morale boost
                    }
                },
                ImmediateConsequences = new List<ConsequencePreview>
                {
                    new ConsequencePreview
                    {
                        Description = "Improves team morale and employee satisfaction",
                        Type = ConsequenceType.Relationship,
                        Severity = ConsequenceRisk.Low,
                        AffectedCharacters = new List<string> { "evelyn_cross", "all_employees" }
                    }
                },
                ConsequenceFlags = new List<string> { "supportive_leadership", "employee_focused" }
            });

            // Aggressive/Decisive Option
            choices.Add(new DialogueChoice
            {
                ChoiceId = "aggressive_approach",
                ChoiceText = "We need to act decisively and take control of this situation immediately.",
                Tone = ChoiceTone.Aggressive,
                ToneDescription = "Bold and action-oriented",
                RiskLevel = ConsequenceRisk.High,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = characterId,
                    RespectChange = characterId == "marcus_vey" ? 10 : -5, // Marcus likes aggression, others may not
                    TrustChange = -2,
                    ImpactDescription = characterId == "marcus_vey" 
                        ? "Demonstrates strong leadership and decisiveness" 
                        : "May seem too hasty or inconsiderate"
                },
                ImmediateConsequences = new List<ConsequencePreview>
                {
                    new ConsequencePreview
                    {
                        Description = "Quick resolution but potential for unintended consequences",
                        Type = ConsequenceType.Business,
                        Severity = ConsequenceRisk.Medium
                    }
                },
                LongTermConsequences = new List<ConsequencePreview>
                {
                    new ConsequencePreview
                    {
                        Description = "May create resistance or resentment among team members",
                        Type = ConsequenceType.Relationship,
                        Severity = ConsequenceRisk.Medium,
                        TriggerQuarter = null // Ongoing effect
                    }
                },
                ConsequenceFlags = new List<string> { "aggressive_leadership", "quick_decision" },
                RequiresConditions = new List<string> { "relationship:joan:trust:30" } // Requires some trust
            });

            // Diplomatic/Collaborative Option
            choices.Add(new DialogueChoice
            {
                ChoiceId = "diplomatic_approach",
                ChoiceText = "Let's bring together the key stakeholders and find a solution that works for everyone.",
                Tone = ChoiceTone.Diplomatic,
                ToneDescription = "Collaborative and inclusive",
                RiskLevel = ConsequenceRisk.Medium,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = characterId,
                    RespectChange = 7,
                    PersonalConnectionChange = 3,
                    ImpactDescription = "Shows wisdom and inclusive leadership style",
                    SecondaryEffects = new Dictionary<string, int>
                    {
                        ["evelyn_cross"] = 8,  // HR loves collaboration
                        ["harold_finch"] = 5   // Legal appreciates careful approach
                    }
                },
                ImmediateConsequences = new List<ConsequencePreview>
                {
                    new ConsequencePreview
                    {
                        Description = "Takes more time but builds consensus and buy-in",
                        Type = ConsequenceType.Story,
                        Severity = ConsequenceRisk.Low
                    }
                },
                ConsequenceFlags = new List<string> { "collaborative_approach", "stakeholder_engagement" },
                UnlocksFutureOptions = new List<string> { "team_leadership_path", "consensus_builder" }
            });

            return choices;
        }

        /// <summary>
        /// Creates a dialogue choice with comprehensive consequence tracking.
        /// </summary>
        public static DialogueChoice CreateChoiceWithConsequences(
            string choiceId,
            string choiceText,
            ChoiceTone tone,
            string primaryCharacter,
            int relationshipImpact,
            ConsequenceRisk riskLevel = ConsequenceRisk.Low)
        {
            var choice = new DialogueChoice
            {
                ChoiceId = choiceId,
                ChoiceText = choiceText,
                Tone = tone,
                ToneDescription = GetToneDescription(tone),
                RiskLevel = riskLevel,
                RelationshipImpact = new RelationshipImpact
                {
                    PrimaryCharacter = primaryCharacter,
                    TrustChange = relationshipImpact,
                    ImpactDescription = $"Affects relationship with {primaryCharacter}"
                }
            };

            // Add relationship changes to the legacy format for compatibility
            choice.RelationshipChanges[primaryCharacter] = relationshipImpact;

            return choice;
        }

        private static string GetToneDescription(ChoiceTone tone)
        {
            return tone switch
            {
                ChoiceTone.Professional => "Business-focused and formal",
                ChoiceTone.Supportive => "Caring and empathetic",
                ChoiceTone.Aggressive => "Direct and forceful",
                ChoiceTone.Diplomatic => "Tactful and collaborative",
                ChoiceTone.Personal => "Intimate and heartfelt",
                ChoiceTone.Humorous => "Light-hearted and playful",
                _ => "Neutral approach"
            };
        }

        /// <summary>
        /// Demonstrates how to use the enhanced dialogue system with multiple characters.
        /// </summary>
        public static Dictionary<string, DialogueNode> CreateMultiCharacterDialogueExample()
        {
            var dialogues = new Dictionary<string, DialogueNode>();

            // Joan dialogue with relationship-aware responses
            var joanNode = CreateSampleBranchingDialogue("joan", "quarterly_review");
            joanNode.AdaptiveText["relationship:joan:personal:60"] = "You know, after working together for so long, I feel comfortable being completely honest with you about our situation.";
            dialogues["joan_quarterly"] = joanNode;

            // Marcus Vey dialogue focused on financial decisions
            var marcusNode = CreateSampleBranchingDialogue("marcus_vey", "investment_opportunity");
            marcusNode.DialogueText = "I've identified a high-risk, high-reward investment opportunity. The potential returns are substantial, but so are the risks.";
            marcusNode.EmotionalTone = EmotionalTone.Excited;
            dialogues["marcus_investment"] = marcusNode;

            // Evelyn Cross dialogue about employee concerns
            var evelynNode = CreateSampleBranchingDialogue("evelyn_cross", "employee_welfare");
            evelynNode.DialogueText = "I'm concerned about the impact our recent decisions are having on team morale. We need to address this carefully.";
            evelynNode.EmotionalTone = EmotionalTone.Concerned;
            dialogues["evelyn_welfare"] = evelynNode;

            return dialogues;
        }
    }
}