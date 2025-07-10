import numpy as np
import pandas as pd
from rest_framework.views import APIView
from rest_framework.response import Response
from rest_framework import status
from rest_framework.permissions import IsAuthenticated
from api.models import PlayerProfile, GameSession


class PlayerStatsView(APIView):
    """
    Get overall statistics for a player across all games
    """
    permission_classes = [IsAuthenticated]
    
    def get(self, request, player_id):
        try:
            player = PlayerProfile.objects.get(id=player_id)
            sessions = GameSession.objects.filter(player=player)
            
            if not sessions.exists():
                return Response({
                    "message": "No game sessions found for this player"
                }, status=status.HTTP_200_OK)
            
            # Convert to DataFrame for easier analysis
            sessions_data = list(sessions.values())
            df = pd.DataFrame(sessions_data)
            
            # Calculate statistics
            stats = {
                "total_sessions": len(df),
                "total_games_played": df['game_name'].nunique(),
                "total_playtime_minutes": df['duration_minutes'].sum(),
                "average_session_duration": df['duration_minutes'].mean(),
                "average_score": df['score'].mean(),
                "highest_score": df['score'].max(),
                "games_completed": df['completed'].sum(),
                "completion_rate": (df['completed'].sum() / len(df)) * 100,
                "game_distribution": df['game_name'].value_counts().to_dict(),
                "recent_activity": df.sort_values('start_time', ascending=False).head(5)[
                    ['game_name', 'start_time', 'score', 'difficulty_level']
                ].to_dict('records')
            }
            
            return Response(stats, status=status.HTTP_200_OK)
            
        except PlayerProfile.DoesNotExist:
            return Response({"error": "Player not found"}, status=status.HTTP_404_NOT_FOUND)
        except Exception as e:
            return Response({"error": str(e)}, status=status.HTTP_500_INTERNAL_SERVER_ERROR)


class GamePerformanceView(APIView):
    """
    Get detailed performance metrics for a specific game and player
    """
    permission_classes = [IsAuthenticated]
    
    def get(self, request, game_name, player_id):
        try:
            player = PlayerProfile.objects.get(id=player_id)
            sessions = GameSession.objects.filter(player=player, game_name=game_name)
            
            if not sessions.exists():
                return Response({
                    "message": f"No sessions found for game {game_name} and player {player.user.username}"
                }, status=status.HTTP_200_OK)
            
            # Convert to DataFrame for analysis
            sessions_data = list(sessions.values())
            df = pd.DataFrame(sessions_data)
            
            # Extract detailed session data
            df['session_data'] = df['session_data'].apply(lambda x: x if x else {})
            
            # Calculate performance metrics based on game type
            if game_name == "NeuroSprint":
                return self._analyze_neurosprint(df)
            elif game_name == "EmotionEcho":
                return self._analyze_emotion_echo(df)
            elif game_name == "MemoryMaze":
                return self._analyze_memory_maze(df)
            elif game_name == "BalanceBot":
                return self._analyze_balance_bot(df)
            elif game_name == "SocialScope":
                return self._analyze_social_scope(df)
            else:
                # Generic analysis for any game
                return self._analyze_generic(df, game_name)
                
        except PlayerProfile.DoesNotExist:
            return Response({"error": "Player not found"}, status=status.HTTP_404_NOT_FOUND)
        except Exception as e:
            return Response({"error": str(e)}, status=status.HTTP_500_INTERNAL_SERVER_ERROR)
    
    def _analyze_generic(self, df, game_name):
        """Generic analysis for any game"""
        performance = {
            "game_name": game_name,
            "total_sessions": len(df),
            "total_playtime_minutes": df['duration_minutes'].sum(),
            "average_session_duration": df['duration_minutes'].mean(),
            "average_score": df['score'].mean(),
            "highest_score": df['score'].max(),
            "score_progression": df.sort_values('start_time')[['start_time', 'score']].to_dict('records'),
            "completion_rate": (df['completed'].sum() / len(df)) * 100,
            "difficulty_distribution": df['difficulty_level'].value_counts().to_dict()
        }
        return Response(performance, status=status.HTTP_200_OK)
    
    def _analyze_neurosprint(self, df):
        """Specific analysis for NeuroSprint game"""
        # Extract relevant metrics from session_data
        reaction_times = []
        attention_scores = []
        obstacle_counts = []
        
        for _, row in df.iterrows():
            session = row['session_data']
            if 'reaction_times' in session:
                reaction_times.extend(session['reaction_times'])
            if 'attention_score' in session:
                attention_scores.append(session['attention_score'])
            if 'obstacles_encountered' in session:
                obstacle_counts.append(session['obstacles_encountered'])
        
        performance = {
            "game_name": "NeuroSprint",
            "total_sessions": len(df),
            "average_reaction_time": np.mean(reaction_times) if reaction_times else None,
            "reaction_time_trend": self._calculate_trend(reaction_times) if reaction_times else None,
            "average_attention_score": np.mean(attention_scores) if attention_scores else None,
            "attention_consistency": np.std(attention_scores) if attention_scores else None,
            "average_obstacles_per_session": np.mean(obstacle_counts) if obstacle_counts else None,
            "score_progression": df.sort_values('start_time')[['start_time', 'score']].to_dict('records'),
        }
        return Response(performance, status=status.HTTP_200_OK)
    
    def _analyze_emotion_echo(self, df):
        """Specific analysis for EmotionEcho game"""
        # Implementation would extract emotion-related metrics
        # This is a placeholder for the actual implementation
        return Response({"message": "Emotion Echo analysis not yet implemented"}, status=status.HTTP_501_NOT_IMPLEMENTED)
    
    def _analyze_memory_maze(self, df):
        """Specific analysis for MemoryMaze game"""
        # Implementation would extract memory-related metrics
        # This is a placeholder for the actual implementation
        return Response({"message": "Memory Maze analysis not yet implemented"}, status=status.HTTP_501_NOT_IMPLEMENTED)
    
    def _analyze_balance_bot(self, df):
        """Specific analysis for BalanceBot game"""
        # Implementation would extract motor skill metrics
        # This is a placeholder for the actual implementation
        return Response({"message": "Balance Bot analysis not yet implemented"}, status=status.HTTP_501_NOT_IMPLEMENTED)
    
    def _analyze_social_scope(self, df):
        """Specific analysis for SocialScope game"""
        # Implementation would extract social interaction metrics
        # This is a placeholder for the actual implementation
        return Response({"message": "Social Scope analysis not yet implemented"}, status=status.HTTP_501_NOT_IMPLEMENTED)
    
    def _calculate_trend(self, values):
        """Calculate linear trend in a series of values"""
        if not values or len(values) < 2:
            return 0
        
        x = np.arange(len(values))
        y = np.array(values)
        
        # Linear regression to find slope
        slope, _ = np.polyfit(x, y, 1)
        return slope


