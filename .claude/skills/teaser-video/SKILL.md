---
name: teaser-video
description: Produce a launch-quality product teaser (~45s) with motion graphics, licensed music cut to the edit and recorded sound effects - real app UI captured with Playwright, composed in Remotion as 3D layers, kinetic type and camera moves cut to a tempo grid. Delivers MP4 + README GIF. Use for "make a teaser", "product video", "demo video for the website", "README gif", "refilm the teaser".
---

# Teaser video

Motion design that contains the product, not a screen recording with title cards. Real UI is composed as objects on a dark stage, a camera moves through them, cuts land on the beat of a licensed track edited to the timeline, and recorded sound effects hit the same frames.

```
capture (Playwright)          compose (Remotion)              audio (Mixkit + ffmpeg)
  2x stills of screens   ->     scenes on a 120 BPM grid  <-    music-<track>.wav + sfx/*.wav
  1x CDP footage of flows       crops, 3D, camera, type          on the same grid
                                        |
                                 render -> review -> deliver (MP4 + GIF)
```

The working LogisticsX teaser is the reference implementation:

| Path                                      | What it is                                                      |
| ----------------------------------------- | --------------------------------------------------------------- |
| `reference/capture/session.js`            | Signed-in page via a persistent profile; in-app navigation      |
| `reference/capture/rig.js`                | CDP screencast, constant-rate assembly, injected cursor         |
| `reference/capture/stills.js`             | 2x screenshots into `compose/public/stills/`                    |
| `reference/capture/take-ai-dispatch.js`   | The one filmed flow, with an event log for the edit and the SFX |
| `reference/compose/src/`                  | Remotion project: timeline, theme, components, a file per scene |
| `reference/compose/audio/build-audio.mjs` | Downloads music and effects, edits each track onto the grid     |
| `reference/compose/preview.mjs`           | Bundle once, render chosen frames as stills (fast review loop)  |
| `reference/compose/deliver.sh`            | Web MP4, README GIF, 2fps review frames                         |

Work in the scratchpad, never in the repo. `compose/public/` is not committed; rebuild it:

```bash
cp -r <skill>/reference/{capture,compose} "$SCRATCH/" && cd "$SCRATCH"
(cd capture && npm install) && (cd compose && npm install)
export MSYS_NO_PATHCONV=1 && cd capture
node stills.js home=/home loads=/loads eld=/eld invoices=/invoices payroll=/payroll maintenance=/maintenance \
  loadboard=/loadboard safety=/safety financials=/reports/financials containers=/containers expenses=/expenses messages=/messages
TEASER_USER=customer1@test.com TEASER_BASE=https://customer.logisticsx.app TEASER_ROOT_IS_HOME=1 node stills.js customer=/shipments
node take-ai-dispatch.js && mkdir -p ../compose/public/footage && cp takes/ai-run.mp4 ../compose/public/footage/ai.mp4
node stills.js ai-final=/ai-dispatch                       # the finished session, for the close-ups
cd ../compose
mkdir -p public/phones public/tiles && cp <repo>/docs/store-assets/screenshots/raw/*.png public/phones/
for n in loads loadboard invoices containers expenses home payroll eld messages maintenance safety customer; do
  ffmpeg -y -i public/stills/$n.png -vf scale=1600:900 -q:v 3 public/tiles/$n.jpg; done
node audio/build-audio.mjs                                  # needs ffmpeg with librubberband on PATH
```

A new take changes the timings: update `SEGMENTS`, `CAMERA` and `PILLS` in `scenes/AIDispatch.tsx` from `takes/ai-run.json`, and re-measure the crop rects on `ai-final.png`. The finale crop shows whatever state the decision is in when `ai-final` is captured.

## The two hard rules

**1. No real-world side effects without itemized consent.** Filming clicks real buttons. List every storyboard action that writes data or leaves the machine (email, push, payments, third-party calls), say what each does and how many times, and get agreement. Prefer filming up to the irreversible step and cutting on the click. If something fires unintentionally, stop it at the source, verify, and report it with a count.

