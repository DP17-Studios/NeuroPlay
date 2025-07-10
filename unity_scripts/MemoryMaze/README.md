# Memory Maze - Dementia Cognitive Tracker

## Overview
Memory Maze is a 2D/3D maze navigation game designed for dementia and memory rehabilitation. The game challenges players to remember patterns, sequences, and routes with timed and distraction-based challenges.

## Game Mechanics
- **2D/3D Maze Navigation**: Players navigate through increasingly complex mazes
- **Pattern Recognition**: Remember and reproduce visual or spatial patterns
- **Sequence Memorization**: Recall and repeat sequences of actions or objects
- **Timed Challenges**: Complete tasks within time constraints
- **Distraction Elements**: Focus on tasks despite visual or auditory distractions

## Backend Features
- **Memory Recall Accuracy Trends**: Tracks improvement or decline in memory function
- **Navigation Efficiency Mapping**: Analyzes path-finding strategies and efficiency
- **Error Heatmap**: Visualizes where and when memory errors occur
- **Historical Progress Charts**: Shows long-term cognitive performance

## AI Analysis
- Uses numpy, seaborn, and time-series models for data analysis
- Identifies patterns in memory performance over time
- Detects early warning signs of cognitive decline
- Provides personalized difficulty adjustment based on performance

## Files in this Directory
- `MemoryMazeController.cs`: Main game controller script
- `MazeGenerator.cs`: Procedurally generates maze layouts
- `PatternManager.cs`: Creates and validates pattern recognition challenges
- `SequenceManager.cs`: Manages sequence memorization tasks
- `NavigationTracker.cs`: Records player movement and navigation choices
- `DifficultySetting.cs`: Adjusts game difficulty based on performance
- `UIManager.cs`: Manages game interface elements

## Integration with Backend
The game sends the following data to the backend:
- Maze completion time and accuracy
- Pattern recognition success rate
- Sequence recall precision
- Navigation path data (compared to optimal paths)
- Error frequency and types
- Response time for memory tasks