# Specialized analytics views for each health domain
class ADHDAnalyticsView(APIView):
    """
    Analytics specific to ADHD tracking through NeuroSprint game
    """
    permission_classes = [IsAuthenticated]
    
    def get(self, request, player_id):
        # Implementation would focus on attention metrics
        return Response({"message": "ADHD analytics not yet implemented"}, status=status.HTTP_501_NOT_IMPLEMENTED)


class EmotionAnalyticsView(APIView):
    """
    Analytics specific to emotional health tracking through EmotionEcho game
    """
    permission_classes = [IsAuthenticated]
    
    def get(self, request, player_id):
        # Implementation would focus on sentiment analysis
        return Response({"message": "Emotion analytics not yet implemented"}, status=status.HTTP_501_NOT_IMPLEMENTED)


class MemoryAnalyticsView(APIView):
    """
    Analytics specific to memory and cognitive tracking through MemoryMaze game
    """
    permission_classes = [IsAuthenticated]
    
    def get(self, request, player_id):
        # Implementation would focus on memory recall metrics
        return Response({"message": "Memory analytics not yet implemented"}, status=status.HTTP_501_NOT_IMPLEMENTED)


class MotorSkillsAnalyticsView(APIView):
    """
    Analytics specific to motor skills tracking through BalanceBot game
    """
    permission_classes = [IsAuthenticated]
    
    def get(self, request, player_id):
        # Implementation would focus on motor control metrics
        return Response({"message": "Motor skills analytics not yet implemented"}, status=status.HTTP_501_NOT_IMPLEMENTED)


class ASDAnalyticsView(APIView):
    """
    Analytics specific to ASD tracking through SocialScope game
    """
    permission_classes = [IsAuthenticated]
    
    def get(self, request, player_id):
        # Implementation would focus on social interaction metrics
        return Response({"message": "ASD analytics not yet implemented"}, status=status.HTTP_501_NOT_IMPLEMENTED)