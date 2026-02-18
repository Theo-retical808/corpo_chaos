using CorporateChaos.Models;

namespace CorporateChaos.Systems
{
    public static class CharacterDialogue
    {
        // Marcus Vey - CFO: Shrewd, numbers-driven, impatient, risk-loving
        public static class MarcusVey
        {
            public static readonly Dictionary<string, List<string>> DialogueByContext = new Dictionary<string, List<string>>
            {
                ["introduction"] = new List<string>
                {
                    "Marcus Vey here, your new CFO. I've reviewed the financials - we have potential, but we're playing it too safe.",
                    "Let me be direct: I'm here to maximize returns. Conservative strategies won't get us to the top.",
                    "I've seen companies like ours stagnate with cautious leadership. Are you ready to take calculated risks for real growth?"
                },
                ["high_capital"] = new List<string>
                {
                    "Excellent capital position! This is exactly when we should be aggressive. High-risk, high-reward investments are calling.",
                    "With this kind of money, we could leverage into some serious growth opportunities. Playing it safe now would be a waste.",
                    "I'm seeing investment opportunities that could triple our returns. The question is: do you have the appetite for it?"
                },
                ["low_capital"] = new List<string>
                {
                    "Cash flow is tight, but that's when the best opportunities emerge. Desperate times call for bold moves.",
                    "We need to think outside the box here. Traditional cost-cutting won't save us - we need revenue acceleration.",
                    "This is make-or-break time. I have some aggressive strategies that could turn this around quickly."
                },
                ["high_risk"] = new List<string>
                {
                    "Perfect! High risk means we're finally playing to win. This is where fortunes are made.",
                    "I love seeing those risk numbers climb. It means we're not leaving money on the table.",
                    "Risk-averse companies die slow deaths. We're positioning ourselves for explosive growth."
                },
                ["employee_concerns"] = new List<string>
                {
                    "Look, I get that people are worried, but business isn't a charity. We optimize for profit, period.",
                    "Employee satisfaction is nice, but shareholder value is what keeps the lights on.",
                    "Sometimes you have to make tough decisions. That's what separates successful leaders from the rest."
                },
                ["market_opportunity"] = new List<string>
                {
                    "The market is ripe for disruption. We should be acquiring competitors while they're weak.",
                    "I'm seeing arbitrage opportunities everywhere. We need to move fast before others catch on.",
                    "Market timing is everything. Right now, aggressive expansion could capture massive market share."
                }
            };

            public static string GetDialogue(string context, Company company, Random random)
            {
                if (!DialogueByContext.ContainsKey(context))
                    context = "introduction";

                var dialogues = DialogueByContext[context];
                return dialogues[random.Next(dialogues.Count)];
            }

            public static List<string> GetAdvice(Company company)
            {
                var advice = new List<string>();

                if (company.Capital > 1000000)
                    advice.Add("💰 Marcus: 'With this capital, we should pursue aggressive expansion or high-yield investments.'");
                
                if (company.Risk < 20)
                    advice.Add("📈 Marcus: 'We're playing it too safe. Higher risk could mean exponentially higher rewards.'");
                
                if (company.MarketShare < 30)
                    advice.Add("🎯 Marcus: 'Market share acquisition through strategic investments could accelerate our growth.'");

                return advice;
            }
        }

