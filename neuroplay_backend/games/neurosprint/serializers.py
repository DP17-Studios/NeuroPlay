from rest_framework import serializers
from .models import NeuroSprintSession, NeuroSprintProgress, NeuroSprintRecommendation
from api.serializers import PlayerProfileSerializer


class NeuroSprintSessionSerializer(serializers.ModelSerializer):
    player_username = serializers.ReadOnlyField(source='session.player.user.username')
    session_id = serializers.ReadOnlyField(source='session.id')
    start_time = serializers.ReadOnlyField(source='session.start_time')
    end_time = serializers.ReadOnlyField(source='session.end_time')
    score = serializers.ReadOnlyField(source='session.score')
    
    class Meta:
        model = NeuroSprintSession
        fields = [
            'id', 'session_id', 'player_username', 'start_time', 'end_time', 'score',
            'avg_attention_score', 'attention_consistency', 'attention_drops',
            'avg_reaction_time', 'min_reaction_time', 'max_reaction_time', 'reaction_time_std',
            'obstacles_avoided', 'obstacles_hit', 'obstacle_avoidance_rate',
            'distractions_ignored', 'distractions_triggered', 'distraction_resistance_rate',
            'reaction_times', 'attention_scores', 'adhd_indicators',
            'created_at', 'updated_at'
        ]
        read_only_fields = ['id', 'created_at', 'updated_at']


class NeuroSprintProgressSerializer(serializers.ModelSerializer):
    player = PlayerProfileSerializer(read_only=True)
    player_id = serializers.PrimaryKeyRelatedField(
        source='player', 
        read_only=True
    )
    
    class Meta:
        model = NeuroSprintProgress
        fields = [
            'id', 'player', 'player_id', 'total_sessions', 'total_playtime_minutes',
            'highest_score', 'avg_attention_score', 'attention_trend',
            'avg_reaction_time', 'reaction_time_trend',
            'overall_obstacle_avoidance_rate', 'overall_distraction_resistance_rate',
            'current_difficulty_level', 'difficulty_progression',
            'adhd_likelihood_score', 'attention_consistency_score',
            'created_at', 'updated_at'
        ]
        read_only_fields = ['id', 'player', 'player_id', 'created_at', 'updated_at']


class NeuroSprintRecommendationSerializer(serializers.ModelSerializer):
    player_username = serializers.ReadOnlyField(source='player.user.username')
    
    class Meta:
        model = NeuroSprintRecommendation
        fields = [
            'id', 'player', 'player_username', 'title', 'description',
            'priority', 'recommendation_type', 'is_implemented',
            'implementation_date', 'triggering_session',
            'created_at', 'updated_at'
        ]
        read_only_fields = ['id', 'created_at', 'updated_at']