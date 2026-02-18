# Story Mode Implementation Guide

## Overview
Story Mode provides a guided tutorial experience where players learn corporate management through 8 structured quarters with Secretary Joan as their personal assistant.

**🚧 Expansion in Active Development:** The story mode is being expanded from a basic 8-quarter tutorial into a comprehensive narrative experience spanning all 120 quarters. 

**Current Implementation Status:**
- ✅ **Extended Data Models:** Character relationships, choice tracking, and narrative state management
- ✅ **Character Management System:** Relationship tracking and arc progression for all story characters
- ✅ **All 8 Story Characters:** Complete character implementations with distinct personalities and strategic roles
  - Marcus Vey (CFO) - Risk-loving financial strategist
  - Evelyn Cross (HR Head) - Employee-focused culture guardian  
  - Vincent Duro (Rival CEO) - Competitive nemesis/respected opponent
  - Lucinda Vale (PR/Marketing) - Creative brand strategist
  - Gregory Shaw (Operations) - Methodical efficiency expert
  - Selena Park (Venture Capitalist) - Strategic investment advisor
  - Harold Finch (Legal Counsel) - Risk-averse compliance guardian
  - Sophie Kim (Junior Analyst) - Enthusiastic data specialist
- 🚧 **Enhanced Dialogue System:** Branching conversations with relationship-based adaptations (in progress)
- 🚧 **Four-Act Structure:** Extended narrative spanning Tutorial → Rising Action → Climax → Resolution

See the [Story Mode Expansion Specification](.kiro/specs/story-mode-expansion/) for detailed requirements, design documentation, and implementation progress.

## Current Story Structure

### Tutorial Phase (Quarters 1-8)
Each quarter introduces a new game mechanic in order of priority:

1. **Q1 - Basic Operations**: Understanding company stats and basic gameplay
2. **Q2 - Employee Hiring**: Learning the recruitment system
3. **Q3 - Department Management**: Organizing workforce effectively
4. **Q4 - Executive Decisions**: Making strategic business choices
5. **Q5 - Financial Management**: Budget allocation and financial planning
6. **Q6 - Crisis Management**: Handling unexpected challenges
7. **Q7 - Market Competition**: Competing against rivals
8. **Q8 - Advanced Strategy**: Mastering complex decision-making

### Full Mode (Quarter 9+)
After completing the tutorial, players have access to all mechanics and play independently.

## Key Characters

### Secretary Joan
- **Role**: Personal corporate assistant and guide
- **Image**: `images/assistant.png` (needs to be added)
- **Personality**: Professional, supportive, knowledgeable
- **Function**: Provides tutorials, objectives, and encouragement

## Story Events

### Quarter 1: Company Takeover
- **Scenario**: Taking over MidCorp Industries with key staff already in place
- **Learning**: Basic company stats and operations
- **Starting Team**: Dr. Sarah Mitchell (Research), Alex Rodriguez (Marketing), Jennifer Chen (HR)
- **Key Warning**: Joan explains that losing all employees results in immediate business failure
- **Objective**: Complete first quarter and understand dashboard

### Quarter 2: Team Expansion
- **Scenario**: Board wants company growth
- **Learning**: Hiring system and candidate evaluation
- **Objective**: Hire 2-3 new employees

### Quarter 3: Workforce Organization
- **Scenario**: New hires need proper assignments
- **Learning**: Department management and employee placement
- **Objective**: Assign all employees to appropriate departments

### Quarter 4: Strategic Direction
- **Scenario**: Company needs strategic leadership
- **Learning**: Executive decisions and strategic planning
- **Objective**: Launch a marketing campaign

### Quarter 5: Financial Planning
- **Scenario**: Budget allocation for balanced growth
- **Learning**: Financial management and resource allocation
- **Objective**: Adjust department budget allocations

### Quarter 6: Supply Chain Crisis
- **Scenario**: Major supplier failure
- **Learning**: Crisis management and risk handling
- **Objective**: Navigate crisis while maintaining stability

### Quarter 7: Competitive Threat
- **Scenario**: Competitor launches similar product
- **Learning**: Market competition and defensive strategies
- **Objective**: Defend market share against competition

### Quarter 8: Strategic Mastery
- **Scenario**: Demonstrating complete understanding
- **Learning**: Advanced strategic thinking
- **Objective**: Achieve 12% market share and 60+ morale

## Technical Implementation

### Core Files
- `models/StoryMode.cs`: Data structures and story script
- `systems/StoryModeManager.cs`: Story mode logic and progression
- `views/StoryModeGuide.xaml/.cs`: Joan's dialogue interface

### Integration Points
- Main menu Story Mode button
- Quarter-end story event triggers
- Mechanic unlock system
- Progress tracking and save system