        // Evelyn Cross - HR Head: Empathetic, organized, protective of employees
        public static class EvelynCross
        {
            public static readonly Dictionary<string, List<string>> DialogueByContext = new Dictionary<string, List<string>>
            {
                ["introduction"] = new List<string>
                {
                    "Hello, I'm Evelyn Cross, your Head of HR. I believe our people are our greatest asset, and I'm here to protect that.",
                    "I've spent years building positive workplace cultures. Every decision we make should consider the human impact.",
                    "My philosophy is simple: take care of your employees, and they'll take care of the business."
                },
                ["low_morale"] = new List<string>
                {
                    "I'm deeply concerned about our employee morale. When people are unhappy, productivity plummets and turnover skyrockets.",
                    "We need immediate intervention here. Low morale is like a virus - it spreads quickly and damages everything.",
                    "I've seen companies collapse because they ignored employee satisfaction. We cannot let that happen here."
                },
                ["high_morale"] = new List<string>
                {
                    "Our team morale is excellent! This is exactly the kind of positive culture that drives sustainable success.",
                    "Happy employees are productive employees. This positive energy will translate directly to better business outcomes.",
                    "We should celebrate this achievement and use it as a foundation for continued growth."
                },
                ["firing_concerns"] = new List<string>
                {
                    "I understand business needs, but every termination affects team morale. We need to handle this very carefully.",
                    "Before we let anyone go, let's explore performance improvement plans and additional training opportunities.",
                    "Firing should always be the last resort. The cost of replacing and retraining often exceeds the short-term savings."
                },
                ["hiring_opportunity"] = new List<string>
                {
                    "This is a great opportunity to bring in fresh talent and strengthen our team culture.",
                    "I've identified some excellent candidates who would not only fill our skill gaps but also enhance our workplace dynamic.",
                    "Strategic hiring now could prevent burnout and improve overall team performance."
                },
                ["cost_cutting_concerns"] = new List<string>
                {
                    "I'm worried about the impact of cost-cutting on our people. Employee benefits and development shouldn't be the first to go.",
                    "Short-term savings from cutting employee programs often lead to long-term costs in turnover and recruitment.",
                    "Let's find ways to reduce costs that don't compromise our commitment to our team."
                }
            };

            public static string GetDialogue(string context, Company company, Random random)
            {
                if (!DialogueByContext.ContainsKey(context))
                    context = "introduction";

                var dialogues = DialogueByContext[context];
                return dialogues[random.Next(dialogues.Count)];
            }

            public static List<string> GetAdvice(Company company)
            {
                var advice = new List<string>();

                if (company.Morale < 30)
                    advice.Add("😟 Evelyn: 'Employee morale is critically low. We need immediate action to prevent turnover.'");
                
                if (company.EmployeeCount < 5)
                    advice.Add("👥 Evelyn: 'We're understaffed. Consider hiring to improve productivity and reduce burnout.'");
                
                if (company.Morale > 60)
                    advice.Add("😊 Evelyn: 'Excellent team morale! This positive culture is our competitive advantage.'");

                return advice;
            }
        }

        // Vincent Duro - Rival CEO: Aggressive, cunning, publicly charming, privately cutthroat
        public static class VincentDuro
        {
            public static readonly Dictionary<string, List<string>> DialogueByContext = new Dictionary<string, List<string>>
            {
                ["introduction"] = new List<string>
                {
                    "Vincent Duro, CEO of DuroCorp. I've been watching your little operation with... interest.",
                    "Charmed to finally meet face-to-face. Your company has been making some noise in our market.",
                    "I always make it a point to know my competition personally. Consider this a courtesy call."
                },
                ["market_threat"] = new List<string>
                {
                    "Impressive market share growth, but you're entering dangerous territory now. My territory.",
                    "You've done well so far, but the real competition starts when you threaten established players like myself.",
                    "I hope you're prepared for what comes next. The market can be... unforgiving to newcomers who overreach."
                },
                ["competitive_response"] = new List<string>
                {
                    "Every move you make, I'm already three steps ahead. That's how you survive in this business.",
                    "You think you're being aggressive? Let me show you what real market dominance looks like.",
                    "I've crushed bigger companies than yours. But I respect the fight you're putting up."
                },
                ["respect_earned"] = new List<string>
                {
                    "I have to admit, you've exceeded my expectations. Perhaps we're more alike than I initially thought.",
                    "Your strategic thinking is... adequate. In another life, we might have been allies.",
                    "You've proven you can play at this level. That earns you a certain degree of professional respect."
                },
                ["market_dominance"] = new List<string>
                {
                    "Congratulations. You've achieved what few can. I may be your rival, but I'm not blind to excellence.",
                    "Market dominance suits you. Perhaps it's time we discussed... collaboration rather than competition.",
                    "You've won this round. But remember - in business, today's victor is tomorrow's target."
                }
            };

