from rest_framework import serializers
from django.contrib.auth.models import User
from .models import PlayerProfile, GameSession


class UserSerializer(serializers.ModelSerializer):
    class Meta:
        model = User
        fields = ['id', 'username', 'email', 'first_name', 'last_name']
        read_only_fields = ['id']


class PlayerProfileSerializer(serializers.ModelSerializer):
    user = UserSerializer(read_only=True)
    username = serializers.CharField(write_only=True, required=False)
    email = serializers.EmailField(write_only=True, required=False)
    
    class Meta:
        model = PlayerProfile
        fields = [
            'id', 'user', 'username', 'email', 'date_of_birth',
            'has_adhd', 'has_dementia', 'has_motor_issues', 'has_asd', 'has_depression',
            'games_played', 'total_sessions', 'total_playtime_minutes',
            'preferred_difficulty', 'created_at', 'updated_at'
        ]
        read_only_fields = ['id', 'games_played', 'total_sessions', 'total_playtime_minutes', 'created_at', 'updated_at']
    
    def create(self, validated_data):
        username = validated_data.pop('username', None)
        email = validated_data.pop('email', None)
        
        if username and email:
            user = User.objects.create(username=username, email=email)
            profile = PlayerProfile.objects.create(user=user, **validated_data)
            return profile
        
        raise serializers.ValidationError("Username and email are required to create a new profile")


class GameSessionSerializer(serializers.ModelSerializer):
    player_username = serializers.ReadOnlyField(source='player.user.username')
    
    class Meta:
        model = GameSession
        fields = [
            'id', 'player', 'player_username', 'game_name', 
            'start_time', 'end_time', 'duration_minutes',
            'score', 'difficulty_level', 'completed',
            'session_data', 'created_at', 'updated_at'
        ]
        read_only_fields = ['id', 'created_at', 'updated_at']