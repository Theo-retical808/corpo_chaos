# Design Document: Story Mode Expansion

## Overview

The story mode expansion transforms Corporate Chaos from a basic tutorial system into a compelling narrative experience spanning the full 120-quarter game. This design extends the existing StoryModeManager and dialogue systems to support rich character development, meaningful player choices, and emotional investment while maintaining seamless integration with core game mechanics.

The expansion introduces 8 additional characters beyond Joan, each with distinct personalities and strategic importance. The narrative follows a four-act structure that evolves from tutorial guidance to complex interpersonal relationships, culminating in meaningful endings that reflect the player's journey and choices.

## Architecture

### Narrative Engine Architecture

The expanded story system builds upon the existing Legacy_System while adding new components for character management, choice tracking, and narrative branching:

```
StoryModeManager (Extended)
├── CharacterManager
│   ├── Character Relationship Tracking
│   ├── Personality State Management
│   └── Character Arc Progression
├── NarrativeEngine
│   ├── Story Event Generation
│   ├── Choice Consequence System
│   └── Branch Path Management
├── DialogueSystem (Enhanced)
│   ├── Branching Conversation Trees
│   ├── Context-Aware Responses
│   └── Character Voice Consistency
└── EmotionalBeatManager
    ├── Pacing Control
    ├── Emotional Arc Tracking
    └── Investment Moment Creation
```

### Integration with Existing Systems

The story expansion integrates with existing Corporate Chaos systems:

- **ChaosEngine**: Story events can trigger or be triggered by chaos events
- **DecisionSystem**: Executive decisions become story catalysts
- **SaveLoadManager**: Extended to include character relationships and story state
- **Company Model**: Story events affect and respond to company performance
- **Employee System**: Characters can influence hiring, firing, and department management

## Components and Interfaces

### Core Character System

#### Character Data Model
```csharp
public class StoryCharacter
{
    public string CharacterId { get; set; }
    public string Name { get; set; }
    public string Role { get; set; }
    public CharacterPersonality Personality { get; set; }
    public CharacterRelationship RelationshipWithPlayer { get; set; }
    public List<string> CharacterArcMilestones { get; set; }
    public Dictionary<string, object> CharacterState { get; set; }
    public List<string> AvailableDialogueTopics { get; set; }
    public EndingInfluence EndingImpact { get; set; }
}

public class CharacterRelationship
{
    public int TrustLevel { get; set; } // -100 to 100
    public int ProfessionalRespect { get; set; } // -100 to 100
    public int PersonalConnection { get; set; } // -100 to 100
    public List<string> SharedExperiences { get; set; }
    public List<string> ConflictHistory { get; set; }
    public RelationshipPhase CurrentPhase { get; set; }
}

public enum RelationshipPhase
{
    FirstMeeting,
    ProfessionalAcquaintance,
    TrustedColleague,
    PersonalFriend,
    LifelongBond,
    Strained,
    Hostile
}
```

#### Character Personalities and Roles

**Joan (Secretary/Assistant)**
- Personality: Professional, supportive, gradually more personal
- Arc: Professional Assistant → Trusted Advisor → Personal Confidant → Lifelong Friend
- Ending Impact: Provides guidance and warnings; relationship quality affects player confidence

**Marcus Vey (CFO)**
- Personality: Shrewd, numbers-driven, impatient, risk-loving
- Arc: Ambitious newcomer → Trusted financial advisor → Strategic partner or rival
- Ending Impact: Investment advice affects bankruptcy/growth paths

**Evelyn "Eve" Cross (Head of HR)**
- Personality: Empathetic, organized, protective of employees
- Arc: Cautious HR professional → Employee advocate → Cultural guardian
- Ending Impact: Employee satisfaction affects productivity and retention

**Vincent Duro (Rival CEO)**
- Personality: Aggressive, cunning, publicly charming, privately cutthroat
- Arc: Distant competitor → Direct rival → Nemesis or respected opponent
- Ending Impact: Competitive pressure affects market share growth

**Lucinda "Lucy" Vale (PR & Marketing Head)**
- Personality: Creative, persuasive, flamboyant, headline-focused
- Arc: Enthusiastic marketer → Brand strategist → Public face of company
- Ending Impact: PR campaigns influence market dominance path