LogisticsX: approving an AI dispatch `assign_load_to_truck` decision only writes `Load.AssignedTruckId`; `create_trip` pushes to the driver and Telegram; `propose_counter_offer` emails a real broker. Never confirm the last two on camera.

**2. Every frame is public.** Use seeded demo accounts, frame around anything personal, and review the final cut frame by frame before delivering.

## 1. Storyboard to a tempo

Pick the tempo first; make every duration a multiple of it. 120 BPM at 60fps gives a 30-frame beat and a 120-frame bar, so scenes start on bars and hits land on beats. `src/timeline.ts` and the edit lists in `audio/build-audio.mjs` share these constants.

Arc that works (bars): cold open with kinetic type (2), the problem as UI fragments piling up (2), silence then brand reveal on the downbeat (2), hero feature (6), three or four proof scenes (1-2 each), fast montage with one screen per beat (2), close and URL (4).

Per scene, write what the viewer should feel, which captured asset carries it, and the one on-screen sentence. A scene with no job is cut before filming.

## 2. Capture

**Stills (2x)** carry most of the video. `deviceScaleFactor: 2` + `page.screenshot` gives 3200x1800 PNGs of a 1600x900 page, sharp to 2x zoom. `Crop` takes a rect in the page's CSS pixels; measure regions on a 1600x900 downscale with a 100px grid (`drawgrid=w=100:h=100`).

**Footage (1x)** only for flows that must visibly happen (typing, an agent working, a click). CDP `Page.startScreencast` records CSS pixels regardless of scale factor, so takes are 1600x900. Keep the camera at or below ~1.3x on footage; use a 2x still of the end state for close-ups. Do not use `zoom: 2` on the document: it breaks `vh` layout and Mapbox. Log event times during the take (`mark()` in the take script); they drive the edit and the SFX.

Before rolling: dismiss cookie banners, avoid tiles showing zeros or broken numbers, and clean junk test records in the demo tenant through the UI (with consent).

**Auth gotchas (LogisticsX):**

- The SPA keeps tokens in `sessionStorage`: navigate in-app with `history.pushState` + `popstate`, not `page.goto`.
- The identity server rate-limits login to 10 attempts per 15 minutes per IP. Use a persistent profile (`launchPersistentContext`) so later scripts bounce through "Sign in".
- The customer portal lands on `/`; treat the root as signed-in.
- Under Git Bash set `MSYS_NO_PATHCONV=1` or routes like `/loads` become Windows paths.

**AI dispatch take:** the agent only sees Draft loads not in a trip, so check the tenant has some ("0 unassigned loads" makes a dull take). Keep the request to one load.

## 3. Compose in Remotion

Check Remotion APIs against remotion.dev rather than guessing. The look:

- **Flat ink stage** with a faint drifting dot grid and one global lens layer (vignette, animated grain). No gradients, glows or coloured icon circles; colour only for state (green approved, red violation, amber warning). The brand mark is the one gradient.
- **UI as objects.** Crops float in a `perspective` container with `preserve-3d`, enter on springs from depth, dim as newer ones arrive, collapse to a point before a cut. `Window` frames footage; phones get a bezel component.
- **A camera, not zooms.** Keyframes of `{frame, scale, focus x/y}` in page coordinates, eased in-out, drive a transform on the window. Never animate footage with ffmpeg `zoompan` (rounds to whole pixels, shakes).
- **Kinetic type** in the app's fonts (`@remotion/google-fonts`): lines rise out of masks, words stagger 3-5 frames, numbers count up with tabular figures, mono labels type themselves. Over busy UI, dim the layers behind instead of adding a glow.
- **Speed ramps** as `OffthreadVideo` segments, each with its own `trimBefore` and `playbackRate`. Map logged take times through the same segment table to place SFX.
- **Custom graphics** where the product shows none, e.g. the tilted US route map from `us-atlas` + `d3-geo` with the demo tenant's real lanes.

