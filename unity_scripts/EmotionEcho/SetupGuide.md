# EmotionEcho - Unity Setup Guide

## Creating the Game Scene

1. Create a new scene in Unity (File > New Scene)
2. Save the scene as "EmotionEcho.unity"

## Setting Up the Environment

1. Create a minimalist, atmospheric environment:
   - Simple background scenes that change with story progression
   - Subtle ambient lighting that adjusts based on emotional context
   - Minimal distractions to keep focus on dialogue
   
2. Set up Audio Environment:
   - Add ambient background sounds
   - Create audio mixer with different emotional presets
   - Configure reverb zones for voice processing

## Setting Up Character Visualization

1. Create or import character portraits/avatars:
   - Main character (player representation)
   - NPC characters for dialogue
   - Different emotional states for each character
   
2. Alternatively, use abstract visual representations:
   - Color patterns that change with emotions
   - Particle systems that respond to voice tone
   - Animated shapes that transform based on dialogue

## Setting Up UI

1. Create a Canvas (GameObject > UI > Canvas)
2. Add an EventSystem if not already present (GameObject > UI > Event System)
3. Inside the Canvas, create the following UI elements:

### Main UI Elements

- **Panel - Main Menu**: Initial menu
  - Title: "Emotion Echo"
  - Button: "New Story"
  - Button: "Continue Story"
  - Button: "Settings"
  - Button: "Credits"
  
- **Panel - Dialogue**: Main gameplay interface
  - Image: Character portrait/visualization
  - Text: Character name
  - Text: Dialogue content
  - Image: Voice input indicator
  - Text: Transcribed player response (when speaking)
  - Button: "Skip" (optional)
  
- **Panel - Choices**: Appears when manual choices are needed
  - Multiple buttons for different dialogue options
  - Only shown when voice input is not used or as fallback
  
- **Panel - Emotion Display**: Visual representation of detected emotions
  - Image: Emotion icon
  - Text: Detected emotion label
  - Slider: Emotion intensity
  - Can be minimized or expanded
  
- **Panel - Settings**: Game settings
  - Toggle: Voice Recognition On/Off
  - Slider: Voice Detection Sensitivity
  - Dropdown: Language Selection
  - Toggle: Show Emotion Analysis
  - Button: "Microphone Setup"
  - Button: "Return"
  
- **Panel - Microphone Setup**: Microphone configuration
  - Dropdown: Microphone Device Selection
  - Button: "Test Microphone"
  - Slider: Microphone Volume
  - Text: Status message
  - Button: "Save Settings"
  
- **Panel - Story End**: Appears at story conclusion
  - Text: Ending description
  - Text: Emotional journey summary
  - Button: "New Story"
  - Button: "Main Menu"

## Setting Up Voice Recognition

1. Import required packages:
   - Unity's built-in Microphone class
   - Consider third-party assets for enhanced voice recognition
   
2. Create an empty GameObject named "VoiceManager"
3. Add the VoiceRecognitionManager component
4. Configure microphone settings:
   - Sample rate: 44100 Hz
   - Recording length: 10 seconds (rolling buffer)
   - Auto restart: true

## Creating Dialogue System

1. Create a DialogueSystem GameObject
2. Add the DialogueManager component
3. Set up dialogue data structure:
   - Create a DialogueNode scriptable object for each conversation point
   - Create an EmotionalResponse scriptable object for different emotion paths
   - Create a StoryBranch scriptable object for major narrative branches

## Setting Up Emotion Analysis

1. Create an EmotionAnalysis GameObject
2. Add the EmotionAnalyzer component
3. Configure emotion detection settings:
   - Text sentiment analysis sensitivity
   - Voice tone analysis parameters
   - Emotion categories and thresholds

## Adding Components to GameController Object

1. Create an empty GameObject named "GameController"
2. Add the following components:
   - EmotionEchoController
   - StoryManager
   - UIManager
   
3. Set up the EmotionEchoController:
   - Voice Manager: Reference to VoiceManager GameObject
   - Dialogue Manager: Reference to DialogueManager
   - Emotion Analyzer: Reference to EmotionAnalyzer
   - UI Manager: Reference to UIManager
   - Starting Node: Reference to first dialogue node
   
4. Set up the StoryManager:
   - Story Branches: Array of StoryBranch scriptable objects
   - Character References: References to character portraits/visualizations
   - Environment References: References to background scenes
   - Emotional Thresholds: Define when story branches based on emotions
   
5. Set up the UIManager:
   - References to all UI panels and elements
   - Animation controllers for UI transitions
   - Audio references for UI sounds

6. Add the NeuroplaysApiClient to the scene if not already present
   - Game Name: "EmotionEcho"

## Creating Story Content

1. Create a Story scriptable object (Create > ScriptableObjects > Story)
2. For each story point:
   - Create DialogueNode with:
     - Character speaking
     - Dialogue text
     - Audio clip (if using voice acting)
     - Expected emotional responses
     - Next nodes based on emotion
   
3. Create branching paths based on emotional responses:
   - Happy/positive path
   - Sad/negative path
   - Neutral/ambivalent path
   - Angry/frustrated path

## Testing in Unity

1. Enter Play mode to test the game
2. Test microphone input:
   - Verify that voice is detected
   - Check transcription accuracy
   - Test emotion detection from voice
3. Test dialogue system:
   - Ensure dialogue displays correctly
   - Verify branching based on detected emotions
   - Test manual choice fallback
4. Test a complete story path:
   - Play through from beginning to end
   - Try different emotional responses
   - Verify different endings are reached
5. Test settings functionality:
   - Microphone selection
   - Voice recognition toggles
   - Language options

## Building for Desktop

1. Set the build platform to Windows/Mac/Linux
2. Configure microphone permissions
3. Include necessary third-party DLLs for voice recognition
4. Build and test the standalone application

## Backend Integration

The game automatically integrates with the Neuroplay backend using the NeuroplaysApiClient.
The following data is sent to the backend:

- Voice recordings (converted to text)
- Detected emotional states during gameplay
- Conversation choices and pathways
- Session duration and engagement metrics
- Local emotion analysis results for server-side validation

No additional configuration is needed beyond what's specified in this guide.

## Privacy Considerations

Since this game collects voice data and performs emotion analysis, consider adding:

1. A privacy notice at first launch
2. Clear information about data collection
3. Option to delete recorded voice data
4. Consent form for sending data to the backend
5. Option to play without voice recognition (text-only mode)