from django.apps import AppConfig


class NeuroSprintConfig(AppConfig):
    default_auto_field = 'django.db.models.BigAutoField'
    name = 'games.neurosprint'
    verbose_name = 'NeuroSprint ADHD & Attention Training'
    
    def ready(self):
        # Import signal handlers
        import games.neurosprint.signals