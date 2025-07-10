# NeuroSprint - Reaction & Focus Tracker

## Overview
NeuroSprint is an endless runner game designed to train attention and measure reaction time for users with ADHD or attention challenges. The game features visual and audio-based obstacle dodging with dynamic difficulty adjustment.

## Game Mechanics
- **Endless Runner Format**: Player character automatically moves forward
- **Obstacle Dodging**: Player must avoid obstacles by jumping, sliding, or moving side to side
- **Distractions**: Visual and audio distractions appear to test focus
- **Dynamic Difficulty**: Game adjusts difficulty based on player performance

## Backend Features
- **Reaction Time Data Logging**: Measures how quickly players respond to obstacles
- **Attention Consistency Scoring**: Tracks focus over time during gameplay
- **Focus and Accuracy Leaderboards**: Competitive elements to encourage engagement
- **Time-Window Anomaly Detection**: Identifies patterns that may indicate ADHD

## AI Analysis
- Uses pandas, scikit-learn, and matplotlib for data analysis
- Tracks reaction time trends over multiple sessions
- Identifies attention lapses and consistency issues
- Provides personalized feedback based on performance patterns

## Files in this Directory
- `NeuroSprintController.cs`: Main game controller script
- `ObstacleManager.cs`: Manages obstacle spawning and behavior
- `DistractionManager.cs`: Controls visual and audio distractions
- `PlayerController.cs`: Handles player input and movement
- `ScoreManager.cs`: Tracks and calculates player performance metrics
- `UIManager.cs`: Manages game interface elements

## Integration with Backend
The game sends the following data to the backend:
- Reaction times for each obstacle
- Attention score throughout gameplay
- Obstacle avoidance success rate
- Distraction response data
- Overall score and session duration