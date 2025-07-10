from django.db.models.signals import post_save
from django.dispatch import receiver
from api.models import GameSession
from .models import NeuroSprintSession, NeuroSprintProgress


@receiver(post_save, sender=GameSession)
def create_neurosprint_session(sender, instance, created, **kwargs):
    """
    Signal handler to create a NeuroSprintSession when a GameSession is created
    """
    # Only process NeuroSprint game sessions
    if instance.game_name != "NeuroSprint":
        return
        
    # If this is a new session, create a basic NeuroSprintSession
    if created:
        NeuroSprintSession.objects.create(session=instance)


@receiver(post_save, sender=NeuroSprintSession)
def update_neurosprint_progress(sender, instance, created, **kwargs):
    """
    Signal handler to update NeuroSprintProgress when a NeuroSprintSession is updated
    """
    # Get or create progress record for this player
    player = instance.session.player
    
    progress, created = NeuroSprintProgress.objects.get_or_create(
        player=player,
        defaults={
            'last_session': instance
        }
    )
    
    # If not a new progress record, just update the last session reference
    if not created:
        progress.last_session = instance
        progress.save(update_fields=['last_session', 'updated_at'])