            public static string GetDialogue(string context, Company company, Random random)
            {
                if (!DialogueByContext.ContainsKey(context))
                    context = "introduction";

                var dialogues = DialogueByContext[context];
                return dialogues[random.Next(dialogues.Count)];
            }

            public static List<string> GetAdvice(Company company)
            {
                var advice = new List<string>();

                if (company.MarketShare > 40)
                    advice.Add("🏢 Vincent: 'Impressive market share, but can you maintain it against real competition?'");
                
                if (company.MarketShare > 60)
                    advice.Add("⚔️ Vincent: 'You've entered the big leagues now. Expect the competition to get fierce.'");

                return advice;
            }
        }

        // Lucinda Vale - PR & Marketing Head: Creative, persuasive, flamboyant, headline-focused
        public static class LucindaVale
        {
            public static readonly Dictionary<string, List<string>> DialogueByContext = new Dictionary<string, List<string>>
            {
                ["introduction"] = new List<string>
                {
                    "Darling! Lucinda Vale, your new PR and Marketing maven. I'm here to make this company absolutely irresistible!",
                    "Lucy Vale at your service! I live and breathe brand magic. Together, we're going to create something spectacular!",
                    "Hello gorgeous! I'm Lucy, and I'm going to transform your company into the talk of the industry!"
                },
                ["low_reputation"] = new List<string>
                {
                    "Oh honey, our reputation needs some serious TLC! But don't worry - I specialize in miraculous transformations.",
                    "This is actually perfect! Nothing makes a better story than a dramatic comeback. We're going to be front-page news!",
                    "Low reputation just means we have nowhere to go but up, and up we shall go - spectacularly!"
                },
                ["high_reputation"] = new List<string>
                {
                    "Absolutely divine! Our reputation is soaring, and I have so many ideas to keep us in the spotlight!",
                    "This is what I live for! When a company shines this bright, the whole world takes notice.",
                    "We're not just successful - we're becoming iconic! Let's capitalize on this momentum!"
                },
                ["marketing_opportunity"] = new List<string>
                {
                    "I'm seeing incredible opportunities for brand positioning! We could own this narrative completely!",
                    "The market is practically begging for what we're offering. We just need to tell our story better!",
                    "This is our moment to shine! I have campaigns brewing that will make our competitors weep with envy!"
                },
                ["crisis_management"] = new List<string>
                {
                    "Crisis? I prefer to call it an 'opportunity for narrative restructuring!' We can spin this beautifully!",
                    "Every great company has faced challenges. It's how we handle them that creates legends!",
                    "Don't panic, darling! I've turned bigger disasters into triumph stories. This is just another day at the office!"
                }
            };

            public static string GetDialogue(string context, Company company, Random random)
            {
                if (!DialogueByContext.ContainsKey(context))
                    context = "introduction";

                var dialogues = DialogueByContext[context];
                return dialogues[random.Next(dialogues.Count)];
            }

            public static List<string> GetAdvice(Company company)
            {
                var advice = new List<string>();

                if (company.Reputation < 20)
                    advice.Add("📢 Lucy: 'Our public image needs work! A strategic PR campaign could transform our reputation!'");
                
                if (company.MarketShare > 50)
                    advice.Add("✨ Lucy: 'We're market leaders! Let's make sure everyone knows it with a victory campaign!'");

                return advice;
            }
        }

