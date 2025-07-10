from datetime import datetime
from rest_framework.views import APIView
from rest_framework.response import Response
from rest_framework import status
from rest_framework.permissions import IsAuthenticated
from .models import Game, PlayerGameConfig
from .serializers import GameSerializer, PlayerGameConfigSerializer
from api.models import PlayerProfile, GameSession
from api.serializers import GameSessionSerializer


class GameListView(APIView):
    """
    Get a list of all available games
    """
    permission_classes = [IsAuthenticated]
    
    def get(self, request):
        games = Game.objects.filter(active=True)
        serializer = GameSerializer(games, many=True)
        return Response(serializer.data, status=status.HTTP_200_OK)


class GameConfigView(APIView):
    """
    Get configuration for a specific game, including player-specific settings if available
    """
    permission_classes = [IsAuthenticated]
    
    def get(self, request, game_name):
        try:
            game = Game.objects.get(name=game_name)
            
            # Get player-specific config if player_id is provided
            player_id = request.query_params.get('player_id', None)
            if player_id:
                try:
                    player = PlayerProfile.objects.get(id=player_id)
                    player_config, created = PlayerGameConfig.objects.get_or_create(
                        player=player,
                        game=game,
                        defaults={
                            'difficulty_level': player.preferred_difficulty,
                            'config_overrides': {}
                        }
                    )
                    
                    # Merge default config with player overrides
                    config = game.default_config.copy()
                    config.update(player_config.config_overrides)
                    
                    response_data = {
                        'game': GameSerializer(game).data,
                        'player_config': PlayerGameConfigSerializer(player_config).data,
                        'merged_config': config
                    }
                    
                    return Response(response_data, status=status.HTTP_200_OK)
                except PlayerProfile.DoesNotExist:
                    return Response({"error": "Player not found"}, status=status.HTTP_404_NOT_FOUND)
            
            # Return just the game config if no player_id
            return Response(GameSerializer(game).data, status=status.HTTP_200_OK)
            
        except Game.DoesNotExist:
            return Response({"error": "Game not found"}, status=status.HTTP_404_NOT_FOUND)


class GameSessionStartView(APIView):
    """
    Start a new game session
    """
    permission_classes = [IsAuthenticated]
    
    def post(self, request):
        try:
            player_id = request.data.get('player_id')
            game_name = request.data.get('game_name')
            difficulty_level = request.data.get('difficulty_level', 'medium')
            
            if not player_id or not game_name:
                return Response(
                    {"error": "player_id and game_name are required"}, 
                    status=status.HTTP_400_BAD_REQUEST
                )
            
            player = PlayerProfile.objects.get(id=player_id)
            game = Game.objects.get(name=game_name)
            
            # Create a new session
            session = GameSession.objects.create(
                player=player,
                game_name=game_name,
                start_time=datetime.now(),
                difficulty_level=difficulty_level,
                session_data={}
            )
            
            # Update game and player stats
            game.times_played += 1
            game.save()
            
            player_config, created = PlayerGameConfig.objects.get_or_create(
                player=player,
                game=game,
                defaults={
                    'difficulty_level': difficulty_level,
                    'config_overrides': {}
                }
            )
            
            player_config.times_played += 1
            player_config.last_played = datetime.now()
            player_config.save()
            
            return Response(
                GameSessionSerializer(session).data, 
                status=status.HTTP_201_CREATED
            )
            
        except PlayerProfile.DoesNotExist:
            return Response({"error": "Player not found"}, status=status.HTTP_404_NOT_FOUND)
        except Game.DoesNotExist:
            return Response({"error": "Game not found"}, status=status.HTTP_404_NOT_FOUND)
        except Exception as e:
            return Response({"error": str(e)}, status=status.HTTP_500_INTERNAL_SERVER_ERROR)


class GameSessionEndView(APIView):
    """
    End a game session and record results
    """
    permission_classes = [IsAuthenticated]
    
    def post(self, request, session_id):
        try:
            session = GameSession.objects.get(id=session_id)
            
            # Ensure session isn't already completed
            if session.end_time:
                return Response(
                    {"error": "Session already ended"}, 
                    status=status.HTTP_400_BAD_REQUEST
                )
            
            # Update session with end data
            session.end_time = datetime.now()
            session.score = request.data.get('score', 0)
            session.completed = request.data.get('completed', False)
            
            # Calculate duration
            if session.start_time:
                duration = (session.end_time - session.start_time).total_seconds() / 60
                session.duration_minutes = duration
            
            session.save()
            
            # Update player stats
            player = session.player
            player.total_sessions += 1
            player.total_playtime_minutes += session.duration_minutes or 0
            player.games_played = GameSession.objects.filter(player=player).values('game_name').distinct().count()
            player.save()
            
            # Update player game config
            try:
                game = Game.objects.get(name=session.game_name)
                player_config = PlayerGameConfig.objects.get(player=player, game=game)
                
                player_config.total_playtime_minutes += session.duration_minutes or 0
                
                # Update highest score if applicable
                if session.score > player_config.highest_score:
                    player_config.highest_score = session.score
                
                player_config.save()
                
            except (Game.DoesNotExist, PlayerGameConfig.DoesNotExist):
                pass  # Skip if game or config doesn't exist
            
            return Response(
                GameSessionSerializer(session).data, 
                status=status.HTTP_200_OK
            )
            
        except GameSession.DoesNotExist:
            return Response({"error": "Session not found"}, status=status.HTTP_404_NOT_FOUND)
        except Exception as e:
            return Response({"error": str(e)}, status=status.HTTP_500_INTERNAL_SERVER_ERROR)


class GameDataUploadView(APIView):
    """
    Upload gameplay data for a session
    """
    permission_classes = [IsAuthenticated]
    
    def post(self, request, session_id):
        try:
            session = GameSession.objects.get(id=session_id)
            
            # Get the data to be added to the session
            session_data = request.data.get('session_data', {})
            if not isinstance(session_data, dict):
                return Response(
                    {"error": "session_data must be a JSON object"}, 
                    status=status.HTTP_400_BAD_REQUEST
                )
            
            # Update or merge with existing session data
            current_data = session.session_data or {}
            current_data.update(session_data)
            session.session_data = current_data
            session.save()
            
            return Response(
                {"message": "Session data updated successfully"}, 
                status=status.HTTP_200_OK
            )
            
        except GameSession.DoesNotExist:
            return Response({"error": "Session not found"}, status=status.HTTP_404_NOT_FOUND)
        except Exception as e:
            return Response({"error": str(e)}, status=status.HTTP_500_INTERNAL_SERVER_ERROR)