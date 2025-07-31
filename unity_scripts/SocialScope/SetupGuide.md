# SocialScope - Unity Setup Guide

## Creating the Game Scene

1. Create a new scene in Unity (File > New Scene)
2. Save the scene as "SocialScope.unity"

## Setting Up the Environment

1. Create a 3D environment representing common social settings:
   - Living room scene
   - Classroom scene
   - Public space (park, café, etc.)
   - Office environment
   
2. For each environment:
   - Add appropriate furniture and props
   - Set up lighting to create a realistic atmosphere
   - Add interaction points (marked with subtle indicators)

## Setting Up Characters

1. Import humanoid character models or use Unity's character assets
2. For each character:
   - Add a Character Controller component
   - Add an Animator component
   - Create blend shapes for facial expressions (or use a facial animation system)
   
3. Create an Animator Controller for characters with the following states:
   - Idle
   - Walking
   - Talking (with variations for different emotions)
   - Listening
   - Various emotional states (happy, sad, confused, etc.)

4. Set up the player character:
   - First-person or third-person controller based on preference
   - Camera setup appropriate for the chosen perspective

## Setting Up UI

1. Create a Canvas (GameObject > UI > Canvas)
2. Add an EventSystem if not already present (GameObject > UI > Event System)
3. Inside the Canvas, create the following UI elements:

### Main UI Elements

- **Panel - Main Menu**: Initial menu
  - Title: "SocialScope"
  - Button: "Start New Session"
  - Button: "Continue Session"
  - Button: "Settings"
  - Button: "Tutorial"
  
- **Panel - Scenario Info**: Shows current scenario information
  - Text: Scenario title
  - Text: Brief description
  - Text: Current objective
  
- **Panel - Dialogue**: Shows character dialogue
  - Text: Character name
  - Text: Dialogue content
  - Image: Character portrait (optional)
  
- **Panel - Response Options**: Shows player response choices
  - Multiple buttons for different response options
  - Each button should have text describing the response
  
- **Panel - Feedback**: Shows feedback after player choices
  - Text: Feedback content
  - Image: Visual indicator of response appropriateness
  - Initially hidden, shown after responses
  
- **Panel - Social Skills Meter**: Visual representation of social skills
  - Progress bars for different social skill categories
  - Text labels for each category
  
- **Panel - Settings**: Game settings
  - Toggle: Enable/disable webcam gaze tracking
  - Slider: Dialogue text speed
  - Dropdown: Difficulty level
  - Button: "Return to Main Menu"
  
- **Panel - Game Over**: End of session summary
  - Text: Session summary
  - Text: Skills improved
  - Text: Areas for improvement
  - Button: "Continue" or "Return to Main Menu"

## Setting Up Webcam Integration (Optional)

1. Create an empty GameObject named "GazeTracker"
2. Add the GazeTracker component
3. Configure webcam settings:
   - Camera device selection
   - Tracking frequency
   - Calibration settings

## Creating Dialogue System

1. Create a DialogueManager GameObject
2. Add the DialogueController component
3. Set up dialogue data structure:
   - Create a Dialogue scriptable object
   - Create a DialogueNode scriptable object for each dialogue entry
   - Create a ResponseOption scriptable object for player choices

## Adding Components to GameController Object

1. Create an empty GameObject named "GameController"
2. Add the following components:
   - SocialScopeController
   - ScenarioManager
   - FacialExpressionController
   - DialogueController
   - SocialResponseAnalyzer
   
3. Set up the SocialScopeController:
   - UI References: Connect to all UI elements
   - Difficulty Level: Initial difficulty setting
   - Enable Gaze Tracking: Boolean for webcam integration
   
4. Set up the ScenarioManager:
   - Scenario Data: Array of scenario scriptable objects
   - Environment References: References to different environment prefabs
   - Character References: References to different character prefabs
   
5. Set up the FacialExpressionController:
   - Character References: Connect to character models
   - Expression Presets: Define different facial expressions
   
6. Set up the DialogueController:
   - Dialogue Panel: Reference to dialogue UI panel
   - Response Panel: Reference to response options UI panel
   - Text Speed: Characters per second for text animation
   
7. Set up the SocialResponseAnalyzer:
   - Feedback Panel: Reference to feedback UI panel
   - Social Skills Meter: Reference to skills meter UI
   - Feedback Delay: Time before showing feedback

8. Add the NeuroplaysApiClient to the scene if not already present
   - Game Name: "SocialScope"

## Creating Scenario Scriptable Objects

1. Create a Scenario scriptable object (Create > ScriptableObjects > Scenario)
2. For each scenario, define:
   - Title: Name of the scenario
   - Description: Brief description
   - Environment: Which environment to use
   - Characters: Which characters to include
   - Dialogue Tree: Reference to starting dialogue node
   - Objectives: List of objectives to complete
   - Difficulty Level: 1-5 scale

## Testing in Unity

1. Enter Play mode to test the game
2. Navigate through the main menu
3. Test a scenario:
   - Ensure characters appear correctly
   - Check that dialogue displays properly
   - Test response options
   - Verify feedback system
   - Check social skills meter updates
4. Test webcam integration if enabled
5. Complete a scenario and verify the summary screen

## Building for Desktop

1. Set the build platform to Windows/Mac/Linux
2. Configure Player Settings:
   - Resolution settings
   - Quality settings
   - Input system configuration
3. Build and test the standalone application

## Backend Integration

The game automatically integrates with the Neuroplay backend using the NeuroplaysApiClient.
The following data is sent to the backend:

- Social scenario response choices
- Response time to social cues
- Gaze tracking data (if enabled)
- Social skill category performance metrics
- Engagement level with different scenario types
- Learning progression across social contexts

No additional configuration is needed beyond what's specified in this guide.