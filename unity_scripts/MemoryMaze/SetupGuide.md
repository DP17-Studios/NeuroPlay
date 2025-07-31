# MemoryMaze - Unity Setup Guide

## Creating the Game Scene

1. Create a new scene in Unity (File > New Scene)
2. Save the scene as "MemoryMaze.unity"

## Setting Up the Environment

1. Create a basic grid system for maze generation:
   - Create an empty GameObject named "MazeGrid"
   - Position it at (0,0,0) in the scene
   
2. For 2D Mode:
   - Set up an orthographic camera looking down at the grid
   - Create 2D sprite assets for walls, paths, and markers
   - Set up appropriate 2D lighting
   
3. For 3D Mode:
   - Set up a perspective camera
   - Create 3D models for walls, paths, and landmarks
   - Set up appropriate 3D lighting with shadows
   - Add post-processing effects for visual clarity

## Creating Grid Cell Prefabs

1. Create the following prefabs:
   - **Wall Cell**: Obstacle that blocks movement
     - 2D: Sprite with Box Collider 2D
     - 3D: Cube with Box Collider
   
   - **Path Cell**: Walkable area
     - 2D: Sprite with no collider
     - 3D: Plane or cube with no collider or trigger collider
   
   - **Start Cell**: Starting position
     - Distinct visual appearance
     - Tag as "StartPosition"
   
   - **Goal Cell**: Destination
     - Distinct visual appearance
     - Tag as "GoalPosition"
   
   - **Marker Cell**: Visual cue for memory tasks
     - Distinct colors or symbols
     - Can be toggled on/off

2. Add the GridCell component to each prefab
3. Configure the properties for each cell type

## Setting Up Player Character

1. Create a player character:
   - For 2D: Sprite with appropriate animations
   - For 3D: Simple character model or first-person controller
   
2. Add the following components:
   - Rigidbody (2D or 3D depending on mode)
   - Collider (2D or 3D depending on mode)
   - Player Controller script
   
3. Configure player movement:
   - Movement speed
   - Rotation speed (for 3D)
   - Input controls

## Setting Up UI

1. Create a Canvas (GameObject > UI > Canvas)
2. Add an EventSystem if not already present (GameObject > UI > Event System)
3. Inside the Canvas, create the following UI elements:

### Main UI Elements

- **Panel - Main Menu**: Initial menu
  - Title: "Memory Maze"
  - Button: "Start Game"
  - Button: "Tutorial"
  - Button: "Settings"
  - Button: "Exit"
  
- **Panel - Game Mode**: Game mode selection
  - Button: "2D Mode"
  - Button: "3D Mode"
  - Button: "Back"
  
- **Panel - Difficulty**: Difficulty selection
  - Button: "Easy"
  - Button: "Medium"
  - Button: "Hard"
  - Button: "Adaptive"
  - Button: "Back"
  
- **Panel - Game HUD**: In-game heads-up display
  - Text: "Level"
  - Text: Level number
  - Text: "Time"
  - Text: Timer display
  - Text: "Score"
  - Text: Current score
  
- **Panel - Pattern Display**: Shows patterns to memorize
  - Grid of cells/images for pattern display
  - Text: Instructions
  - Text: Countdown timer
  
- **Panel - Sequence Display**: Shows sequences to memorize
  - Series of images or symbols
  - Text: Instructions
  - Text: Countdown timer
  
- **Panel - Level Complete**: End of level summary
  - Text: "Level Complete"
  - Text: Time taken
  - Text: Errors made
  - Text: Score for level
  - Button: "Next Level"
  - Button: "Main Menu"
  
- **Panel - Game Over**: End of game summary
  - Text: "Game Over"
  - Text: Final score
  - Text: Performance summary
  - Text: Memory performance metrics
  - Button: "Play Again"
  - Button: "Main Menu"
  
