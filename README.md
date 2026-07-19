# Concussion VR Prototype

A safety-first Unity research prototype for a supervised, symptom-limited VR session. It is not a medical device and must not diagnose, clear, or independently treat concussion.

## Flow

1. Pre-session symptom check
2. Seated nature-based breathing task
3. Head-gaze visual pursuit task
4. Working-memory and reaction-time task
5. Post-session symptom check
6. De-identified CSV export

## Run

- Unity version: 6000.4.9f1
- Open `Assets/ConcussionVR/Scenes/ConcussionVRPrototype.unity`
- Press Play
- Use the mouse for assessment buttons
- Use `1`, `2`, or `3` in the memory task
- Press `Esc` at any time for the participant/observer safety stop

CSV files are written beneath `Application.persistentDataPath/ConcussionVR/Sessions`.

## Boundaries

- Quest 3 measurements are labeled head-gaze, not eye gaze.
- Intensity progression is not automated.
- Free walking is intentionally excluded.
- Clinical eligibility, stopping rules, and progression require an approved protocol and qualified clinician oversight.