Iterate with `node preview.mjs <frames...>` and a contact sheet, not video renders. Many 3200px PNGs in one frame make headless Chrome fail ("source image cannot be decoded"); use downscaled JPEG tiles wherever an image is drawn small.

## 4. Music and sound effects

Use licensed recordings, not synthesis (a generated score sounded generic, and noise-based effects read as hiss). You cannot listen: you screen and measure, the user picks by ear.

**Source.** Mixkit's free music and SFX licenses allow commercial web, social and ad use, no attribution, editing allowed. They forbid music-only remixes, registering the audio with a rights-management (Content ID) service, and redistributing raw files, including "with source files". So `build-audio.mjs` downloads at build time, audio is never committed, and `public/audio/CREDITS.md` records what was used. Avoid Pixabay for anything going to YouTube: uploads can be registered for Content ID after download.

**Screening.** Mixkit tag pages carry JSON-LD (name, artist, genre, duration, preview URL); listing markup carries mood tags. Download candidates and measure tempo (onset autocorrelation), beat clarity, energy rises and drops, breakdowns, and whether it ends or fades. Shortlist four or five with mood and structure for the user.

**Fitting a track.** Beat-track for exact tempo, first downbeat and per-bar energy, then write its edit list in `MUSIC`: pieces of source time putting the drop on the logo reveal (8s) and the last phrase or the track's own ending on the close (40s). Cut on downbeats, preferably 4 or 8 bar phrase boundaries, and hide a jump behind a timeline silence. A 120 BPM track needs nothing else ("Ramp It Up" is 120.01). Within a few percent, stretch with `rubberband` (pitch kept); avoid more than about 4%. Every region boundary gets a short fade (30ms out, 3ms in on the downbeat). Masters get 4 dB headroom because they peak above 0 dBFS once decoded.

**Effects.** One recording per role (swoosh, whoosh, impact, click, key, pop, chime, tick), trimmed of digital silence only (a whoosh swells from quiet), normalized, peak time saved to `sfx/peaks.json`. Cues in `Teaser.tsx` name the frame the sound should peak on; its sequence starts that much earlier. Prefer recordings whose noise floor is digital silence.

Measure: `ebur128` for loudness, `showwavespic` of the whole cut plus 1s windows around every edit and silence, to confirm drops land on the frame and nothing clicks. `deliver.sh` normalizes the final mix to -14 LUFS with a -1.5 dBTP ceiling in two linear passes.

## 5. Render, review, deliver

```bash
npx remotion render Teaser out/teaser-ramp.mkv --props='{"music":"ramp"}' --codec=h264 --crf=16 --audio-codec=pcm-16
npx remotion render Teaser out/gif-src.mp4 --frames=720-1439 --props='{"gif":true,"music":"ramp"}' --codec=h264 --crf=14 --muted
./deliver.sh out/teaser-ramp.mkv out/final out/gif-src.mp4   # -> out/final/teaser-ramp.mp4 + teaser.gif
```

The master is MKV so audio stays PCM until the single AAC encode. A full render takes about a minute. Read the 2fps review frames for PII, clipped panels, unreadable text and broken data, spot-check a few at full resolution, and probe duration and size.

The README GIF is the AI dispatch scene rendered with `gif: true`, which drops grain and the drifting grid (per-frame noise doubles GIF size). At 800px, 15fps, 128 colours it is about 7.5 MB; keep it under ~10 MB. GitHub won't inline a committed MP4, so the website gets the MP4 and the README the GIF.

LogisticsX outputs replace `docs/images/teaser.gif` and, in the private submodule, `src/Client/Logistics.Angular/projects/website/public/demo-video.mp4`.

Report paths, duration, dimensions, and anything that happened while filming the user should know (writes to the demo tenant, production errors seen on camera).