### Features
- **Progressive Unlocking**: Mechanics unlock as story progresses
- **Guided Tutorials**: Joan provides step-by-step instructions
- **Narrative Context**: Each quarter has story-driven scenarios
- **Objective Tracking**: Clear goals for each tutorial phase
- **Graduation System**: Transition to full mode after Q8

## Usage Instructions

### Starting Story Mode
1. Click "📖 Story Mode" on main menu
2. Confirm to start guided experience
3. Joan will appear with Q1 tutorial

### During Story Mode
- Joan appears at start of each new quarter
- Follow objectives shown in dialogue window
- Mechanics unlock progressively
- Story events provide context for learning

### Completing Story Mode
- After Q8, receive graduation message
- All mechanics become available
- Continue playing in full mode
- Story progress is saved automatically

## Customization Options

### Adding New Story Events
1. Add event to `StoryScript.StoryEvents` dictionary
2. Define quarter, dialogue, and objectives
3. Specify which mechanic to unlock

### Modifying Joan's Dialogue
- Edit dialogue arrays in story events
- Support for multi-part conversations
- Navigation between dialogue segments

### Extending Tutorial Length
- Add more quarters to tutorial phase
- Modify graduation threshold in StoryModeManager
- Add new mechanic types as needed

## File Requirements

### Images
- `images/assistant.png`: Secretary Joan's avatar (80x80px recommended)

### Save Files
- `story_progress.json`: Tracks player progress through story mode
- Compatible with existing save/load system

## Bug Fixes and Improvements

### Starting Employees Fix
- **Issue**: Story mode was ending immediately due to zero employees
- **Solution**: Added 3 starting employees in key departments (Research, Marketing, HR)
- **Characters**: 
  - Dr. Sarah Mitchell (Senior Research Scientist)
  - Alex Rodriguez (Mid-level Marketing Specialist) 
  - Jennifer Chen (Senior HR Manager)

### Employee Management Education
- **Joan's Warnings**: Added explicit guidance about employee retention
- **Key Message**: "Never let your employee count reach zero! Without human capital, the business cannot operate"
- **Reinforcement**: Reminders in Q3 (Department Management) and Q6 (Crisis Management)

### Technical Implementation
- `SetupStartingEmployees()` method in StoryModeManager
- Pre-assigned employees to appropriate departments
- Proper skill sets and experience levels for tutorial balance

## Future Enhancements

### Story Mode Expansion (In Active Development)
The story mode is being transformed into a comprehensive narrative experience with:

**Currently Implemented:**
- **Extended Data Architecture**: Character relationships, choice tracking, and narrative state management systems
- **Character Management**: Relationship dynamics, arc progression, and personality-driven interactions  
- **All 8 Story Characters**: Complete character implementations with distinct personalities, dialogue patterns, and strategic roles
  - Marcus Vey (CFO) - Risk-loving financial strategist who influences investment decisions
  - Evelyn Cross (HR Head) - Employee-focused culture guardian who affects morale and retention
  - Vincent Duro (Rival CEO) - Competitive nemesis whose actions impact market competition
  - Lucinda Vale (PR/Marketing) - Creative brand strategist who influences reputation and marketing effectiveness
  - Gregory Shaw (Operations) - Methodical efficiency expert who affects operational performance
  - Selena Park (Venture Capitalist) - Strategic investment advisor who offers buyout opportunities
  - Harold Finch (Legal Counsel) - Risk-averse compliance guardian who prevents legal disasters
  - Sophie Kim (Junior Analyst) - Enthusiastic data specialist who provides performance insights
- **Foundation Systems**: CharacterManager and CharacterDialogue classes supporting the expanded narrative

**In Development:**
- **Extended Timeline**: Story content spanning all 120 quarters instead of just 8
- **Character Development**: Joan evolves from tutorial guide to lifelong friend across four relationship phases
- **Enhanced Dialogue System**: Branching conversations with relationship-based adaptations and choice consequences (in progress)
- **Meaningful Choices**: Player decisions affect story direction, character relationships, and game endings
- **Emotional Investment**: Designed emotional beats and character arcs for genuine narrative engagement
- **Multiple Endings**: Character relationships influence paths toward market dominance, buyout opportunities, or other outcomes

*See `.kiro/specs/story-mode-expansion/` for complete requirements, design, and implementation plans.*

### Legacy Potential Additions
- Achievement system for story milestones
- Replay system for completed tutorials
- Advanced scenarios beyond basic tutorial

### Integration Opportunities
- Link with existing chaos events system
- Expand competitive scenarios
- Add industry-specific story branches
- Integrate with high score system for story mode achievements