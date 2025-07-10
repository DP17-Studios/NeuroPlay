from rest_framework import serializers
from .models import Game, PlayerGameConfig


class GameSerializer(serializers.ModelSerializer):
    class Meta:
        model = Game
        fields = [
            'id', 'name', 'display_name', 'description', 'version',
            'health_domain', 'default_config', 'unity_bundle_url',
            'unity_bundle_version', 'times_played', 'active',
            'created_at', 'updated_at'
        ]
        read_only_fields = ['id', 'times_played', 'created_at', 'updated_at']


class PlayerGameConfigSerializer(serializers.ModelSerializer):
    game_name = serializers.ReadOnlyField(source='game.name')
    game_display_name = serializers.ReadOnlyField(source='game.display_name')
    
    class Meta:
        model = PlayerGameConfig
        fields = [
            'id', 'player', 'game', 'game_name', 'game_display_name',
            'config_overrides', 'difficulty_level', 'current_level',
            'highest_score', 'times_played', 'total_playtime_minutes',
            'last_played', 'created_at', 'updated_at'
        ]
        read_only_fields = ['id', 'game_name', 'game_display_name', 
                           'highest_score', 'times_played', 
                           'total_playtime_minutes', 'last_played',
                           'created_at', 'updated_at']