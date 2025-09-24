## Game Overview

This is a mobile-friendly Unity game where players must pop balloons in a specific color sequence to score points.

## Core Gameplay

-   **Balloon Spawning**:  BalloonSpawner  continuously spawns balloons with random colors that float upward from the bottom of the screen
-   **Color Sequences**:  SequenceManager  generates random color sequences (typically 3 colors) displayed as UI targets
-   **Input System**:  InputManager  handles both mouse clicks (desktop) and touch input (mobile) for balloon interaction
-   **Balloon Behavior**:  BalloonController  manages balloon movement, color assignment, popping animations, and speed variations

## Game Mechanics

-   Players must pop balloons in the  **exact order**  shown in the sequence
-   **Correct pops**  advance the sequence and award points
-   **Wrong pops**  reduce health/lives
-   **Sequence completion**  gives bonus points and generates a new sequence
-   **Game over**  occurs when health reaches zero
-   Balloons have  **random float speeds**  for added challenge

## Technical Features

-   **Health System**:  HealthManager  tracks player lives
-   **Score System**:  ScoreManager  manages points and UI updates
-   **Visual Effects**: Camera shake on balloon pops via  GameTween.Instance
-   **Audio**: Pop sound effects when balloons are destroyed
-   **Animations**: Custom sprite-based or Animator-based pop animations

## Game Flow

1.  Game generates a color sequence (e.g., Red → Blue → Green)
2.  Balloons spawn continuously with random colors
3.  Player must find and pop balloons matching the sequence order
4.  Correct sequence completion → bonus points → new sequence
5.  Wrong balloon → lose health → potential game over

This creates an engaging pattern-matching game with time pressure as balloons continuously spawn and float away if not popped in time.