from rest_framework import viewsets, permissions, status
from rest_framework.views import APIView
from rest_framework.response import Response
from django.shortcuts import get_object_or_404
from .models import NeuroSprintSession, NeuroSprintProgress, NeuroSprintRecommendation
from .serializers import (
    NeuroSprintSessionSerializer, 
    NeuroSprintProgressSerializer,
    NeuroSprintRecommendationSerializer
)
from api.models import PlayerProfile, GameSession
import numpy as np
from datetime import datetime


class NeuroSprintSessionViewSet(viewsets.ModelViewSet):
    """
    API endpoint for NeuroSprint sessions
    """
    queryset = NeuroSprintSession.objects.all()
    serializer_class = NeuroSprintSessionSerializer
    permission_classes = [permissions.IsAuthenticated]
    
    def get_queryset(self):
        """
        Optionally filter by player
        """
        queryset = NeuroSprintSession.objects.all()
        player_id = self.request.query_params.get('player_id', None)
        
        if player_id:
            queryset = queryset.filter(session__player_id=player_id)
            
        return queryset


class NeuroSprintProgressViewSet(viewsets.ModelViewSet):
    """
    API endpoint for NeuroSprint progress
    """
    queryset = NeuroSprintProgress.objects.all()
    serializer_class = NeuroSprintProgressSerializer
    permission_classes = [permissions.IsAuthenticated]
    
    def get_queryset(self):
        """
        Optionally filter by player
        """
        queryset = NeuroSprintProgress.objects.all()
        player_id = self.request.query_params.get('player_id', None)
        
        if player_id:
            queryset = queryset.filter(player_id=player_id)
            
        return queryset


class NeuroSprintRecommendationViewSet(viewsets.ModelViewSet):
    """
    API endpoint for NeuroSprint recommendations
    """
    queryset = NeuroSprintRecommendation.objects.all()
    serializer_class = NeuroSprintRecommendationSerializer
    permission_classes = [permissions.IsAuthenticated]
    
    def get_queryset(self):
        """
        Optionally filter by player
        """
        queryset = NeuroSprintRecommendation.objects.all()
        player_id = self.request.query_params.get('player_id', None)
        
        if player_id:
            queryset = queryset.filter(player_id=player_id)
            
        return queryset