        // Gregory Shaw - Operations Manager: Calm, methodical, numbers-focused, cynical
        public static class GregoryShaw
        {
            public static readonly Dictionary<string, List<string>> DialogueByContext = new Dictionary<string, List<string>>
            {
                ["introduction"] = new List<string>
                {
                    "Gregory Shaw, Operations Manager. I've seen a lot of companies come and go. Let's see if you can beat the odds.",
                    "Greg Shaw here. I handle operations - the unglamorous but essential work that keeps companies running.",
                    "Operations Manager Shaw reporting. I deal in facts, not fantasies. Let's talk about what actually works."
                },
                ["high_efficiency"] = new List<string>
                {
                    "Finally, some decent operational efficiency. This is what happens when you focus on fundamentals instead of flashy initiatives.",
                    "Good numbers across the board. Efficiency improvements like this don't happen by accident - they require discipline.",
                    "I'm impressed. Most executives talk about efficiency, but you're actually delivering measurable results."
                },
                ["low_efficiency"] = new List<string>
                {
                    "Our operational efficiency is abysmal. You can't run a company on good intentions and marketing campaigns.",
                    "These numbers are exactly what I expected. Without proper operational focus, everything else is just window dressing.",
                    "I've seen this pattern before - companies that ignore operations fundamentals don't last long."
                },
                ["high_risk"] = new List<string>
                {
                    "Risk levels this high make operations nearly impossible to manage. Every process becomes unstable.",
                    "I can't maintain operational excellence when everything is in constant chaos. We need stability.",
                    "High-risk strategies might sound exciting, but they make my job - keeping things running - exponentially harder."
                },
                ["cost_optimization"] = new List<string>
                {
                    "Now this is smart management. Operational cost optimization is where real value gets created.",
                    "Finally, someone who understands that cutting waste isn't about being cheap - it's about being efficient.",
                    "These cost reductions will compound over time. This is how you build sustainable competitive advantage."
                },
                ["expansion_concerns"] = new List<string>
                {
                    "Rapid expansion sounds great in boardrooms, but I'm the one who has to make it actually work operationally.",
                    "Before we grow, we need to perfect our current operations. Scaling broken processes just creates bigger problems.",
                    "I've seen too many companies collapse under the weight of poorly planned expansion. Let's be methodical."
                }
            };

            public static string GetDialogue(string context, Company company, Random random)
            {
                if (!DialogueByContext.ContainsKey(context))
                    context = "introduction";

                var dialogues = DialogueByContext[context];
                return dialogues[random.Next(dialogues.Count)];
            }

            public static List<string> GetAdvice(Company company)
            {
                var advice = new List<string>();

                if (company.Risk > 60)
                    advice.Add("⚙️ Greg: 'Operations are becoming unstable. We need to focus on efficiency and risk reduction.'");
                
                if (company.EmployeeCount > 15)
                    advice.Add("📊 Greg: 'With this workforce size, we need better operational systems and processes.'");
                
                var efficiency = Math.Max(50, 100 - company.Risk);
                if (efficiency < 70)
                    advice.Add("🔧 Greg: 'Operational efficiency is suffering. Time to streamline our processes.'");

                return advice;
            }
        }

        // Selena Park - Venture Capitalist: Persuasive, strategic, ROI-focused
        public static class SelenaPark
        {
            public static readonly Dictionary<string, List<string>> DialogueByContext = new Dictionary<string, List<string>>
            {
                ["introduction"] = new List<string>
                {
                    "Selena Park, representing Apex Ventures. I've been tracking your company's performance with great interest.",
                    "Hello! I'm Selena from Apex Ventures. Your growth trajectory has caught our attention in the investment community.",
                    "Selena Park here. I specialize in identifying high-potential companies for strategic investment opportunities."
                },
                ["investment_opportunity"] = new List<string>
                {
                    "Your financials are impressive. We're prepared to discuss a significant investment to accelerate your growth.",
                    "Companies with your profile often benefit from strategic capital injection. Let's explore what's possible.",
                    "I see tremendous potential here. The right investment partnership could transform your market position."
                },
                ["buyout_hint"] = new List<string>
                {
                    "Companies that reach the billion-dollar threshold often attract... interesting acquisition opportunities.",
                    "At your current trajectory, you might soon be fielding calls from major conglomerates looking to acquire.",
                    "Billion-dollar companies have options that smaller firms don't. Exit strategies become very attractive."
                },
                ["performance_analysis"] = new List<string>
                {
                    "Your ROI metrics are solid, but there's room for optimization. Let me share some strategic insights.",
                    "I've analyzed your performance against industry benchmarks. Here's what the data tells us.",
                    "From an investment perspective, your company shows strong fundamentals with clear growth potential."
                },
                ["market_positioning"] = new List<string>
                {
                    "Market positioning is everything in today's economy. You're well-positioned for the next growth phase.",
                    "Your competitive advantage is clear, but maintaining it requires strategic capital allocation.",
                    "I see opportunities for market expansion that could significantly increase your valuation."
                }
            };

