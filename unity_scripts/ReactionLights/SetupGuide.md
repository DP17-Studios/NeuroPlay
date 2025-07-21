# ReactionLights - Unity Setup Guide

## Creating the Game Scene

1. Create a new scene in Unity (File > New Scene)
2. Save the scene as "ReactionLights.unity"

## Setting Up UI

1. Create a Canvas (GameObject > UI > Canvas)
2. Add an EventSystem if not already present (GameObject > UI > Event System)
3. Inside the Canvas, create the following UI elements:

### Main UI Elements

- **Panel**: Background panel (full screen)
  - Color: Dark Gray (#222222)
  
- **Text - Title**: Main title text
  - Text: "Reaction Lights"
  - Font Size: 48
  - Color: White
  - Position: Top center
  
- **Text - Instructions**: Instructions text
  - Text: "Press Start to begin"
  - Font Size: 24
  - Color: White
  - Position: Center
  
- **Text - Attempt Counter**: Shows current attempt
  - Text: "Attempt 0 of 3"
  - Font Size: 18
  - Color: White
  - Position: Top right
  
- **Text - Result**: Shows reaction time result
  - Text: ""
  - Font Size: 36
  - Color: White
  - Position: Below lights
  - Initially disabled
  
- **Text - Average**: Shows average reaction time
  - Text: ""
  - Font Size: 24
  - Color: White
  - Position: Below result text
  - Initially disabled
  
- **Button - Start**: Start button
  - Text: "Start"
  - Position: Bottom center
  
- **Image - TouchPanel**: Invisible full-screen touch area
  - Color: Transparent
  - Position: Center (full screen)
  - Add Button component with no text
  
- **Empty GameObject - LightsContainer**: Container for light objects
  - Position: Center

## Creating Light Prefab

1. Create a new UI Image (GameObject > UI > Image)
2. Rename to "LightPrefab"
3. Set its properties:
   - Shape: Circle
   - Size: 50x50
   - Color: White (will be changed by script)
4. Create a Prefab from this object
5. Delete the original from the scene

## Adding Components to GameController Object

1. Create an empty GameObject named "GameController"
2. Add the following components:
   - ReactionLightsController
   - ReactionUIManager
   - LightSequenceManager
   - ReactionAnalytics
   
3. Set up the ReactionUIManager:
   - Main Canvas: Reference to your Canvas
   - Lights Container: Reference to LightsContainer
   - Light Prefab: Reference to your LightPrefab
   - Reference all text elements and buttons
   
4. Set up the ReactionLightsController:
   - Attempts Per Session: 3
   - Min Red Light Duration: 5
   - Max Red Light Duration: 10
   - Number Of Lights: 5
   
5. Add the NeuroplaysApiClient to the scene if not already present
   - Game Name: "ReactionLights"

## Setting Up Touch Input

1. Add the ReactionInputHandler component to the TouchPanel object
2. In the Inspector, ensure the following:
   - Allow Touch Input: true
   - Allow Mouse Input: true
   - Allow Keyboard Input: true
   - React Key: Space

## Testing in Unity

1. Enter Play mode to test the game
2. Click "Start" to begin the light sequence
3. The 5 red lights should appear one by one
4. After a random delay (5-10 seconds), the lights turn green
5. Click/tap as quickly as possible
6. Your reaction time will be displayed
7. The game will proceed through 3 attempts
8. At the end, your average reaction time will be shown

## Building for Mobile

1. Set the build platform to Android or iOS
2. Configure Player Settings for mobile
3. Ensure the game works in portrait mode
4. Build and deploy to your device

## Backend Integration

The game automatically integrates with the Neuroplay backend using the existing NeuroplaysApiClient.
No additional configuration is needed beyond what's specified in this guide.