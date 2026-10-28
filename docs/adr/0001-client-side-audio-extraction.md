# 0001: Convert video to audio on the client

## Context
Users upload video files. Only the audio is needed for transcription.
Sending full video makes requests large and slow, and risks exceeding
the 25 MB Whisper limit.

## Decision
Extract audio in the frontend and upload only the audio.
The backend only handles audio → transcript.

## Consequences
+ Much smaller HTTP payloads and faster uploads
+ Less server CPU and bandwidth (no server-side video processing)
+ Backend stays simple (no ffmpeg dependency)
- Conversion work happens on the user's device (slower on weak devices)
- Browser support/codecs vary; the backend must still validate the
  file extension and size, since it can't trust the client
- Long audio may still exceed 25 MB and need chunking