            public static string GetDialogue(string context, Company company, Random random)
            {
                if (!DialogueByContext.ContainsKey(context))
                    context = "introduction";

                var dialogues = DialogueByContext[context];
                return dialogues[random.Next(dialogues.Count)];
            }

            public static List<string> GetAdvice(Company company)
            {
                var advice = new List<string>();

                if (company.Capital > 750000000)
                    advice.Add("💼 Selena: 'Companies with your financial profile often attract acquisition interest from major conglomerates...'");
                
                if (company.MarketShare > 50)
                    advice.Add("📈 Selena: 'Strong market position creates excellent opportunities for strategic partnerships.'");
                
                if (company.Capital > 500000000)
                    advice.Add("💰 Selena: 'Your valuation is approaching levels that open up significant exit opportunities.'");

                return advice;
            }
        }

        // Harold Finch - Legal Counsel: Precise, pedantic, highly cautious
        public static class HaroldFinch
        {
            public static readonly Dictionary<string, List<string>> DialogueByContext = new Dictionary<string, List<string>>
            {
                ["introduction"] = new List<string>
                {
                    "Harold Finch, Legal Counsel. I'm here to ensure this company operates within proper legal and regulatory frameworks.",
                    "Mr. Finch, your legal advisor. I must emphasize the importance of compliance and risk mitigation in all business decisions.",
                    "Harold Finch speaking. My role is to protect this company from legal exposure and regulatory complications."
                },
                ["high_risk_warning"] = new List<string>
                {
                    "I must strongly advise against these high-risk strategies. The legal exposure is... considerable.",
                    "Current risk levels expose us to potential lawsuits, regulatory action, and compliance violations.",
                    "From a legal standpoint, this level of risk is highly inadvisable. We need immediate risk mitigation measures."
                },
                ["compliance_concerns"] = new List<string>
                {
                    "I've identified several areas where our compliance protocols need strengthening. This is not optional.",
                    "Regulatory compliance isn't just about avoiding penalties - it's about protecting the company's future.",
                    "I strongly recommend implementing additional compliance measures before proceeding with expansion plans."
                },
                ["contract_negotiations"] = new List<string>
                {
                    "Any major business decisions should be thoroughly reviewed for legal implications before implementation.",
                    "I insist on reviewing all significant contracts and agreements. The devil is always in the details.",
                    "Proper legal documentation is essential. Verbal agreements and handshake deals are recipes for disaster."
                },
                ["crisis_legal_advice"] = new List<string>
                {
                    "In crisis situations, legal considerations become paramount. Every response must be carefully calculated.",
                    "I recommend extreme caution in all communications during this crisis. Legal liability is a serious concern.",
                    "Crisis management requires legal oversight. One wrong statement could expose us to significant liability."
                }
            };

            public static string GetDialogue(string context, Company company, Random random)
            {
                if (!DialogueByContext.ContainsKey(context))
                    context = "introduction";

                var dialogues = DialogueByContext[context];
                return dialogues[random.Next(dialogues.Count)];
            }

            public static List<string> GetAdvice(Company company)
            {
                var advice = new List<string>();

                if (company.Risk > 70)
                    advice.Add("⚖️ Harold: 'Current risk levels expose us to potential legal and regulatory issues. Immediate mitigation required.'");
                
                if (company.ConsecutiveNegativeQuarters > 0)
                    advice.Add("📋 Harold: 'Financial distress increases legal vulnerabilities. We need careful crisis management.'");
                
                if (company.MarketShare > 60)
                    advice.Add("🏛️ Harold: 'Market dominance brings increased regulatory scrutiny. Compliance becomes critical.'");

                return advice;
            }
        }

