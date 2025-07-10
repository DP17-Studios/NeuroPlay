from rest_framework import viewsets, permissions
from django.contrib.auth.models import User
from .models import PlayerProfile, GameSession
from .serializers import UserSerializer, PlayerProfileSerializer, GameSessionSerializer


class UserViewSet(viewsets.ModelViewSet):
    """
    API endpoint for users
    """
    queryset = User.objects.all().order_by('-date_joined')
    serializer_class = UserSerializer
    permission_classes = [permissions.IsAuthenticated]


class PlayerProfileViewSet(viewsets.ModelViewSet):
    """
    API endpoint for player profiles
    """
    queryset = PlayerProfile.objects.all()
    serializer_class = PlayerProfileSerializer
    permission_classes = [permissions.IsAuthenticated]
    
    def get_queryset(self):
        """
        Optionally restricts the returned profiles to a given user,
        by filtering against a `username` query parameter in the URL.
        """
        queryset = PlayerProfile.objects.all()
        username = self.request.query_params.get('username', None)
        if username is not None:
            queryset = queryset.filter(user__username=username)
        return queryset


class GameSessionViewSet(viewsets.ModelViewSet):
    """
    API endpoint for game sessions
    """
    queryset = GameSession.objects.all()
    serializer_class = GameSessionSerializer
    permission_classes = [permissions.IsAuthenticated]
    
    def get_queryset(self):
        """
        Optionally filter sessions by player, game_name, or date range
        """
        queryset = GameSession.objects.all()
        
        player_id = self.request.query_params.get('player_id', None)
        if player_id is not None:
            queryset = queryset.filter(player_id=player_id)
            
        game_name = self.request.query_params.get('game_name', None)
        if game_name is not None:
            queryset = queryset.filter(game_name=game_name)
            
        start_date = self.request.query_params.get('start_date', None)
        if start_date is not None:
            queryset = queryset.filter(start_time__gte=start_date)
            
        end_date = self.request.query_params.get('end_date', None)
        if end_date is not None:
            queryset = queryset.filter(start_time__lte=end_date)
            
        return queryset