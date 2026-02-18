# Win/Lose Conditions Update

## New Win Conditions

# Win/Lose Conditions Update

## New Win Conditions

### 1. Market Dominance (UPDATED)
- **Condition**: Reach 65% market share (reduced from 70%)
- **Result**: Immediate victory
- **Joan's Hints**: 
  - "You're so close to something special..." (at 60%+ market share)
  - "Push for that final stretch!" (at 60%+ market share)

### 2. Billionaire Acquisition (UPDATED)
- **Condition**: Reach $1,000,000,000 in capital (reduced from $5 billion)
- **Result**: Player gets choice to sell company or continue
- **Options**:
  - **SELL**: Retire as billionaire (Victory)
  - **CONTINUE**: Keep playing (can still win via market share or retire at Q120)
- **Joan's Hints**:
  - "Companies with this kind of wealth sometimes attract acquisition offers..." (at $500M+)
  - "Major corporations notice this kind of success" (at $750M+)
  - "You're in the big leagues now!" (at $1B+)

## New Lose Conditions

### 1. Bankruptcy (UPDATED)
- **Old Condition**: Capital ≤ 0 (immediate game over)
- **New Condition**: 2 consecutive quarters of negative capital
- **Tracking**: `Company.ConsecutiveNegativeQuarters` property
- **Joan's Hints**:
  - "If this continues for another quarter, we might face... serious consequences" (1 negative quarter)
  - "CRITICAL: X quarter(s) of negative capital - bankruptcy risk!" (in analysis)
  - "URGENT: Take immediate action to avoid... permanent consequences" (in recommendations)

### 2. No Employees (UPDATED)
- **Old Condition**: EmployeeCount ≤ 0 (any time)
- **New Condition**: EmployeeCount ≤ 0 after Quarter 1 (allows Q1 to start with 0)
- **Result**: "Business Failure - No employees left to run the company"
- **Joan's Hints**:
  - "A company can't function without people" (at ≤2 employees)
  - "CRITICAL: Only X employee(s) - companies need people to survive!" (at ≤1 employees)
  - "URGENT: Hire employees immediately - companies can't survive without people" (at ≤2 employees)

### 3. Retirement (Unchanged)
- **Condition**: Quarter > 120 (only in non-endless mode)
- **Result**: "Retirement - You've reached the end of your 30-year career!"
- **Joan's Health Dialogue**: Special dialogue around quarters 110-115 about health and aging

## Joan's Hint System

### Subtle Hints (Never Explicit)
- **Win Conditions**: Uses phrases like "something special", "interesting opportunities", "big leagues"
- **Lose Conditions**: Uses phrases like "serious consequences", "permanent consequences", "companies need people"

### Health & Retirement Dialogue (Q110-115)
Joan will mention:
- "You've been at this for nearly 30 years now. How are you feeling?"
- "The stress of running a company can take its toll on one's health"
- "You're not getting any younger"
- "Think about your long-term health and retirement plans"

### Context-Aware Responses
Joan's dialogue adapts based on:
- Current capital level (hints at acquisition at high levels)
- Market share progress (encouragement near 65%)
- Employee count (warnings when dangerously low)
- Consecutive negative quarters (bankruptcy warnings)
- Quarter number (health concerns near retirement)

## Implementation Details

### Code Changes
1. **Company.cs**: Added `ConsecutiveNegativeQuarters` tracking
2. **MainWindow.xaml.cs**: 
   - Updated win/lose condition logic
   - Added `HandleBillionaireWin()` method with sell/continue choice
   - Enhanced bankruptcy tracking
3. **JoanDialogue.xaml.cs**: 
   - Added contextual hints for all conditions
   - Added retirement health dialogue
   - Enhanced situation analysis with subtle warnings

### Game Flow
1. Each quarter, track if capital is negative
2. Reset counter if capital becomes positive
3. Check for 2 consecutive negative quarters → bankruptcy
4. Check for $1B capital → acquisition offer
5. Check for 65% market share → victory
6. Check for 0 employees (after Q1) → business failure
7. Check for Q120+ (non-endless) → retirement

This system provides more nuanced win/lose conditions while maintaining game balance and providing players with strategic choices and subtle guidance through Joan's dialogue system.