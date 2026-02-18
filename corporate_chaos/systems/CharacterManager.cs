using CorporateChaos.Models;
using System.Text.Json;

namespace CorporateChaos.Systems
{
    public class CharacterManager
    {
        private ExtendedStoryModeData storyData;
        private Company company;

        public CharacterManager(ExtendedStoryModeData storyData, Company company)
        {
            this.storyData = storyData;
            this.company = company;
            InitializeCharacterRelationships();
        }

        private void InitializeCharacterRelationships()
        {
            // Initialize relationships for all characters if they don't exist
            foreach (var character in StoryScript.Characters.Values)
            {
                if (!storyData.CharacterRelationships.ContainsKey(character.CharacterId))
                {
                    storyData.CharacterRelationships[character.CharacterId] = new CharacterRelationship
                    {
                        TrustLevel = 0,
                        ProfessionalRespect = 0,
                        PersonalConnection = 0,
                        CurrentPhase = RelationshipPhase.FirstMeeting
                    };
                }

                if (!storyData.CharacterArcs.ContainsKey(character.CharacterId))
                {
                    storyData.CharacterArcs[character.CharacterId] = new CharacterArcState
                    {
                        CharacterId = character.CharacterId,
                        CurrentPhase = CharacterArcPhase.Introduction,
                        NextMilestoneQuarter = character.IntroductionQuarter
                    };
                }
            }
        }

        public void UpdateCharacterRelationship(string characterId, int trustChange, int respectChange, int connectionChange, string experience = "")
        {
            if (!storyData.CharacterRelationships.ContainsKey(characterId))
                return;

            var relationship = storyData.CharacterRelationships[characterId];
            
            // Apply changes with bounds checking
            relationship.TrustLevel = Math.Max(-100, Math.Min(100, relationship.TrustLevel + trustChange));
            relationship.ProfessionalRespect = Math.Max(-100, Math.Min(100, relationship.ProfessionalRespect + respectChange));
            relationship.PersonalConnection = Math.Max(-100, Math.Min(100, relationship.PersonalConnection + connectionChange));

            // Add experience if provided
            if (!string.IsNullOrEmpty(experience))
            {
                relationship.SharedExperiences.Add($"Q{storyData.CurrentQuarter}: {experience}");
            }

            // Update relationship phase based on overall relationship strength
            UpdateRelationshipPhase(characterId);
        }

        private void UpdateRelationshipPhase(string characterId)
        {
            var relationship = storyData.CharacterRelationships[characterId];
            var averageRelationship = (relationship.TrustLevel + relationship.ProfessionalRespect + relationship.PersonalConnection) / 3;

            // Special handling for Joan based on quarter progression
            if (characterId == "joan")
            {
                var joanPhase = StoryScript.GetJoanPhaseForQuarter(storyData.CurrentQuarter);
                relationship.CurrentPhase = joanPhase;
                return;
            }

            // General relationship phase progression for other characters
            relationship.CurrentPhase = averageRelationship switch
            {
                < -50 => RelationshipPhase.Hostile,
                < -20 => RelationshipPhase.Strained,
                < 20 => RelationshipPhase.ProfessionalAcquaintance,
                < 50 => RelationshipPhase.TrustedColleague,
                < 80 => RelationshipPhase.PersonalFriend,
                _ => RelationshipPhase.LifelongBond
            };
        }

        public void ProcessBusinessDecisionImpact(string decisionType, Dictionary<string, object> decisionData)
        {
            // Update character relationships based on business decisions
            switch (decisionType)
            {
                case "employee_bonus":
                    UpdateCharacterRelationship("evelyn_cross", 5, 3, 2, "Approved employee bonuses");
                    UpdateCharacterRelationship("marcus_vey", -2, 0, 0, "Concerned about bonus costs");
                    break;

                case "cost_cutting":
                    UpdateCharacterRelationship("marcus_vey", 3, 5, 1, "Supported cost reduction measures");
                    UpdateCharacterRelationship("evelyn_cross", -3, -2, -1, "Worried about employee impact");
                    break;

                case "marketing_campaign":
                    UpdateCharacterRelationship("lucinda_vale", 4, 3, 2, "Launched marketing campaign");
                    UpdateCharacterRelationship("harold_finch", -1, 0, 0, "Cautious about marketing risks");
                    break;

                case "employee_firing":
                    UpdateCharacterRelationship("evelyn_cross", -5, -3, -2, "Fired employees");
                    UpdateCharacterRelationship("marcus_vey", 2, 1, 0, "Supported workforce optimization");
                    break;

                case "high_risk_investment":
                    UpdateCharacterRelationship("marcus_vey", 5, 4, 2, "Pursued high-risk investment");
                    UpdateCharacterRelationship("harold_finch", -4, -2, -1, "Concerned about legal risks");
                    break;
            }
        }

