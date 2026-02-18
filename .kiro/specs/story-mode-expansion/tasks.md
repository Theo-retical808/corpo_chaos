# Implementation Plan: Story Mode Expansion

## Overview

This implementation plan transforms the Corporate Chaos story mode from a basic 10-quarter tutorial into a compelling narrative experience spanning all 120 quarters. The implementation extends existing systems (StoryModeManager, dialogue components) while adding new character management, choice tracking, and narrative branching capabilities. All tasks build incrementally to ensure the expanded story system integrates seamlessly with existing game mechanics.

## Current Implementation Status

**COMPLETED SYSTEMS:**
- ✅ **Core Data Models**: ExtendedStoryModeData, StoryCharacter, CharacterRelationship, DialogueNode, DialogueChoice, NarrativeEvent
- ✅ **Character System**: CharacterManager with relationship tracking and arc progression
- ✅ **All 8 Characters**: Complete character profiles with dialogue patterns (Marcus Vey, Evelyn Cross, Vincent Duro, Lucinda Vale, Gregory Shaw, Selena Park, Harold Finch, Sophie Kim)
- ✅ **Enhanced Dialogue System**: DialogueSystem with branching conversations, relationship adaptation, and context awareness
- ✅ **JoanDialogue UI**: Updated with branching conversation support, choice buttons, consequence previews
- ✅ **Narrative Engine**: Event generation, character development events, conflict events, milestone events
- ✅ **Character Dialogue**: Complete dialogue system with character-specific patterns and advice
- ✅ **Story Integration**: StoryModeManager extended with character management and adaptive dialogue

**PARTIALLY IMPLEMENTED:**
- 🔄 **Choice Tracking**: Data models exist but need integration with main game loop
- 🔄 **Story Branching**: Framework exists but needs content generation
- 🔄 **Act Transitions**: Enums and helper methods exist but need event integration
- 🔄 **Ending System**: Data models exist but need probability calculation integration

**NOT IMPLEMENTED:**
- ❌ **EmotionalBeatManager**: System for managing emotional pacing
- ❌ **StoryMechanicIntegration**: Dedicated system for story-mechanic connections
- ❌ **Timeline Content Coverage**: System ensuring content across all 120 quarters
- ❌ **Main Game Integration**: Connecting narrative events to quarterly progression

## Tasks

- [x] 1. Extend core story data models and infrastructure
  - Create ExtendedStoryModeData class extending existing StoryModeData
  - Add character relationship tracking, choice history, and narrative state models
  - Implement data migration system for existing story saves
  - _Requirements: 10.1, 10.2, 10.5, 10.6_

- [ ]* 1.1 Write property test for data model extensions
  - **Property 14: Legacy System Compatibility**
  - **Validates: Requirements 10.1, 10.2, 10.6**

- [x] 2. Implement character system foundation
  - [x] 2.1 Create StoryCharacter and CharacterRelationship data models
    - Define character personality traits, relationship phases, and arc states
    - Implement relationship calculation and progression logic
    - _Requirements: 3.1, 3.7, 9.1_

  - [ ]* 2.2 Write property test for character system completeness
    - **Property 3: Character System Completeness**
    - **Validates: Requirements 3.1, 3.7, 13.6**

  - [x] 2.3 Implement CharacterManager class
    - Handle character relationship tracking and updates
    - Manage character arc progression and milestone tracking
    - _Requirements: 2.1, 3.3, 9.1_

  - [ ]* 2.4 Write property test for relationship dynamics
    - **Property 6: Relationship Dynamics System**
    - **Validates: Requirements 9.1, 9.2, 9.3, 9.4**

- [x] 3. Create the 8 additional story characters
  - [x] 3.1 Implement Marcus Vey (CFO) character profile and dialogue patterns
    - Define personality traits: shrewd, numbers-driven, impatient, risk-loving
    - Create character arc: ambitious newcomer → trusted advisor → strategic partner/rival
    - Use placeholder image (only Joan currently has an image asset)
    - _Requirements: 3.7, 13.6_

  - [x] 3.2 Implement Evelyn Cross (HR Head) character profile and dialogue patterns
    - Define personality traits: empathetic, organized, protective of employees
    - Create character arc: cautious professional → employee advocate → cultural guardian
    - _Requirements: 3.7, 13.6_

  - [x] 3.3 Implement Vincent Duro (Rival CEO) character profile and dialogue patterns
    - Define personality traits: aggressive, cunning, publicly charming, privately cutthroat
    - Create character arc: distant competitor → direct rival → nemesis/respected opponent
    - _Requirements: 3.7, 13.6_

  - [x] 3.4 Implement Lucinda Vale (PR/Marketing Head) character profile and dialogue patterns
    - Define personality traits: creative, persuasive, flamboyant, headline-focused
    - Create character arc: enthusiastic marketer → brand strategist → public face
    - _Requirements: 3.7, 13.6_

  - [x] 3.5 Implement Gregory Shaw (Operations Manager) character profile and dialogue patterns
    - Define personality traits: calm, methodical, numbers-focused, cynical
    - Create character arc: steady manager → efficiency expert → operational backbone
    - _Requirements: 3.7, 13.6_

  - [x] 3.6 Implement Selena Park (Venture Capitalist) character profile and dialogue patterns
    - Define personality traits: persuasive, strategic, ROI-focused
    - Create character arc: potential investor → financial partner → buyout opportunity
    - _Requirements: 3.7, 13.6_

  - [x] 3.7 Implement Harold Finch (Legal Counsel) character profile and dialogue patterns
    - Define personality traits: precise, pedantic, highly cautious
    - Create character arc: risk-averse lawyer → trusted advisor → strategic protector
    - _Requirements: 3.7, 13.6_

  - [x] 3.8 Implement Sophie Kim (Junior Analyst) character profile and dialogue patterns
    - Define personality traits: enthusiastic, naive, data-loving
    - Create character arc: eager intern → valuable analyst → protégé/successor
    - _Requirements: 3.7, 13.6_

  - [ ]* 3.9 Write property test for character introduction events
    - **Property 5: Character Introduction Events**
    - **Validates: Requirements 3.2**

