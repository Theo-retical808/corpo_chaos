# Corporate Chaos

> **Beta Testing Notice:** This project is currently in beta testing. Features and mechanics may change, and there is no official instruction manual yet. Early testers have been briefed on the gameplay mechanics. Please expect updates and improvements in future releases.

Corporate Chaos is a turn-based business management simulation designed to gamify the experience of running a company. Its initial goal is to allow users to **simulate business management and supervision** in a fun yet analytical way, while also providing insight into different approaches people take when managing a business.  

## Features

- **Turn-Based System:** Each decision and action is processed in turns, allowing strategic planning and thoughtful management.  
- **Comprehensive Management:** Users can manage nearly all aspects of a business, from operations to finance, marketing, and human resources.  
- **Chaotic Event Tree:** Random events occur throughout the game, influenced by three core stats. This introduces unpredictability and challenges players to adapt.  
- **Story Mode:** Interactive tutorial system with Secretary Joan guiding players through game mechanics over the first 10 quarters. Now expanded into a comprehensive 120-quarter narrative experience (see Story Mode Expansion below).
- **Game Statistics:** At the end of each run, detailed statistics summarize the performance, decisions, and outcomes.
- **Options System:** Comprehensive settings management accessible from the main menu:
  - **Audio Settings:** Volume control slider (0-100%) with real-time preview and mute toggle
  - **Display Settings:** Windowed or fullscreen mode with instant application
  - **Settings Persistence:** All preferences saved to `settings.json` and restored on startup
  - **Real-time Updates:** Changes apply immediately without requiring restart
  - See `OPTIONS_SYSTEM.md` for complete documentation

## Planned Features

### Story Mode Expansion (Completed - Beta Testing)
The story mode has been expanded from a basic 10-quarter tutorial into a comprehensive narrative experience spanning all 120 quarters of gameplay. 

**✅ IMPLEMENTATION COMPLETE - All Core Systems Operational:**

**Completed Systems:**
- ✅ **Core Data Models:** Extended story data structures with character relationships, choice tracking, and narrative state management
- ✅ **Character System Foundation:** Character relationship tracking, arc progression, and personality management systems
- ✅ **All 8 Story Characters:** Complete character profiles with distinct personalities, dialogue patterns, and strategic roles
  - Marcus Vey (CFO) - Risk-loving financial strategist
  - Evelyn Cross (HR Head) - Employee-focused culture guardian
  - Vincent Duro (Rival CEO) - Competitive nemesis/respected opponent
  - Lucinda Vale (PR/Marketing) - Creative brand strategist
  - Gregory Shaw (Operations) - Methodical efficiency expert
  - Selena Park (Venture Capitalist) - Strategic investment advisor
  - Harold Finch (Legal Counsel) - Risk-averse compliance guardian
  - Sophie Kim (Junior Analyst) - Enthusiastic data specialist
- ✅ **Enhanced Dialogue System:** Branching conversations with relationship-based adaptations and choice consequences
- ✅ **Narrative Event System:** Story-mechanic integration, emotional beat management, and content distribution
- ✅ **Four-Act Narrative Structure:** Tutorial (Q1-10), Rising Action (Q11-60), Climax (Q61-100), Resolution (Q101-120)
- ✅ **Joan's Character Progression:** Evolves from professional assistant to lifelong friend across four relationship phases
- ✅ **Choice Tracking & Consequences:** Player decisions affect story direction, character relationships, and game endings
- ✅ **Story Branching System:** 8 distinct narrative paths based on player choices and company performance
- ✅ **Emotional Beat Manager:** Balanced pacing of triumph, challenge, bittersweet, and surprise moments
- ✅ **Timeline Content Coverage:** Story content distributed across all 120 quarters with proper pacing
- ✅ **Character Ending Impact:** Character advice and relationships influence ending probabilities
- ✅ **Main Game Integration:** Narrative events integrated with quarterly progression and UI
- ✅ **Character Interaction UI:** Dedicated window for player-initiated character conversations

**Key Features:**
- **Multi-Act Narrative Structure:** Four-act story with distinct emotional arcs and escalating stakes
- **Rich Character Development:** Joan and 8 additional characters with evolving relationships and personal arcs
- **Meaningful Player Choices:** Decisions affect story direction, character relationships, and game endings
- **Story-Mechanic Integration:** Business decisions become story catalysts with narrative context and character reactions
- **Multiple Endings:** Character relationships and choices influence paths toward market dominance, buyout opportunities, or other outcomes
- **Emotional Investment:** Designed emotional beats and character arcs create genuine investment in the narrative
- **Story Branching:** 8 distinct narrative paths (Aggressive Growth, Conservative Management, Employee-Focused, Profit-Focused, Ethical Leadership, Ruthless Efficiency, Innovation-Driven, Market Domination)

*See `.kiro/specs/story-mode-expansion/` for detailed requirements, design, and implementation documentation.*  

## Intention

The original intention of Corporate Chaos was to serve as a simulation for collecting data on **business practices and decision-making approaches** from different users. Over time, it has also become a small-scale game, enjoyed by testers and myself, blending both **analytics and entertainment**.

## License

See the [LICENSE](LICENSE) file for details.