**Gregory "Greg" Shaw (Operations Manager)**
- Personality: Calm, methodical, numbers-focused, cynical
- Arc: Steady operations manager → Efficiency expert → Operational backbone
- Ending Impact: Operations quality affects overall performance

**Selena Park (Venture Capitalist)**
- Personality: Persuasive, strategic, ROI-focused
- Arc: Potential investor → Financial partner → Buyout opportunity
- Ending Impact: Offers conglomerate buyout at $1B threshold

**Harold "Hal" Finch (Legal Counsel)**
- Personality: Precise, pedantic, highly cautious
- Arc: Risk-averse lawyer → Trusted legal advisor → Strategic protector
- Ending Impact: Legal guidance prevents bankruptcy from lawsuits

**Sophie Kim (Junior Analyst/Intern)**
- Personality: Enthusiastic, naive, data-loving
- Arc: Eager intern → Valuable analyst → Protégé or successor
- Ending Impact: Data insights provide hidden bonuses

### Enhanced Dialogue System

#### Branching Conversation Trees
```csharp
public class DialogueNode
{
    public string NodeId { get; set; }
    public string SpeakerCharacterId { get; set; }
    public string DialogueText { get; set; }
    public List<DialogueChoice> PlayerChoices { get; set; }
    public List<string> RequiredConditions { get; set; }
    public Dictionary<string, object> StateChanges { get; set; }
    public EmotionalTone Tone { get; set; }
}

public class DialogueChoice
{
    public string ChoiceText { get; set; }
    public string NextNodeId { get; set; }
    public Dictionary<string, int> RelationshipChanges { get; set; }
    public List<string> ConsequenceFlags { get; set; }
    public ChoiceTone Tone { get; set; }
}

public enum ChoiceTone
{
    Professional,
    Supportive,
    Aggressive,
    Diplomatic,
    Personal,
    Humorous
}
```

### Story Event System

#### Narrative Event Types
```csharp
public enum NarrativeEventType
{
    CharacterIntroduction,
    RelationshipMilestone,
    PersonalChallenge,
    BusinessConflict,
    EmotionalBeat,
    ChoiceConsequence,
    ActTransition,
    EndingSetup
}

public class NarrativeEvent
{
    public string EventId { get; set; }
    public NarrativeEventType EventType { get; set; }
    public int TriggerQuarter { get; set; }
    public List<string> TriggerConditions { get; set; }
    public List<string> InvolvedCharacters { get; set; }
    public DialogueTree EventDialogue { get; set; }
    public List<StoryChoice> EventChoices { get; set; }
    public Dictionary<string, object> GameplayEffects { get; set; }
    public EmotionalWeight EmotionalImpact { get; set; }
}
```

## Data Models

### Extended Story Mode Data
```csharp
public class ExtendedStoryModeData : StoryModeData
{
    // Character relationship tracking
    public Dictionary<string, CharacterRelationship> CharacterRelationships { get; set; }
    
    // Story choice history
    public List<StoryChoiceRecord> ChoiceHistory { get; set; }
    
    // Current narrative state
    public NarrativeAct CurrentAct { get; set; }
    public List<string> ActiveStoryArcs { get; set; }
    public Dictionary<string, object> StoryFlags { get; set; }
    
    // Character arc progression
    public Dictionary<string, CharacterArcState> CharacterArcs { get; set; }
    
    // Emotional investment tracking
    public EmotionalInvestmentProfile PlayerEmotionalState { get; set; }
    
    // Ending path tracking
    public EndingPathData EndingProgression { get; set; }
}

public class StoryChoiceRecord
{
    public int Quarter { get; set; }
    public string EventId { get; set; }
    public string ChoiceId { get; set; }
    public string ChoiceText { get; set; }
    public Dictionary<string, int> RelationshipImpacts { get; set; }
    public List<string> ConsequenceFlags { get; set; }
}

public class EndingPathData
{
    public List<EndingType> ViableEndings { get; set; }
    public Dictionary<EndingType, double> EndingProbabilities { get; set; }
    public List<string> EndingRequirementsMet { get; set; }
    public List<string> EndingBlockers { get; set; }
}

public enum EndingType
{
    MarketDominance,        // 65% market share
    ConglomerateBuyout,     // $1B+ capital
    GracefulRetirement,     // Reach Q120 with stable company
    BankruptcyFailure,      // Consecutive negative quarters
    LostManpowerFailure,    // Zero employees
    HealthRetirement        // Early retirement due to health
}
```