- **Panel - Settings**: Game settings
  - Toggle: Sound effects
  - Toggle: Background music
  - Slider: Volume
  - Toggle: Visual aids
  - Dropdown: Color scheme (for accessibility)
  - Button: "Save Settings"
  - Button: "Return"

## Creating Challenge Types

1. Create Pattern Recognition Challenges:
   - Create a grid of cells that light up in patterns
   - Allow player to reproduce the pattern
   - Track accuracy and response time
   
2. Create Sequence Memorization Challenges:
   - Display a sequence of symbols or actions
   - Allow player to reproduce the sequence
   - Track accuracy and response time
   
3. Create Navigation Challenges:
   - Generate mazes of varying complexity
   - Add landmarks or visual cues
   - Track navigation efficiency and errors

## Adding Components to GameController Object

1. Create an empty GameObject named "GameController"
2. Add the following components:
   - MemoryMazeController
   - MazeGenerator
   - PatternManager
   - SequenceManager
   - NavigationTracker
   - DifficultySetting
   - UIManager
   
3. Set up the MemoryMazeController:
   - Player: Reference to player character
   - UI Manager: Reference to UIManager
   - Game Mode: 2D or 3D
   - Initial Difficulty: Starting difficulty level
   
4. Set up the MazeGenerator:
   - Grid Size: Initial size of maze (e.g., 10x10)
   - Cell Prefabs: References to wall, path, start, goal prefabs
   - Maze Parent: Reference to MazeGrid GameObject
   - Generation Algorithm: Choose between algorithms (e.g., Recursive Backtracker, Prim's)
   
5. Set up the PatternManager:
   - Pattern Grid: Reference to pattern display UI
   - Pattern Sizes: Array of pattern dimensions for different difficulties
   - Display Duration: How long patterns are shown
   - Response Timeout: Maximum time allowed for response
   
6. Set up the SequenceManager:
   - Sequence Elements: Array of symbols or images
   - Sequence Lengths: Array of sequence lengths for different difficulties
   - Display Duration: How long sequences are shown
   - Response Timeout: Maximum time allowed for response
   
7. Set up the NavigationTracker:
   - Track Player: Reference to player character
   - Record Interval: How often to record position (seconds)
   - Path Visualization: Whether to show the player's path
   
8. Set up the DifficultySetting:
   - Difficulty Levels: Array of difficulty presets
   - Adaptive Parameters: Settings for adaptive difficulty
   - Performance Metrics: Which metrics affect difficulty

9. Add the NeuroplaysApiClient to the scene if not already present
   - Game Name: "MemoryMaze"

## Testing in Unity

1. Enter Play mode to test the game
2. Test the main menu navigation
3. Try both 2D and 3D modes
4. Test each challenge type:
   - Pattern recognition
   - Sequence memorization
   - Maze navigation
5. Verify scoring and timing functionality
6. Test level progression
7. Check that performance data is being recorded correctly

## Building for Different Platforms

### For Desktop:
1. Set the build platform to Windows/Mac/Linux
2. Configure keyboard and mouse controls
3. Test with various screen resolutions
4. Build and test the standalone application

### For Mobile:
1. Set the build platform to Android or iOS
2. Configure touch controls
3. Optimize UI for touch screens
4. Test on actual devices
5. Build and deploy to your device

## Backend Integration

The game automatically integrates with the Neuroplay backend using the NeuroplaysApiClient.
The following data is sent to the backend:

- Maze completion time and accuracy
- Pattern recognition success rate
- Sequence recall precision
- Navigation path data (compared to optimal paths)
- Error frequency and types
- Response time for memory tasks

No additional configuration is needed beyond what's specified in this guide.

## Accessibility Considerations

Since this game is designed for dementia and memory rehabilitation:

1. Include high-contrast color options
2. Provide clear, simple instructions
3. Add visual and audio cues for important events
4. Include options to adjust game speed
5. Provide alternative control schemes
6. Consider adding a "helper mode" with additional guidance