- [x] 4. Checkpoint - Verify character system foundation
  - Ensure all tests pass, ask the user if questions arise.

- [x] 5. Enhance dialogue system for branching conversations
  - [x] 5.1 Extend DialogueNode and DialogueChoice models
    - Add support for multiple response options with tone indicators
    - Implement consequence tracking and relationship impact calculation
    - _Requirements: 4.2, 12.1, 12.4_

  - [x] 5.2 Update JoanDialogue.xaml/.cs to support branching conversations
    - Add UI elements for multiple choice selection
    - Implement choice consequence preview and tone indication
    - _Requirements: 12.1, 12.4_

  - [x] 5.3 Implement dialogue adaptation based on relationships and context
    - Create dialogue filtering system based on relationship levels
    - Add context-aware dialogue generation for different story states
    - _Requirements: 12.2, 12.3_

  - [ ]* 5.4 Write property test for dialogue system enhancement
    - **Property 10: Dialogue System Enhancement**
    - **Validates: Requirements 12.1, 12.2, 12.3, 12.4**

- [x] 6. Implement narrative event system
  - [x] 6.1 Create NarrativeEvent and NarrativeEngine classes
    - Define event types: character development, conflicts, milestones, choices
    - Implement event generation based on company state and player actions
    - _Requirements: 6.1, 6.2, 11.1, 11.3_

  - [x] 6.2 Implement story-mechanic integration system
    - Connect business events (hiring, firing, decisions) to story generation
    - Create character reaction system for major business events
    - _Requirements: 6.2, 6.3, 6.4_

  - [ ]* 6.3 Write property test for story-mechanic integration
    - **Property 9: Story-Mechanic Integration**
    - **Validates: Requirements 6.1, 6.2, 6.3, 6.4**

- [ ] 7. Implement choice tracking and consequence system
  - [ ] 7.1 Integrate choice tracking with main game loop
    - Connect StoryChoiceRecord creation to dialogue choices
    - Implement choice consequence triggering in quarterly progression
    - _Requirements: 4.3, 4.4_

  - [-] 7.2 Implement story branching system
    - Create branch path generation based on choice history
    - Implement different narrative experiences for different choice sequences
    - _Requirements: 4.5, 7.1, 7.3_

  - [ ]* 7.3 Write property test for choice system functionality
    - **Property 7: Choice System Functionality**
    - **Validates: Requirements 4.1, 4.2, 4.3, 4.4**

  - [ ]* 7.4 Write property test for story branching system
    - **Property 8: Story Branching System**
    - **Validates: Requirements 4.5, 7.1, 7.3, 7.4**

- [ ] 8. Implement four-act narrative structure
  - [ ] 8.1 Integrate act transition events with quarterly progression
    - Connect NarrativeEngine act transition events to main game loop
    - Implement act transition event display and player interaction
    - _Requirements: 1.1, 1.2, 1.5_

  - [ ] 8.2 Implement Joan's character phase progression system
    - Integrate Joan phase transitions with relationship tracking
    - Update dialogue system to reflect Joan's evolving relationship
    - _Requirements: 2.1, 2.2_

  - [ ]* 8.3 Write property test for four-act narrative structure
    - **Property 1: Four-Act Narrative Structure**
    - **Validates: Requirements 1.1**

  - [ ]* 8.4 Write property test for act transition events
    - **Property 2: Act Transition Events**
    - **Validates: Requirements 1.2, 1.5**

  - [ ]* 8.5 Write property test for Joan character phase progression
    - **Property 4: Joan Character Phase Progression**
    - **Validates: Requirements 2.1**