### Character Arc Management
```csharp
public class CharacterArcState
{
    public string CharacterId { get; set; }
    public CharacterArcPhase CurrentPhase { get; set; }
    public List<string> CompletedMilestones { get; set; }
    public List<string> AvailableMilestones { get; set; }
    public Dictionary<string, object> ArcSpecificData { get; set; }
    public int NextMilestoneQuarter { get; set; }
}

public enum CharacterArcPhase
{
    Introduction,
    Development,
    Conflict,
    Resolution,
    Legacy
}
```

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

Now I need to analyze the acceptance criteria to determine which ones can be tested as properties. Let me use the prework tool to analyze the testability of each acceptance criterion.

<function_calls>
<invoke name="prework">
<parameter name="featureName">story-mode-expansion

### Property 1: Four-Act Narrative Structure
*For any* quarter number from 1 to 120, the Story_System should correctly identify which narrative act (Tutorial, Rising Action, Climax, Resolution) the quarter belongs to, and all quarters should be covered by exactly one act.
**Validates: Requirements 1.1**

### Property 2: Act Transition Events
*For any* act transition point (Q10→Q11, Q60→Q61, Q100→Q101), the Story_System should generate appropriate major story events that reflect the narrative progression.
**Validates: Requirements 1.2, 1.5**

### Property 3: Character System Completeness
*For any* character in the system, they should have all required attributes (name, role, personality, relationship tracking, ending impact) and the system should contain exactly 9 characters (Joan + 8 additional characters) with their specified roles and personalities.
**Validates: Requirements 3.1, 3.7, 13.6**

### Property 4: Joan Character Phase Progression
*For any* quarter number, Joan's character phase should correctly correspond to the defined ranges: Professional Assistant (Q1-10), Trusted Advisor (Q11-40), Personal Confidant (Q41-80), and Lifelong Friend (Q81-120).
**Validates: Requirements 2.1**

### Property 5: Character Introduction Events
*For any* character in the system, there should exist an introduction event that establishes their personality and role when they first appear.
**Validates: Requirements 3.2**

### Property 6: Relationship Dynamics System
*For any* player decision affecting employees or company culture, character relationships should change appropriately, and these changes should be reflected in dialogue content, available options, and character behavior.
**Validates: Requirements 9.1, 9.2, 9.3, 9.4**

### Property 7: Choice System Functionality
*For any* story choice presented to the player, it should have at least 2-3 options with defined consequences, and the choice should be tracked and referenced in future story events.
**Validates: Requirements 4.1, 4.2, 4.3, 4.4**

### Property 8: Story Branching System
*For any* sequence of different player choices, the system should generate different narrative content, creating distinct story experiences for different choice paths.
**Validates: Requirements 4.5, 7.1, 7.3, 7.4**

### Property 9: Story-Mechanic Integration
*For any* major business event (hiring, firing, financial decisions, crises), the Story_System should generate appropriate narrative context and character reactions that connect the mechanical action to story content.
**Validates: Requirements 6.1, 6.2, 6.3, 6.4**

### Property 10: Dialogue System Enhancement
*For any* character dialogue, it should maintain personality consistency, adapt based on relationship levels and story context, and support branching conversations with multiple response options that have clear tone indicators.
**Validates: Requirements 12.1, 12.2, 12.3, 12.4**

### Property 11: Timeline Content Coverage
*For any* quarter from 1 to 120, there should be available story content, with events distributed within acceptable frequency ranges to avoid overwhelming players or creating long gaps.
**Validates: Requirements 8.1, 8.3, 8.6**

### Property 12: Character Arc Progression
*For any* character with a defined story arc, their character state should change over time, with relationship progression occurring across multiple quarters.
**Validates: Requirements 2.3, 2.5, 3.3, 3.5**