class ProcessGameSessionData(APIView):
    """
    Process raw game session data to create or update a NeuroSprint session
    """
    permission_classes = [permissions.IsAuthenticated]
    
    def post(self, request, session_id):
        try:
            # Get the game session
            game_session = get_object_or_404(GameSession, id=session_id)
            
            # Verify it's a NeuroSprint session
            if game_session.game_name != "NeuroSprint":
                return Response(
                    {"error": "This endpoint is only for NeuroSprint sessions"},
                    status=status.HTTP_400_BAD_REQUEST
                )
            
            # Extract data from the session
            session_data = game_session.session_data
            
            # Process the data
            processed_data = self._process_session_data(session_data)
            
            # Create or update the NeuroSprint session
            neurosprint_session, created = NeuroSprintSession.objects.update_or_create(
                session=game_session,
                defaults=processed_data
            )
            
            # Update player progress
            self._update_player_progress(game_session.player, neurosprint_session)
            
            # Generate recommendations if needed
            self._generate_recommendations(game_session.player, neurosprint_session)
            
            return Response(
                NeuroSprintSessionSerializer(neurosprint_session).data,
                status=status.HTTP_201_CREATED if created else status.HTTP_200_OK
            )
            
        except Exception as e:
            return Response({"error": str(e)}, status=status.HTTP_500_INTERNAL_SERVER_ERROR)
    
    def _process_session_data(self, session_data):
        """Process raw session data into structured metrics"""
        # Extract metrics from session_data
        reaction_times = session_data.get('reaction_times', [])
        attention_scores = session_data.get('attention_scores', [])
        obstacles_avoided = session_data.get('obstacles_avoided', 0)
        obstacles_hit = session_data.get('obstacles_hit', 0)
        distractions_ignored = session_data.get('distractions_ignored', 0)
        distractions_triggered = session_data.get('distractions_triggered', 0)
        
        # Calculate derived metrics
        total_obstacles = obstacles_avoided + obstacles_hit
        total_distractions = distractions_ignored + distractions_triggered
        
        # Avoid division by zero
        obstacle_avoidance_rate = (obstacles_avoided / total_obstacles * 100) if total_obstacles > 0 else 0
        distraction_resistance_rate = (distractions_ignored / total_distractions * 100) if total_distractions > 0 else 0
        
        # Calculate reaction time metrics
        avg_reaction_time = np.mean(reaction_times) if reaction_times else 0
        min_reaction_time = np.min(reaction_times) if reaction_times else 0
        max_reaction_time = np.max(reaction_times) if reaction_times else 0
        reaction_time_std = np.std(reaction_times) if len(reaction_times) > 1 else 0
        
        # Calculate attention metrics
        avg_attention_score = np.mean(attention_scores) if attention_scores else 0
        attention_consistency = np.std(attention_scores) if len(attention_scores) > 1 else 0
        
        # Count significant attention drops
        attention_drops = 0
        if len(attention_scores) > 1:
            for i in range(1, len(attention_scores)):
                if attention_scores[i] < attention_scores[i-1] - 20:  # Drop of 20+ points
                    attention_drops += 1
        
        # Identify ADHD indicators
        adhd_indicators = {
            'high_reaction_variability': reaction_time_std > 0.3,
            'attention_lapses': attention_drops > 2,
            'distractibility': distraction_resistance_rate < 60,
            'inconsistent_performance': attention_consistency > 15
        }
        
        return {
            'avg_attention_score': avg_attention_score,
            'attention_consistency': attention_consistency,
            'attention_drops': attention_drops,
            'avg_reaction_time': avg_reaction_time,
            'min_reaction_time': min_reaction_time,
            'max_reaction_time': max_reaction_time,
            'reaction_time_std': reaction_time_std,
            'obstacles_avoided': obstacles_avoided,
            'obstacles_hit': obstacles_hit,
            'obstacle_avoidance_rate': obstacle_avoidance_rate,
            'distractions_ignored': distractions_ignored,
            'distractions_triggered': distractions_triggered,
            'distraction_resistance_rate': distraction_resistance_rate,
            'reaction_times': reaction_times,
            'attention_scores': attention_scores,
            'adhd_indicators': adhd_indicators
        }
    
    def _update_player_progress(self, player, session):
        """Update the player's progress with the new session data"""
        # Get or create progress record
        progress, created = NeuroSprintProgress.objects.get_or_create(
            player=player
        )
        
        # Get all sessions for this player
        player_sessions = NeuroSprintSession.objects.filter(
            session__player=player
        ).order_by('session__start_time')
        
        if player_sessions.count() > 0:
            # Calculate metrics across all sessions
            total_sessions = player_sessions.count()
            total_playtime = sum(s.session.duration_minutes or 0 for s in player_sessions)
            highest_score = max(s.session.score or 0 for s in player_sessions)
            
            # Calculate average attention score
            avg_attention = np.mean([s.avg_attention_score for s in player_sessions if s.avg_attention_score])
            
            # Calculate average reaction time
            avg_reaction = np.mean([s.avg_reaction_time for s in player_sessions if s.avg_reaction_time])
            
            # Calculate overall rates
            all_obstacles_avoided = sum(s.obstacles_avoided for s in player_sessions)
            all_obstacles_hit = sum(s.obstacles_hit for s in player_sessions)
            all_distractions_ignored = sum(s.distractions_ignored for s in player_sessions)
            all_distractions_triggered = sum(s.distractions_triggered for s in player_sessions)
            
            total_obstacles = all_obstacles_avoided + all_obstacles_hit
            total_distractions = all_distractions_ignored + all_distractions_triggered
            
            obstacle_rate = (all_obstacles_avoided / total_obstacles * 100) if total_obstacles > 0 else 0
            distraction_rate = (all_distractions_ignored / total_distractions * 100) if total_distractions > 0 else 0
            
            # Calculate trends (if at least 3 sessions)
            attention_trend = 0
            reaction_trend = 0
            
            if player_sessions.count() >= 3:
                # Get the last 5 sessions (or all if fewer)
                recent_sessions = player_sessions.order_by('-session__start_time')[:5]
                recent_attention = [s.avg_attention_score for s in recent_sessions if s.avg_attention_score]
                recent_reaction = [s.avg_reaction_time for s in recent_sessions if s.avg_reaction_time]
                
                # Calculate trends (positive attention trend = improving, negative reaction trend = improving)
                if len(recent_attention) >= 3:
                    x = np.arange(len(recent_attention))
                    y = np.array(recent_attention)
                    attention_trend = np.polyfit(x, y, 1)[0]  # Slope of the line
                
                if len(recent_reaction) >= 3:
                    x = np.arange(len(recent_reaction))
                    y = np.array(recent_reaction)
                    reaction_trend = np.polyfit(x, y, 1)[0]  # Slope of the line
            
            # Calculate ADHD likelihood score (simplified example)
            adhd_indicators_count = sum(1 for indicator, value in session.adhd_indicators.items() if value)
            adhd_likelihood = min(100, adhd_indicators_count * 25)  # Simple scaling
            
            # Update progress record
            progress.total_sessions = total_sessions
            progress.total_playtime_minutes = total_playtime
            progress.highest_score = highest_score
            progress.avg_attention_score = avg_attention
            progress.attention_trend = attention_trend
            progress.avg_reaction_time = avg_reaction
            progress.reaction_time_trend = reaction_trend
            progress.overall_obstacle_avoidance_rate = obstacle_rate
            progress.overall_distraction_resistance_rate = distraction_rate
            progress.adhd_likelihood_score = adhd_likelihood
            progress.attention_consistency_score = 100 - min(100, session.attention_consistency * 5)  # Lower consistency = higher score
            progress.last_session = session
            
            # Update difficulty level if needed
            self._update_difficulty_level(progress, session)
            
            progress.save()
    
    def _update_difficulty_level(self, progress, session):
        """Update difficulty level based on performance"""
        # Simple algorithm for difficulty adjustment
        current_level = progress.current_difficulty_level
        performance_score = 0
        
        # Calculate performance score based on session metrics
        if session.obstacle_avoidance_rate > 80:
            performance_score += 1
        if session.distraction_resistance_rate > 75:
            performance_score += 1
        if session.avg_attention_score > 70:
            performance_score += 1
        if session.avg_reaction_time < 0.7:  # Good reaction time
            performance_score += 1
        
        # Adjust difficulty if needed
        if current_level == 'easy' and performance_score >= 3:
            progress.current_difficulty_level = 'medium'
            progress.difficulty_progression.append({
                'date': datetime.now().isoformat(),
                'from': 'easy',
                'to': 'medium',
                'session_id': session.id
            })
        elif current_level == 'medium' and performance_score >= 4:
            progress.current_difficulty_level = 'hard'
            progress.difficulty_progression.append({
                'date': datetime.now().isoformat(),
                'from': 'medium',
                'to': 'hard',
                'session_id': session.id
            })
        elif current_level == 'medium' and performance_score <= 1:
            progress.current_difficulty_level = 'easy'
            progress.difficulty_progression.append({
                'date': datetime.now().isoformat(),
                'from': 'medium',
                'to': 'easy',
                'session_id': session.id
            })
        elif current_level == 'hard' and performance_score <= 2:
            progress.current_difficulty_level = 'medium'
            progress.difficulty_progression.append({
                'date': datetime.now().isoformat(),
                'from': 'hard',
                'to': 'medium',
                'session_id': session.id
            })
    
    def _generate_recommendations(self, player, session):
        """Generate personalized recommendations based on session data"""
        # Check for attention issues
        if session.avg_attention_score < 50:
            NeuroSprintRecommendation.objects.create(
                player=player,
                title="Improve Focus with Shorter Sessions",
                description="Your attention score is below average. Try playing shorter, more frequent sessions to build up your attention span gradually.",
                priority=2,
                recommendation_type='schedule',
                triggering_session=session
            )
        
        # Check for high distractibility
        if session.distraction_resistance_rate < 60:
            NeuroSprintRecommendation.objects.create(
                player=player,
                title="Distraction Resistance Training",
                description="You seem to be easily distracted during gameplay. Try practicing mindfulness exercises for 5 minutes before playing to improve your ability to ignore distractions.",
                priority=3,
                recommendation_type='exercise',
                triggering_session=session
            )
        
        # Check for reaction time issues
        if session.avg_reaction_time > 0.8:
            NeuroSprintRecommendation.objects.create(
                player=player,
                title="Reaction Time Improvement",
                description="Your reaction time is slower than average. Try the 'Quick Reactions' mini-game to improve your response speed.",
                priority=2,
                recommendation_type='gameplay',
                triggering_session=session
            )
        
        # Check for inconsistent performance
        if session.attention_consistency > 20:
            NeuroSprintRecommendation.objects.create(
                player=player,
                title="Consistency Training",
                description="Your attention levels fluctuate significantly during gameplay. Focus on maintaining consistent attention rather than achieving high scores.",
                priority=1,
                recommendation_type='gameplay',
                triggering_session=session
            )