        // Sophie Kim - Junior Analyst: Enthusiastic, naive, data-loving
        public static class SophieKim
        {
            public static readonly Dictionary<string, List<string>> DialogueByContext = new Dictionary<string, List<string>>
            {
                ["introduction"] = new List<string>
                {
                    "Hi! I'm Sophie Kim, Junior Data Analyst! I'm so excited to work with you and help optimize our performance!",
                    "Sophie Kim here! I just graduated with a degree in Business Analytics and I'm thrilled to contribute to our success!",
                    "Hello! I'm Sophie, your data analyst! I love finding patterns and insights that can help improve our business!"
                },
                ["data_insights"] = new List<string>
                {
                    "I've been analyzing our performance metrics and I found some really interesting patterns in the data!",
                    "The numbers are telling such a fascinating story about our company's trajectory! Let me share what I discovered!",
                    "I ran some statistical models on our performance data and the results are quite revealing!"
                },
                ["optimization_suggestions"] = new List<string>
                {
                    "Based on my analysis, I think there are some amazing opportunities for optimization! Want to hear them?",
                    "I've identified several efficiency improvements that could really boost our performance! This is so exciting!",
                    "The data suggests we could improve our ROI by adjusting a few key operational parameters!"
                },
                ["learning_enthusiasm"] = new List<string>
                {
                    "Every quarter teaches me something new about business! I'm learning so much from watching your decisions!",
                    "This is such a great learning experience! I'm documenting all our strategies for future analysis!",
                    "I love how data-driven decision making can transform a company! Your leadership style is really inspiring!"
                },
                ["future_potential"] = new List<string>
                {
                    "I have so many ideas for advanced analytics that could help us stay ahead of the competition!",
                    "With more experience, I think I could develop predictive models for market trends and risk assessment!",
                    "I'm working on some innovative approaches to performance optimization that could be game-changing!"
                }
            };

            public static string GetDialogue(string context, Company company, Random random)
            {
                if (!DialogueByContext.ContainsKey(context))
                    context = "introduction";

                var dialogues = DialogueByContext[context];
                return dialogues[random.Next(dialogues.Count)];
            }

            public static List<string> GetAdvice(Company company)
            {
                var advice = new List<string>();

                var efficiency = Math.Max(50, 100 - company.Risk);
                advice.Add($"📊 Sophie: 'Data shows our efficiency is at {efficiency}%! I found some optimization opportunities!'");
                
                if (company.MarketShare > 30)
                    advice.Add("📈 Sophie: 'Our market share growth pattern suggests we could capture even more with targeted strategies!'");
                
                if (company.EmployeeCount > 10)
                    advice.Add("👥 Sophie: 'I analyzed our workforce data and found some interesting productivity correlations!'");

                return advice;
            }
        }

        // Helper method to get character-specific dialogue
        public static string GetCharacterDialogue(string characterId, string context, Company company, Random random)
        {
            return characterId switch
            {
                "marcus_vey" => MarcusVey.GetDialogue(context, company, random),
                "evelyn_cross" => EvelynCross.GetDialogue(context, company, random),
                "vincent_duro" => VincentDuro.GetDialogue(context, company, random),
                "lucinda_vale" => LucindaVale.GetDialogue(context, company, random),
                "gregory_shaw" => GregoryShaw.GetDialogue(context, company, random),
                "selena_park" => SelenaPark.GetDialogue(context, company, random),
                "harold_finch" => HaroldFinch.GetDialogue(context, company, random),
                "sophie_kim" => SophieKim.GetDialogue(context, company, random),
                _ => "Hello! I'm still developing my personality. Check back soon!"
            };
        }

        public static List<string> GetCharacterAdvice(string characterId, Company company)
        {
            return characterId switch
            {
                "marcus_vey" => MarcusVey.GetAdvice(company),
                "evelyn_cross" => EvelynCross.GetAdvice(company),
                "vincent_duro" => VincentDuro.GetAdvice(company),
                "lucinda_vale" => LucindaVale.GetAdvice(company),
                "gregory_shaw" => GregoryShaw.GetAdvice(company),
                "selena_park" => SelenaPark.GetAdvice(company),
                "harold_finch" => HaroldFinch.GetAdvice(company),
                "sophie_kim" => SophieKim.GetAdvice(company),
                _ => new List<string>()
            };
        }

