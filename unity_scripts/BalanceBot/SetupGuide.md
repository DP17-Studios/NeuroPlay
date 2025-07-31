# BalanceBot - Unity Setup Guide

## Creating the Game Scene

1. Create a new scene in Unity (File > New Scene)
2. Save the scene as "BalanceBot.unity"

## Setting Up the Physics Environment

1. Create a physics-based environment:
   - Create a flat platform as the main balance surface
   - Add environmental elements that can affect balance (wind zones, moving platforms)
   - Set up appropriate lighting for good visibility
   
2. Configure Physics Settings:
   - Edit > Project Settings > Physics
   - Set gravity to (0, -9.81, 0)
   - Adjust Time Manager for consistent physics simulation
   - Enable "Auto Simulation" for physics

## Setting Up the Robot Character

1. Create or import a robot model:
   - The robot should have articulated parts
   - Center of mass should be adjustable
   - Add appropriate colliders to all parts
   
2. Add the following components to the robot:
   - Rigidbody (Use realistic mass and drag settings)
   - Character Joint components for articulated parts
   - RobotController script
   
3. Configure the robot's physical properties:
   - Set mass distribution for realistic balance challenges
   - Adjust joint limits for natural movement
   - Set up physics materials for appropriate friction

## Setting Up UI

1. Create a Canvas (GameObject > UI > Canvas)
2. Add an EventSystem if not already present (GameObject > UI > Event System)
3. Inside the Canvas, create the following UI elements:

### Main UI Elements

- **Panel - Main Menu**: Initial menu
  - Title: "BalanceBot"
  - Button: "Start Game"
  - Button: "Tutorial"
  - Button: "Settings"
  - Button: "Exit"
  
- **Panel - Game HUD**: In-game heads-up display
  - Text: "Balance Score"
  - Slider: Balance meter
  - Text: "Time Remaining"
  - Text: Timer display
  - Text: "Current Level"
  - Text: Level indicator
  
- **Panel - Balance Indicators**: Visual feedback on balance state
  - Image: Center of gravity indicator
  - Image: Balance threshold boundaries
  - Image: Direction indicators for tilt
  
- **Panel - Control Method**: Input method selection
  - Toggle: Keyboard
  - Toggle: Mouse
  - Toggle: Tilt Sensor (if available)
  - Button: "Apply"
  
- **Panel - Task Instructions**: Shows current task information
  - Text: Task title
  - Text: Task description
  - Image: Visual instruction (if applicable)
  
- **Panel - Results**: Shows performance after each level
  - Text: "Level Complete"
  - Text: Balance score
  - Text: Time taken
  - Text: Precision rating
  - Button: "Next Level"
  - Button: "Retry"
  - Button: "Main Menu"
  
- **Panel - Settings**: Game settings
  - Slider: Sensitivity adjustment
  - Toggle: Visual aids
  - Dropdown: Difficulty preset
  - Button: "Save Settings"
  - Button: "Return"

## Setting Up Input Systems

1. Configure Input Manager (Edit > Project Settings > Input Manager)
2. Set up the following input axes:
   - Horizontal: For left/right balance control
   - Vertical: For forward/backward balance control
   - Balance: For fine balance adjustments
   
3. For mobile/tablet support:
   - Add accelerometer input handling
   - Configure tilt sensitivity settings
   - Add touch input options

## Creating Task Levels

1. Create a TaskLevel scriptable object (Create > ScriptableObjects > TaskLevel)
2. For each level, define:
   - Level Name: Descriptive name
   - Difficulty: 1-10 scale
   - Duration: Time limit in seconds
   - Balance Threshold: How precise the balance must be
   - Environmental Factors: Wind, platform movement, etc.
   - Success Criteria: Balance time, precision requirements

## Adding Components to GameController Object

1. Create an empty GameObject named "GameController"
2. Add the following components:
   - BalanceBotController
   - PhysicsSimulator (or reference Unity's built-in physics)
   - InputManager
   - TaskGenerator
   - MotionAnalyzer
   - ProgressTracker
   
3. Set up the BalanceBotController:
   - Robot Reference: Link to your robot GameObject
   - Task Levels: Array of TaskLevel scriptable objects
   - UI References: Connect to all UI elements
   - Initial Difficulty: Starting difficulty level
   
4. Set up the InputManager:
   - Keyboard Sensitivity: Adjustment for keyboard input
   - Mouse Sensitivity: Adjustment for mouse input
   - Tilt Sensitivity: Adjustment for accelerometer input
   - Input Method: Default input method
   
5. Set up the TaskGenerator:
   - Task Levels: Reference to the same array of levels
   - Environment Elements: References to wind zones, moving platforms, etc.
   
6. Set up the MotionAnalyzer:
   - Sampling Rate: How often to record motion data (e.g., 10 samples/second)
   - Analysis Window: Time window for motion analysis
   - Precision Thresholds: Defines what constitutes good/bad precision
   
7. Set up the ProgressTracker:
   - Session Data: Structure to store performance data
   - Improvement Metrics: Define how improvement is measured
   - Reward Thresholds: When to award tokens/achievements

8. Add the NeuroplaysApiClient to the scene if not already present
   - Game Name: "BalanceBot"

## Testing in Unity

1. Enter Play mode to test the game
2. Test the main menu navigation
3. Try different input methods:
   - Keyboard: Arrow keys for balance
   - Mouse: Mouse movement for balance
   - Tilt (on mobile): Device tilting for balance
4. Test a complete level cycle:
   - Level start
   - Balance maintenance
   - Level completion
   - Results screen
   - Progression to next level
5. Verify that balance data is being recorded correctly
6. Test the settings menu functionality

## Building for Different Platforms

### For Desktop:
1. Set the build platform to Windows/Mac/Linux
2. Configure keyboard and mouse controls
3. Test with various input devices
4. Build and test the standalone application

### For Mobile:
1. Set the build platform to Android or iOS
2. Configure accelerometer input
3. Optimize UI for touch screens
4. Test on actual devices
5. Build and deploy to your device

## Backend Integration

The game automatically integrates with the Neuroplay backend using the NeuroplaysApiClient.
The following data is sent to the backend:

- Motion control precision metrics
- Balance maintenance duration and quality
- Task completion success rates
- Input method effectiveness comparison
- Motor skill improvement trends
- Session duration and engagement levels

No additional configuration is needed beyond what's specified in this guide.