        public List<string> GetCharacterAdvice(string characterId, Company company)
        {
            if (!StoryScript.Characters.ContainsKey(characterId))
                return new List<string>();

            var character = StoryScript.Characters[characterId];
            var relationship = storyData.CharacterRelationships[characterId];
            var advice = new List<string>();

            // Generate character-specific advice based on personality and company state
            switch (characterId)
            {
                case "marcus_vey":
                    if (company.Capital > 1000000)
                        advice.Add("💰 Marcus suggests: 'With this capital, we could pursue aggressive expansion or high-yield investments.'");
                    if (company.Risk < 20)
                        advice.Add("📈 Marcus suggests: 'We're playing it too safe. Higher risk could mean higher rewards.'");
                    break;

                case "evelyn_cross":
                    if (company.Morale < 30)
                        advice.Add("😟 Evelyn warns: 'Employee morale is critically low. We need immediate action to prevent turnover.'");
                    if (company.EmployeeCount < 5)
                        advice.Add("👥 Evelyn suggests: 'We're understaffed. Consider hiring to improve productivity and reduce burnout.'");
                    break;

                case "vincent_duro":
                    if (company.MarketShare > 40)
                        advice.Add("🏢 Vincent challenges: 'Impressive market share, but can you maintain it against real competition?'");
                    break;

                case "lucinda_vale":
                    if (company.Reputation < 20)
                        advice.Add("📢 Lucy suggests: 'Our public image needs work. A strategic PR campaign could transform our reputation.'");
                    break;

                case "gregory_shaw":
                    if (company.Risk > 60)
                        advice.Add("⚙️ Greg warns: 'Operations are becoming unstable. We need to focus on efficiency and risk reduction.'");
                    if (company.EmployeeCount > 15)
                        advice.Add("📊 Greg suggests: 'With this workforce size, we need better operational systems and processes.'");
                    break;

                case "selena_park":
                    if (company.Capital > 750000000)
                        advice.Add("💼 Selena hints: 'Companies with your financial profile often attract acquisition interest from major conglomerates...'");
                    if (company.MarketShare > 50)
                        advice.Add("📈 Selena suggests: 'Strong market position creates excellent opportunities for strategic partnerships.'");
                    break;

                case "harold_finch":
                    if (company.Risk > 70)
                        advice.Add("⚖️ Harold warns: 'Current risk levels expose us to potential legal and regulatory issues.'");
                    if (company.ConsecutiveNegativeQuarters > 0)
                        advice.Add("📋 Harold advises: 'Financial distress increases legal vulnerabilities. We need careful crisis management.'");
                    break;

                case "sophie_kim":
                    var efficiency = Math.Max(50, 100 - company.Risk);
                    advice.Add($"📊 Sophie reports: 'Data shows our efficiency is at {efficiency}%. I found some optimization opportunities!'");
                    if (company.MarketShare > 30)
                        advice.Add("📈 Sophie suggests: 'Our market share growth pattern suggests we could capture even more with targeted strategies!'");
                    break;
            }

            return advice;
        }

        public bool ShouldIntroduceCharacter(string characterId, int currentQuarter)
        {
            if (!StoryScript.Characters.ContainsKey(characterId))
                return false;

            var character = StoryScript.Characters[characterId];
            var arcState = storyData.CharacterArcs[characterId];

            return currentQuarter >= character.IntroductionQuarter && 
                   arcState.CurrentPhase == CharacterArcPhase.Introduction;
        }

        public void IntroduceCharacter(string characterId)
        {
            if (!storyData.CharacterArcs.ContainsKey(characterId))
                return;

            var arcState = storyData.CharacterArcs[characterId];
            arcState.CurrentPhase = CharacterArcPhase.Development;
            arcState.CompletedMilestones.Add("character_introduction");
        }

        public Dictionary<EndingType, double> CalculateEndingProbabilities()
        {
            var probabilities = new Dictionary<EndingType, double>();

            // Market Dominance ending
            probabilities[EndingType.MarketDominance] = company.MarketShare > 65 ? 0.8 : 0.0;

            // Conglomerate Buyout ending
            probabilities[EndingType.ConglomerateBuyout] = company.Capital > 1000000000 ? 0.7 : 0.0;

            // Bankruptcy ending
            probabilities[EndingType.BankruptcyFailure] = company.ConsecutiveNegativeQuarters >= 1 ? 0.9 : 0.0;

            // Lost Manpower ending
            probabilities[EndingType.LostManpowerFailure] = company.EmployeeCount <= 1 ? 1.0 : 0.0;

            // Graceful Retirement ending (default if no other conditions met)
            if (probabilities.Values.All(p => p < 0.1))
                probabilities[EndingType.GracefulRetirement] = 0.6;

            return probabilities;
        }
    }
}