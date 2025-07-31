# NeuroSprint - Unity Setup Guide

## Creating the Game Scene

1. Create a new scene in Unity (File > New Scene)
2. Save the scene as "NeuroSprint.unity"

## Setting Up the Environment

1. Create a 3D environment with:
   - A long, straight path for the runner
   - Background scenery elements
   - Lighting setup (directional light)

2. Create the following empty GameObjects as containers:
   - "Environment" - parent for all environment objects
   - "Obstacles" - parent for all obstacle objects
   - "Distractions" - parent for visual distraction elements

## Setting Up the Player Character

1. Import a humanoid character model or use Unity's default capsule
2. Position the character at the start of the path
3. Add a Rigidbody component to the character
4. Add a Character Controller component
5. Create an Animator Controller for the character with the following states:
   - Idle
   - Running
   - Jump
   - Slide
   - Fall

## Setting Up UI

1. Create a Canvas (GameObject > UI > Canvas)
2. Add an EventSystem if not already present (GameObject > UI > Event System)
3. Inside the Canvas, create the following UI elements:

### Main UI Elements

- **Panel**: Background panel (full screen)
  - Color: Transparent
  
- **Text - Title**: Main title text
  - Text: "NeuroSprint"
  - Font Size: 48
  - Color: White
  - Position: Top center
  
- **Text - Score**: Score display
  - Text: "Score: 0"
  - Font Size: 24
  - Color: White
  - Position: Top right
  
- **Text - Distance**: Distance traveled
  - Text: "Distance: 0m"
  - Font Size: 24
  - Color: White
  - Position: Top left
  
- **Text - Reaction Time**: Shows last reaction time
  - Text: "Reaction: 0.00s"
  - Font Size: 18
  - Color: White
  - Position: Bottom left
  
- **Button - Start**: Start button
  - Text: "Start"
  - Position: Center
  - Only visible in menu state
  
- **Button - Pause**: Pause button
  - Text: "Pause"
  - Position: Top right corner
  - Only visible during gameplay
  
- **Panel - Game Over**: Game over panel
  - Initially disabled
  - Contains:
    - Text: "Game Over"
    - Text: Final score
    - Text: Average reaction time
    - Button: "Play Again"
    - Button: "Main Menu"

## Creating Obstacle Prefabs

1. Create at least 3 different obstacle types:
   - **Low Obstacle**: Requires sliding to avoid
     - Create a cube or custom model
     - Size: Appropriate for sliding under
     - Tag: "LowObstacle"
   
   - **High Obstacle**: Requires jumping to avoid
     - Create a cube or custom model
     - Size: Appropriate for jumping over
     - Tag: "HighObstacle"
   
   - **Side Obstacle**: Requires moving left/right to avoid
     - Create a cube or custom model
     - Size: Appropriate for side movement
     - Tag: "SideObstacle"

2. For each obstacle:
   - Add a Box Collider component
   - Mark as "Is Trigger"
   - Create a Prefab from this object
   - Delete the original from the scene

## Creating Distraction Prefabs

1. Create visual distraction prefabs:
   - Floating objects that appear in peripheral vision
   - Flashing UI elements
   - Color-changing background elements

2. Prepare audio distraction clips:
   - Short sound effects
   - Background noise
   - Voice distractions

## Adding Components to GameController Object

1. Create an empty GameObject named "GameController"
2. Add the following components:
   - NeuroSprintController
   - ObstacleManager
   - DistractionManager
   - ScoreManager
   - UIManager
   
3. Set up the NeuroSprintController:
   - Player: Reference to your player character
   - Initial Speed: 5
   - Max Speed: 15
   - Speed Increment: 0.1
   
4. Set up the ObstacleManager:
   - Obstacle Prefabs: Array of your obstacle prefabs
   - Min Spawn Distance: 10
   - Max Spawn Distance: 20
   - Initial Spawn Rate: 2
   - Min Spawn Rate: 0.5
   
5. Set up the DistractionManager:
   - Visual Distraction Prefabs: Array of your visual distraction prefabs
   - Audio Distraction Clips: Array of your audio distraction clips
   - Distraction Frequency: 15 (seconds between distractions)
   - Distraction Duration: 3 (seconds each distraction lasts)
   
6. Set up the ScoreManager:
   - Score Text: Reference to score UI text
   - Distance Text: Reference to distance UI text
   - Reaction Time Text: Reference to reaction time UI text
   
7. Set up the UIManager:
   - References to all UI elements
   - Game Over Panel: Reference to game over panel
   - Start Button: Reference to start button
   - Pause Button: Reference to pause button

8. Add the NeuroplaysApiClient to the scene if not already present
   - Game Name: "NeuroSprint"

## Setting Up Player Input

1. Add the PlayerController component to the player character
2. In the Inspector, ensure the following:
   - Jump Key: Space
   - Slide Key: LeftControl
   - Left Key: LeftArrow
   - Right Key: RightArrow
   - Jump Force: 8
   - Gravity Multiplier: 2.5

## Testing in Unity

1. Enter Play mode to test the game
2. Click "Start" to begin the game
3. The player character should start running automatically
4. Obstacles should spawn at regular intervals
5. Test all input controls:
   - Jump over high obstacles
   - Slide under low obstacles
   - Move left/right to avoid side obstacles
6. Verify that distractions appear periodically
7. Check that score and distance increase during gameplay
8. Force a collision to test the game over state

## Building for Mobile

1. Set the build platform to Android or iOS
2. Configure Player Settings for mobile
3. Add touch controls:
   - Swipe up: Jump
   - Swipe down: Slide
   - Swipe left/right: Move left/right
4. Test touch controls on device
5. Build and deploy to your device

## Backend Integration

The game automatically integrates with the Neuroplay backend using the NeuroplaysApiClient.
The following data is sent to the backend:

- Reaction times for each obstacle
- Attention score throughout gameplay
- Obstacle avoidance success rate
- Distraction response data
- Overall score and session duration

No additional configuration is needed beyond what's specified in this guide.