        // Methods required by NarrativeEngine
        public static List<string> GetIntroductionDialogue(string characterId, Company company, Random random)
        {
            var dialogue = GetCharacterDialogue(characterId, "introduction", company, random);
            return new List<string> { dialogue };
        }

        public static List<string> GetRelationshipMilestoneDialogue(string characterId, RelationshipPhase phase, Company company, Random random)
        {
            var context = phase switch
            {
                RelationshipPhase.TrustedColleague => "trusted_colleague",
                RelationshipPhase.PersonalFriend => "personal_friend",
                RelationshipPhase.LifelongBond => "lifelong_bond",
                _ => "relationship_milestone"
            };

            var baseDialogue = GetCharacterDialogue(characterId, context, company, random);
            var milestoneText = GetRelationshipMilestoneText(characterId, phase);
            
            return new List<string> { milestoneText, baseDialogue };
        }

        public static List<string> GetPersonalChallengeDialogue(string characterId, Company company, Random random)
        {
            var challengeContext = GetPersonalChallengeContext(characterId);
            var dialogue = GetCharacterDialogue(characterId, challengeContext, company, random);
            var challengeIntro = GetPersonalChallengeIntro(characterId);
            
            return new List<string> { challengeIntro, dialogue };
        }

        public static List<string> GetConflictDialogue(string characterId, Company company, Random random)
        {
            var conflictContext = "conflict_resolution";
            var dialogue = GetCharacterDialogue(characterId, conflictContext, company, random);
            var conflictIntro = GetConflictIntro(characterId);
            
            return new List<string> { conflictIntro, dialogue };
        }

        private static string GetRelationshipMilestoneText(string characterId, RelationshipPhase phase)
        {
            var character = StoryScript.Characters[characterId];
            return phase switch
            {
                RelationshipPhase.TrustedColleague => $"Your professional relationship with {character.Name} has evolved into one of mutual trust and respect.",
                RelationshipPhase.PersonalFriend => $"{character.Name} has become more than just a colleague - you've developed a genuine personal connection.",
                RelationshipPhase.LifelongBond => $"Your bond with {character.Name} has grown into a lifelong partnership built on shared experiences.",
                _ => $"Your relationship with {character.Name} continues to deepen and strengthen."
            };
        }

        private static string GetPersonalChallengeContext(string characterId)
        {
            return characterId switch
            {
                "joan" => "personal_crisis",
                "marcus_vey" => "career_pressure",
                "evelyn_cross" => "work_life_balance",
                "vincent_duro" => "internal_pressure",
                "lucinda_vale" => "creative_burnout",
                "gregory_shaw" => "family_concerns",
                "selena_park" => "investor_pressure",
                "harold_finch" => "ethical_concerns",
                "sophie_kim" => "overwhelmed",
                _ => "personal_challenge"
            };
        }

        private static string GetPersonalChallengeIntro(string characterId)
        {
            return characterId switch
            {
                "joan" => "I need to talk to you about something personal that's been affecting my work...",
                "marcus_vey" => "I'm facing some pressure from my previous firm that I need to discuss with you.",
                "evelyn_cross" => "I'm struggling to balance everything on my plate and could use your guidance.",
                "vincent_duro" => "My own board is questioning some of my strategies. Interesting position to be in.",
                "lucinda_vale" => "I hate to admit this, but I'm feeling creatively stuck and need to talk it through.",
                "gregory_shaw" => "I have some family issues that might affect my availability. We should discuss this.",
                "selena_park" => "My investment partners are pressuring me for higher returns. This affects our relationship.",
                "harold_finch" => "I have some ethical concerns about our recent decisions that we need to address.",
                "sophie_kim" => "I'm feeling overwhelmed by the complexity of my responsibilities. Can we talk?",
                _ => "I'm facing a personal challenge that I'd like to discuss with you."
            };
        }

        private static string GetConflictIntro(string characterId)
        {
            var character = StoryScript.Characters[characterId];
            return $"We need to address the tension that's developed between us, {character.Name}. This can't continue.";
        }
    }
}