- [ ] 9. Checkpoint - Verify narrative structure implementation
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 10. Implement emotional beat and investment system
  - [ ] 10.1 Create EmotionalBeatManager and emotional event types
    - Define emotional beat categories: triumph, challenge, bittersweet, surprise
    - Implement emotional balance tracking across story timeline
    - _Requirements: 5.1, 5.2, 5.3_

  - [ ] 10.2 Implement character support and empathy response system
    - Create empathetic choice options during character challenges
    - Implement relationship repair opportunities for negative changes
    - _Requirements: 5.4, 9.5_

  - [ ]* 10.3 Write property test for emotional beat implementation
    - **Property 13: Emotional Beat Implementation**
    - **Validates: Requirements 5.1, 5.2, 5.3**

- [ ] 11. Implement timeline content coverage system
  - [ ] 11.1 Create story content distribution system across all 120 quarters
    - Ensure every quarter has available story content
    - Implement event frequency balancing to avoid overwhelming or gaps
    - _Requirements: 8.1, 8.3_

  - [ ] 11.2 Implement milestone and seasonal story events
    - Create time-based events that mark company growth and passage of time
    - Implement company archetype-specific story content
    - _Requirements: 7.6, 8.6_

  - [ ]* 11.3 Write property test for timeline content coverage
    - **Property 11: Timeline Content Coverage**
    - **Validates: Requirements 8.1, 8.3, 8.6**

- [ ] 12. Implement character ending impact system
  - [ ] 12.1 Integrate ending probability tracking with character advice
    - Connect character advice to ending probability calculations
    - Implement ending path updates based on player choices
    - _Requirements: 13.1, 13.2, 13.5_

  - [ ] 12.2 Implement character-specific ending influences
    - Marcus Vey: investment advice affects bankruptcy/growth paths
    - Evelyn Cross: employee satisfaction affects productivity/retention
    - Vincent Duro: competitive responses affect market share
    - Lucy Vale: PR campaigns influence market dominance
    - Greg Shaw: operations efficiency affects performance
    - Selena Park: offers buyout opportunities at $1B threshold
    - Hal Finch: legal advice prevents bankruptcy from lawsuits
    - Sophie Kim: data insights provide hidden bonuses
    - _Requirements: 13.3, 13.4, 13.6_

  - [ ]* 12.3 Write property test for character ending impact system
    - **Property 15: Character Ending Impact System**
    - **Validates: Requirements 13.1, 13.2, 13.3, 13.4, 13.5**

- [ ] 13. Integrate expanded story system with main game loop
  - [ ] 13.1 Connect narrative events to quarterly progression
    - Integrate NarrativeEngine event generation with MainWindow quarterly updates
    - Implement story event display and player interaction in main game flow
    - _Requirements: 10.1, 10.3, 1.4, 6.5_

  - [ ] 13.2 Implement character introduction and interaction system
    - Connect character introduction events to quarterly progression
    - Add character interaction options to main game UI
    - _Requirements: 3.2, 3.3, 3.5_

  - [ ]* 13.3 Write property test for character arc progression
    - **Property 12: Character Arc Progression**
    - **Validates: Requirements 2.3, 2.5, 3.3, 3.5**

- [ ] 14. Final integration and UI enhancements
  - [ ] 14.1 Update StoryModeGuide for character interactions
    - Extend StoryModeGuide.xaml/.cs for new character interactions
    - Add visual indicators for relationship levels and story progress
    - _Requirements: 10.3_

  - [ ] 14.2 Implement story progress tracking UI
    - Add story progress indicators to main game interface
    - Create character relationship status displays
    - _Requirements: 10.3_

  - [ ]* 14.3 Write integration tests for story-game compatibility
    - Test that story events integrate properly with existing game mechanics
    - Verify that expanded story system maintains game performance
    - _Requirements: 1.4_

- [ ] 15. Final checkpoint - Comprehensive system verification
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation and user feedback
- Property tests validate universal correctness properties across the story system
- Unit tests validate specific character interactions and story scenarios
- The implementation maintains backward compatibility with existing story mode saves
- Character personalities and dialogue patterns follow the detailed specifications provided
- All 8 additional characters have distinct roles and ending impacts as specified
- **Character Images**: Only Joan currently has an image asset (assistant.png). All other characters should use placeholder images or fallback to existing UI elements until proper character artwork is created

## Implementation Priority

**HIGH PRIORITY (Core Functionality):**
1. Choice tracking integration (Task 7.1)
2. Act transition integration (Task 8.1)
3. Main game loop integration (Task 13.1)
4. Character introduction system (Task 13.2)

**MEDIUM PRIORITY (Enhanced Experience):**
1. Story branching system (Task 7.2)
2. Emotional beat manager (Task 10.1)
3. Timeline content coverage (Task 11.1)
4. Ending impact system (Task 12.1-12.2)

**LOW PRIORITY (Polish & Testing):**
1. UI enhancements (Task 14.1-14.2)
2. Property tests (all optional tasks marked with *)
3. Integration tests (Task 14.3)