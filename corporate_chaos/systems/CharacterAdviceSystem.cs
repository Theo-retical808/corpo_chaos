using CorporateChaos.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CorporateChaos.Systems
{
    /// <summary>
    /// Generates character-specific advice and manages gameplay effects when advice is followed
    /// Implements Requirements 13.3, 13.4, 13.6
    /// </summary>
    public class CharacterAdviceSystem
    {
        private readonly Company company;
        private readonly ExtendedStoryModeData storyData;
        private readonly EndingProbabilityTracker endingTracker;
        private readonly Random random;

        public CharacterAdviceSystem(Company company, ExtendedStoryModeData storyData, EndingProbabilityTracker endingTracker, Random random)
        {
            this.company = company;
            this.storyData = storyData;
            this.endingTracker = endingTracker;
            this.random = random;
        }

        /// <summary>
        /// Generates contextually appropriate advice for a character based on company state
        /// </summary>
        public CharacterAdvice? GenerateAdvice(string characterId, int quarter)
        {
            return characterId switch
            {
                "marcus_vey" => GenerateMarcusVeyAdvice(quarter),
                "evelyn_cross" => GenerateEvelynCrossAdvice(quarter),
                "vincent_duro" => GenerateVincentDuroAdvice(quarter),
                "lucinda_vale" => GenerateLucindaValeAdvice(quarter),
                "gregory_shaw" => GenerateGregoryShawAdvice(quarter),
                "selena_park" => GenerateSelenaParkAdvice(quarter),
                "harold_finch" => GenerateHaroldFinchAdvice(quarter),
                "sophie_kim" => GenerateSophieKimAdvice(quarter),
                _ => null
            };
        }

        /// <summary>
        /// Applies gameplay effects when player follows character advice
        /// </summary>
        public void ApplyAdviceEffect(CharacterAdvice advice, bool followed)
        {
            // Record the advice response
            endingTracker.RecordAdviceResponse(advice.CharacterId, advice.AdviceType, followed, advice.Quarter);

            if (!followed)
                return;

            // Apply character-specific effects
            switch (advice.CharacterId)
            {
                case "marcus_vey":
                    ApplyMarcusVeyEffect(advice);
                    break;
                case "evelyn_cross":
                    ApplyEvelynCrossEffect(advice);
                    break;
                case "vincent_duro":
                    ApplyVincentDuroEffect(advice);
                    break;
                case "lucinda_vale":
                    ApplyLucindaValeEffect(advice);
                    break;
                case "gregory_shaw":
                    ApplyGregoryShawEffect(advice);
                    break;
                case "selena_park":
                    ApplySelenaParkEffect(advice);
                    break;
                case "harold_finch":
                    ApplyHaroldFinchEffect(advice);
                    break;
                case "sophie_kim":
                    ApplySophieKimEffect(advice);
                    break;
            }
        }

        #region Marcus Vey - CFO (High-risk investments)

        private CharacterAdvice? GenerateMarcusVeyAdvice(int quarter)
        {
            // Aggressive investment advice when capital is high
            if (company.Capital > 500000 && company.Risk < 60)
            {
                return new CharacterAdvice
                {
                    CharacterId = "marcus_vey",
                    AdviceType = "aggressive_investment",
                    Quarter = quarter,
                    Title = "High-Risk Investment Opportunity",
                    Description = "Marcus suggests investing heavily in high-risk, high-reward ventures. This could accelerate growth toward $1B or lead to bankruptcy.",
                    Options = new List<AdviceOption>
                    {
                        new AdviceOption { Text = "Follow Marcus's aggressive strategy", IsFollowing = true },
                        new AdviceOption { Text = "Decline the risky investment", IsFollowing = false }
                    },
                    PotentialImpact = "Increases capital growth rate by 30% but also increases risk by 25 points"
                };
            }

            // Conservative advice during financial distress
            if (company.ConsecutiveNegativeQuarters > 0)
            {
                return new CharacterAdvice
                {
                    CharacterId = "marcus_vey",
                    AdviceType = "conservative_recovery",
                    Quarter = quarter,
                    Title = "Financial Recovery Strategy",
                    Description = "Marcus recommends conservative cost-cutting and revenue focus to stabilize finances.",
                    Options = new List<AdviceOption>
                    {
                        new AdviceOption { Text = "Implement Marcus's recovery plan", IsFollowing = true },
                        new AdviceOption { Text = "Pursue alternative strategy", IsFollowing = false }
                    },
                    PotentialImpact = "Reduces risk by 15 points and improves quarterly revenue by 10%"
                };
            }

            return null;
        }

        private void ApplyMarcusVeyEffect(CharacterAdvice advice)
        {
            switch (advice.AdviceType)
            {
                case "aggressive_investment":
                    // High risk, high reward
                    company.Risk = Math.Min(100, company.Risk + 25);
                    double investmentOutcome = random.NextDouble();
                    if (investmentOutcome > 0.4) // 60% success rate
                    {
                        company.Capital *= 1.3; // 30% capital boost
                        storyData.StoryFlags.Add($"marcus_investment_success_Q{advice.Quarter}");
                    }
                    else
                    {
                        company.Capital *= 0.85; // 15% capital loss
                        storyData.StoryFlags.Add($"marcus_investment_failure_Q{advice.Quarter}");
                    }
                    break;

                case "conservative_recovery":
                    company.Risk = Math.Max(0, company.Risk - 15);
                    company.QuarterlyRevenue *= 1.1;
                    break;
            }
        }

        #endregion

        #region Evelyn Cross - HR Head (Employee satisfaction)

        private CharacterAdvice? GenerateEvelynCrossAdvice(int quarter)
        {
            // Employee retention advice when morale is low
            if (company.Morale < 40)
            {
                return new CharacterAdvice
                {
                    CharacterId = "evelyn_cross",
                    AdviceType = "employee_retention",
                    Quarter = quarter,
                    Title = "Employee Retention Initiative",
                    Description = "Evelyn strongly recommends investing in employee satisfaction programs to prevent turnover and maintain productivity.",
                    Options = new List<AdviceOption>
                    {
                        new AdviceOption { Text = "Implement retention programs", IsFollowing = true },
                        new AdviceOption { Text = "Focus on other priorities", IsFollowing = false }
                    },
                    PotentialImpact = "Increases morale by 20 points and prevents employee loss"
                };
            }

            // Hiring advice when understaffed
            if (company.EmployeeCount < 8)
            {
                return new CharacterAdvice
                {
                    CharacterId = "evelyn_cross",
                    AdviceType = "strategic_hiring",
                    Quarter = quarter,
                    Title = "Strategic Hiring Recommendation",
                    Description = "Evelyn identifies critical staffing gaps that need immediate attention to maintain productivity.",
                    Options = new List<AdviceOption>
                    {
                        new AdviceOption { Text = "Follow Evelyn's hiring plan", IsFollowing = true },
                        new AdviceOption { Text = "Maintain current staffing", IsFollowing = false }
                    },
                    PotentialImpact = "Improves productivity by 15% and reduces operational risk"
                };
            }

            return null;
        }

        private void ApplyEvelynCrossEffect(CharacterAdvice advice)
        {
            switch (advice.AdviceType)
            {
                case "employee_retention":
                    company.Morale = Math.Min(100, company.Morale + 20);
                    // Prevent employee loss for next 2 quarters
                    storyData.StoryFlags.Add($"employee_retention_active_Q{advice.Quarter}");
                    storyData.StoryFlags.Add($"employee_retention_active_Q{advice.Quarter + 1}");
                    break;

                case "strategic_hiring":
                    // Boost productivity through better staffing
                    company.QuarterlyRevenue *= 1.15;
                    company.Risk = Math.Max(0, company.Risk - 10);
                    break;
            }
        }

        #endregion

        #region Vincent Duro - Rival CEO (Competitive responses)

        private CharacterAdvice? GenerateVincentDuroAdvice(int quarter)
        {
            // Competitive warning when market share is growing
            if (company.MarketShare > 35)
            {
                return new CharacterAdvice
                {
                    CharacterId = "vincent_duro",
                    AdviceType = "competitive_warning",
                    Quarter = quarter,
                    Title = "Competitive Threat Assessment",
                    Description = "Vincent warns that your market share growth has attracted aggressive competitive responses. Defensive strategies may be needed.",
                    Options = new List<AdviceOption>
                    {
                        new AdviceOption { Text = "Heed Vincent's warning and prepare defenses", IsFollowing = true },
                        new AdviceOption { Text = "Ignore the competitive threat", IsFollowing = false }
                    },
                    PotentialImpact = "Protects market share from competitive attacks"
                };
            }

            // Collaboration opportunity at high market share
            if (company.MarketShare > 55)
            {
                return new CharacterAdvice
                {
                    CharacterId = "vincent_duro",
                    AdviceType = "collaboration_offer",
                    Quarter = quarter,
                    Title = "Strategic Partnership Proposal",
                    Description = "Vincent suggests that collaboration might be more profitable than continued competition at this level.",
                    Options = new List<AdviceOption>
                    {
                        new AdviceOption { Text = "Accept Vincent's partnership offer", IsFollowing = true },
                        new AdviceOption { Text = "Continue competing independently", IsFollowing = false }
                    },
                    PotentialImpact = "Accelerates market share growth by 5% per quarter"
                };
            }

            return null;
        }

        private void ApplyVincentDuroEffect(CharacterAdvice advice)
        {
            switch (advice.AdviceType)
            {
                case "competitive_warning":
                    // Protect against market share loss
                    storyData.StoryFlags.Add($"competitive_defense_active_Q{advice.Quarter}");
                    storyData.StoryFlags.Add($"competitive_defense_active_Q{advice.Quarter + 1}");
                    break;

                case "collaboration_offer":
                    // Boost market share growth
                    company.MarketShare = Math.Min(100, company.MarketShare + 5);
                    storyData.StoryFlags.Add($"vincent_partnership_active_Q{advice.Quarter}");
                    break;
            }
        }

        #endregion

        #region Lucinda Vale - PR/Marketing Head (Market dominance)

        private CharacterAdvice? GenerateLucindaValeAdvice(int quarter)
        {
            // PR campaign for reputation boost
            if (company.Reputation < 50)
            {
                return new CharacterAdvice
                {
                    CharacterId = "lucinda_vale",
                    AdviceType = "pr_campaign",
                    Quarter = quarter,
                    Title = "Strategic PR Campaign",
                    Description = "Lucy proposes a bold PR campaign to transform public perception and boost market presence.",
                    Options = new List<AdviceOption>
                    {
                        new AdviceOption { Text = "Launch Lucy's PR campaign", IsFollowing = true },
                        new AdviceOption { Text = "Skip the PR investment", IsFollowing = false }
                    },
                    PotentialImpact = "Increases reputation by 25 points and market share by 3%"
                };
            }

            // Market dominance push
            if (company.MarketShare > 45 && company.MarketShare < 65)
            {
                return new CharacterAdvice
                {
                    CharacterId = "lucinda_vale",
                    AdviceType = "market_dominance_push",
                    Quarter = quarter,
                    Title = "Market Dominance Campaign",
                    Description = "Lucy sees an opportunity to push for market dominance with an aggressive marketing blitz.",
                    Options = new List<AdviceOption>
                    {
                        new AdviceOption { Text = "Execute Lucy's dominance strategy", IsFollowing = true },
                        new AdviceOption { Text = "Maintain current marketing pace", IsFollowing = false }
                    },
                    PotentialImpact = "Accelerates market share growth by 8% but costs significant capital"
                };
            }

            return null;
        }

        private void ApplyLucindaValeEffect(CharacterAdvice advice)
        {
            switch (advice.AdviceType)
            {
                case "pr_campaign":
                    company.Reputation = Math.Min(100, company.Reputation + 25);
                    company.MarketShare = Math.Min(100, company.MarketShare + 3);
                    company.Capital -= 50000; // Campaign cost
                    break;

                case "market_dominance_push":
                    company.MarketShare = Math.Min(100, company.MarketShare + 8);
                    company.Reputation = Math.Min(100, company.Reputation + 15);
                    company.Capital -= 150000; // Aggressive campaign cost
                    break;
            }
        }

        #endregion

        #region Gregory Shaw - Operations Manager (Efficiency)

        private CharacterAdvice? GenerateGregoryShawAdvice(int quarter)
        {
            // Efficiency optimization
            if (company.Risk > 50)
            {
                return new CharacterAdvice
                {
                    CharacterId = "gregory_shaw",
                    AdviceType = "efficiency_optimization",
                    Quarter = quarter,
                    Title = "Operational Efficiency Initiative",
                    Description = "Greg proposes streamlining operations to reduce risk and improve overall performance.",
                    Options = new List<AdviceOption>
                    {
                        new AdviceOption { Text = "Implement Greg's efficiency plan", IsFollowing = true },
                        new AdviceOption { Text = "Maintain current operations", IsFollowing = false }
                    },
                    PotentialImpact = "Reduces risk by 20 points and increases revenue by 12%"
                };
            }

            // Process improvement for large workforce
            if (company.EmployeeCount > 15)
            {
                return new CharacterAdvice
                {
                    CharacterId = "gregory_shaw",
                    AdviceType = "process_improvement",
                    Quarter = quarter,
                    Title = "Process Improvement Program",
                    Description = "Greg identifies opportunities to improve processes and maximize workforce productivity.",
                    Options = new List<AdviceOption>
                    {
                        new AdviceOption { Text = "Adopt Greg's process improvements", IsFollowing = true },
                        new AdviceOption { Text = "Keep existing processes", IsFollowing = false }
                    },
                    PotentialImpact = "Increases productivity by 18% and improves morale"
                };
            }

            return null;
        }

        private void ApplyGregoryShawEffect(CharacterAdvice advice)
        {
            switch (advice.AdviceType)
            {
                case "efficiency_optimization":
                    company.Risk = Math.Max(0, company.Risk - 20);
                    company.QuarterlyRevenue *= 1.12;
                    break;

                case "process_improvement":
                    company.QuarterlyRevenue *= 1.18;
                    company.Morale = Math.Min(100, company.Morale + 10);
                    break;
            }
        }

        #endregion

        #region Selena Park - Venture Capitalist (Buyout opportunities)

        private CharacterAdvice? GenerateSelenaParkAdvice(int quarter)
        {
            // Buyout offer at $1B threshold
            if (company.Capital >= 1000000000 && !storyData.StoryFlags.Contains("selena_buyout_offered"))
            {
                return new CharacterAdvice
                {
                    CharacterId = "selena_park",
                    AdviceType = "conglomerate_buyout",
                    Quarter = quarter,
                    Title = "Conglomerate Buyout Offer",
                    Description = "Selena presents a lucrative buyout offer from a major conglomerate. This could be your exit strategy.",
                    Options = new List<AdviceOption>
                    {
                        new AdviceOption { Text = "Accept the buyout offer", IsFollowing = true },
                        new AdviceOption { Text = "Decline and continue building", IsFollowing = false }
                    },
                    PotentialImpact = "Triggers ConglomerateBuyout ending if accepted"
                };
            }

            // Investment opportunity
            if (company.Capital > 250000 && company.Capital < 750000000)
            {
                return new CharacterAdvice
                {
                    CharacterId = "selena_park",
                    AdviceType = "investment_opportunity",
                    Quarter = quarter,
                    Title = "Strategic Investment Opportunity",
                    Description = "Selena offers investment capital to accelerate growth in exchange for strategic partnership.",
                    Options = new List<AdviceOption>
                    {
                        new AdviceOption { Text = "Accept Selena's investment", IsFollowing = true },
                        new AdviceOption { Text = "Decline the investment", IsFollowing = false }
                    },
                    PotentialImpact = "Provides capital injection of 25% current capital"
                };
            }

            return null;
        }

        private void ApplySelenaParkEffect(CharacterAdvice advice)
        {
            switch (advice.AdviceType)
            {
                case "conglomerate_buyout":
                    storyData.StoryFlags.Add("selena_buyout_offered");
                    storyData.StoryFlags.Add("selena_buyout_accepted");
                    // This will trigger the ConglomerateBuyout ending
                    break;

                case "investment_opportunity":
                    double investment = company.Capital * 0.25;
                    company.Capital += investment;
                    storyData.StoryFlags.Add($"selena_investment_Q{advice.Quarter}");
                    break;
            }
        }

        #endregion

        #region Harold Finch - Legal Counsel (Bankruptcy prevention)

        private CharacterAdvice? GenerateHaroldFinchAdvice(int quarter)
        {
            // Legal risk mitigation
            if (company.Risk > 70)
            {
                return new CharacterAdvice
                {
                    CharacterId = "harold_finch",
                    AdviceType = "legal_risk_mitigation",
                    Quarter = quarter,
                    Title = "Legal Risk Mitigation",
                    Description = "Harold warns of serious legal exposure and recommends immediate compliance measures.",
                    Options = new List<AdviceOption>
                    {
                        new AdviceOption { Text = "Implement Harold's legal protections", IsFollowing = true },
                        new AdviceOption { Text = "Accept the legal risks", IsFollowing = false }
                    },
                    PotentialImpact = "Reduces risk by 25 points and prevents lawsuit-related bankruptcy"
                };
            }

            // Bankruptcy prevention during financial crisis
            if (company.ConsecutiveNegativeQuarters >= 2)
            {
                return new CharacterAdvice
                {
                    CharacterId = "harold_finch",
                    AdviceType = "bankruptcy_prevention",
                    Quarter = quarter,
                    Title = "Bankruptcy Prevention Strategy",
                    Description = "Harold outlines legal strategies to prevent bankruptcy and protect company assets.",
                    Options = new List<AdviceOption>
                    {
                        new AdviceOption { Text = "Follow Harold's bankruptcy prevention plan", IsFollowing = true },
                        new AdviceOption { Text = "Handle it without legal intervention", IsFollowing = false }
                    },
                    PotentialImpact = "Provides legal protection against bankruptcy for 3 quarters"
                };
            }

            return null;
        }

        private void ApplyHaroldFinchEffect(CharacterAdvice advice)
        {
            switch (advice.AdviceType)
            {
                case "legal_risk_mitigation":
                    company.Risk = Math.Max(0, company.Risk - 25);
                    storyData.StoryFlags.Add($"legal_protection_active_Q{advice.Quarter}");
                    break;

                case "bankruptcy_prevention":
                    // Provide bankruptcy protection for 3 quarters
                    for (int i = 0; i < 3; i++)
                    {
                        storyData.StoryFlags.Add($"bankruptcy_protection_Q{advice.Quarter + i}");
                    }
                    break;
            }
        }

        #endregion

        #region Sophie Kim - Junior Analyst (Data insights)

        private CharacterAdvice? GenerateSophieKimAdvice(int quarter)
        {
            // Data-driven optimization
            if (quarter % 5 == 0) // Every 5 quarters
            {
                return new CharacterAdvice
                {
                    CharacterId = "sophie_kim",
                    AdviceType = "data_insights",
                    Quarter = quarter,
                    Title = "Data-Driven Optimization Insights",
                    Description = "Sophie's analysis reveals hidden patterns that could optimize company performance.",
                    Options = new List<AdviceOption>
                    {
                        new AdviceOption { Text = "Implement Sophie's data insights", IsFollowing = true },
                        new AdviceOption { Text = "Stick with current approach", IsFollowing = false }
                    },
                    PotentialImpact = "Provides hidden bonuses to all company metrics"
                };
            }

            // Predictive warning
            if (company.ConsecutiveNegativeQuarters == 1)
            {
                return new CharacterAdvice
                {
                    CharacterId = "sophie_kim",
                    AdviceType = "predictive_warning",
                    Quarter = quarter,
                    Title = "Predictive Analytics Warning",
                    Description = "Sophie's models predict potential financial trouble ahead based on current trends.",
                    Options = new List<AdviceOption>
                    {
                        new AdviceOption { Text = "Act on Sophie's predictions", IsFollowing = true },
                        new AdviceOption { Text = "Disregard the predictions", IsFollowing = false }
                    },
                    PotentialImpact = "Prevents further negative quarters through early intervention"
                };
            }

            return null;
        }

        private void ApplySophieKimEffect(CharacterAdvice advice)
        {
            switch (advice.AdviceType)
            {
                case "data_insights":
                    // Hidden bonuses to all metrics
                    company.Reputation = Math.Min(100, company.Reputation + 5);
                    company.Morale = Math.Min(100, company.Morale + 5);
                    company.MarketShare = Math.Min(100, company.MarketShare + 2);
                    company.QuarterlyRevenue *= 1.08;
                    storyData.StoryFlags.Add($"sophie_insights_Q{advice.Quarter}");
                    break;

                case "predictive_warning":
                    // Prevent negative quarter
                    company.QuarterlyRevenue *= 1.15;
                    company.Risk = Math.Max(0, company.Risk - 10);
                    break;
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents a piece of character advice with options for player response
    /// </summary>
    public class CharacterAdvice
    {
        public string CharacterId { get; set; } = "";
        public string AdviceType { get; set; } = "";
        public int Quarter { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public List<AdviceOption> Options { get; set; } = new List<AdviceOption>();
        public string PotentialImpact { get; set; } = "";
    }

    /// <summary>
    /// Represents a player response option to character advice
    /// </summary>
    public class AdviceOption
    {
        public string Text { get; set; } = "";
        public bool IsFollowing { get; set; }
    }
}
