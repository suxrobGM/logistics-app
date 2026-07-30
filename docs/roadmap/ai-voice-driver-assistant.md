# Voice Driver Assistant (mobile)

- **Status**: Planned
- **Priority**: P2 - long bet; every competitor's driver app is forms, drivers hate typing. Voice DVIR alone demos unbelievably well
- **Effort**: XL
- **Category**: AI differentiation / mobile

## Why

Driver-facing AI is an empty niche. "I'm empty at the receiver, what's next?" answered by voice,
and a DVIR filed by talking while walking around the truck, are high-wow features on hardware
drivers already hold.

## What to build

- Driver-scoped agent: same loop, restricted tool set (my HOS via `GetDriverHosStatusTool` pattern, my current/next assignment, submit DVIR, send message to dispatch). Tools must enforce driver identity - never tenant-wide reads.
- Speech: on-device STT/TTS in the KMP app (`src/Client/Logistics.DriverApp/`) - platform APIs first (Android SpeechRecognizer / iOS Speech), streaming ASR later.
- Voice DVIR: conversational walk-through mapping speech to `DvirReport`/`DvirDefect` fields; confirm summary before submit.
- Hands-free/CDL compliance: audio-first UX, large single-tap trigger, never require reading while driving.
- Quota: driver sessions metered like dispatch sessions (`AIQuotaService`).
- Phase it: (1) text chat driver assistant in the app, (2) voice input, (3) voice DVIR.

## Acceptance

A driver completes a full pre-trip DVIR by voice in under 2 minutes, and gets a spoken answer to "how many driving hours do I have left?"

## Notes

_(add dated implementation notes here)_
