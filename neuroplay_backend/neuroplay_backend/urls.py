"""
URL configuration for neuroplay_backend project.
"""
from django.contrib import admin
from django.urls import path, include

urlpatterns = [
    path('admin/', admin.site.urls),
    path('api/', include('api.urls')),
    path('analytics/', include('analytics.urls')),
    path('games/', include('games.urls')),
]