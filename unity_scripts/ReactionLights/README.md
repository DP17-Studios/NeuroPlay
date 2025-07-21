# ReactionLights - F1-Style Reaction Time Game

## Overview
ReactionLights is a reaction time testing game based on Formula 1 start lights. The game presents 5 red lights that turn green after a random period, challenging players to react as quickly as possible when the lights turn green.

## Game Mechanics
- **F1-Style Light Sequence**: 5 red lights appear and then turn green
- **Reaction Testing**: Players must tap the screen when lights turn green
- **False Start Detection**: Disqualifies players who tap before green lights
- **Random Timing**: Red light duration varies randomly between 5-10 seconds
- **Multiple Attempts**: Each session consists of 3 attempts to collect consistent data

## Backend Features
- **Reaction Time Logging**: Measures millisecond-precise reaction times
- **False Start Tracking**: Records premature reactions
- **Statistical Analysis**: Averages and trends across multiple attempts
- **Performance Comparison**: Benchmarks against population averages

## AI Analysis
- Identifies reaction time patterns and consistency
- Detects attention and focus issues
- Provides personalized feedback based on performance

## Files in this Directory
- `ReactionLightsController.cs`: Main game controller script
- `LightSequenceManager.cs`: Manages the F1-style light sequence
- `ReactionInputHandler.cs`: Handles player input and timing
- `ReactionAnalytics.cs`: Manages data collection and analysis
- `ReactionUIManager.cs`: Manages game interface elements

## Integration with Backend
The game sends the following data to the backend:
- Reaction time for each attempt (in milliseconds)
- False start occurrences
- Average reaction time across the session
- Best reaction time
- Session date and time