### Property 13: Emotional Beat Implementation
*For any* significant story event, it should include appropriate emotional context and character reactions, with a balance of positive and negative emotional moments over time.
**Validates: Requirements 5.1, 5.2, 5.3**

### Property 14: Legacy System Compatibility
*For any* existing story mode functionality, it should continue to work correctly with the extended system, and existing save files should be compatible with expanded narrative content.
**Validates: Requirements 10.1, 10.2, 10.6**

### Property 15: Character Ending Impact System
*For any* character interaction or advice, it should appropriately affect the player's path toward different game endings based on the character's expertise area and the player's response to their guidance.
**Validates: Requirements 13.1, 13.2, 13.3, 13.4, 13.5**

## Error Handling

### Story System Error Recovery

The expanded story system must handle various error conditions gracefully:

#### Missing Story Content
- **Fallback Dialogue**: When specific story content is missing, the system falls back to generic character-appropriate dialogue
- **Event Generation**: If planned story events fail to load, the system generates contextually appropriate backup events
- **Character State Recovery**: If character relationship data becomes corrupted, the system estimates appropriate values based on choice history

#### Save Game Compatibility
- **Migration System**: Existing story saves are automatically migrated to the expanded format with default values for new fields
- **Version Detection**: The system detects save file versions and applies appropriate compatibility layers
- **Graceful Degradation**: If migration fails, the system continues with basic story functionality rather than crashing

#### Character Consistency Errors
- **Personality Validation**: Character dialogue is validated against personality profiles before presentation
- **Relationship Bounds**: Character relationships are clamped to valid ranges (-100 to 100) to prevent overflow errors
- **Arc State Validation**: Character arc progression is validated to ensure logical story flow

#### UI Integration Errors
- **Component Fallback**: If enhanced UI components fail, the system falls back to basic dialogue presentation
- **Event Display**: Story events that fail to display properly are logged and skipped rather than blocking progression
- **Choice Validation**: Invalid dialogue choices are filtered out before presentation to players

## Testing Strategy

### Dual Testing Approach

The story mode expansion requires both unit testing and property-based testing to ensure comprehensive coverage:

**Unit Tests** focus on:
- Specific character introduction sequences and dialogue trees
- Individual story event triggers and consequences
- Save/load functionality for extended story data
- UI component integration with new story features
- Edge cases like character relationship boundaries and arc transitions

**Property Tests** focus on:
- Universal properties that hold across all story content and character interactions
- Comprehensive input coverage through randomized story states and player choices
- Validation of story system invariants across different game scenarios
- Testing story branching with various choice combinations and company performance levels

### Property-Based Testing Configuration

Each property test will be configured to run a minimum of 100 iterations using the existing C# property-based testing framework. Tests will be tagged with comments referencing their corresponding design properties:

```csharp
[Test]
[Property(Iterations = 100)]
public void TestFourActNarrativeStructure()
{
    // Feature: story-mode-expansion, Property 1: Four-Act Narrative Structure
    // Test implementation here
}
```

### Integration Testing Strategy

**Story-Game Integration**: Tests verify that story events properly integrate with existing game mechanics without disrupting core gameplay loops.

**Character Relationship Testing**: Validates that character relationships evolve correctly based on player actions and maintain consistency across save/load cycles.

**Narrative Branching Testing**: Ensures that different choice paths create meaningfully different story experiences while maintaining narrative coherence.

**Performance Testing**: Validates that the expanded story system doesn't significantly impact game performance, especially during quarter transitions and dialogue sequences.

### Testing Data Management

**Character Profile Validation**: Tests ensure all characters have complete personality profiles and that dialogue generation respects these profiles.

**Story Content Coverage**: Validates that all quarters have appropriate story content and that no narrative gaps exist in the extended timeline.

**Choice Consequence Testing**: Verifies that player choices have appropriate short-term and long-term consequences that are properly tracked and referenced.

The testing strategy emphasizes both correctness (through property-based testing) and user experience (through unit testing of specific scenarios), ensuring that the expanded story system delivers both technical reliability and engaging narrative experiences.