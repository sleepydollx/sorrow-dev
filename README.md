# Grief (working title)

A psychological horror game about someone haunted by grief after losing their
partner. The presence that pursues you wears the partner's face and reaches out
not to harm you — but to **forgive** you. The whole game is about finding the
courage to stop running from that forgiveness.

Design principle: **mechanics = theme**. Running is what makes grief build;
facing memories is what heals. The player isn't just told about grief — they
*play* it.

---

## What's in this first commit

The core code skeleton. No finished scene or art yet — this is the "bone
structure," with the engine pointed in the right direction. Seven scripts, each
with one clear responsibility:

| Script | Role |
|---|---|
| `Systems/GriefMeter.cs` | **The heart of the game.** A hidden 0–1 grief meter. Rises when the player runs, falls when the player faces a memory. Every other system listens to this value. |
| `Systems/GameManager.cs` | Tracks how many memories have been faced → unlocks the final voicemail one piece at a time. When all are faced, the message plays in full — and turns out not to be an accusation. |
| `Player/FirstPersonController.cs` | Movement, mouse look, and interaction (press E). When running (Shift), it reports to the `GriefMeter` — the act of fleeing is what feeds the grief. |
| `World/Interactable.cs` | Base class for anything you can press E on: a photo, a cup, a door, the focal object of a memory. |
| `World/MemoryObject.cs` | A memory meant to be faced. When faced: eases grief + advances the story. Can only be faced once. |
| `World/RoomAtmosphere.cs` | Makes a room respond to grief: the higher the grief, the dimmer and colder the light; as grief falls, warmth returns. The house becomes a visible read-out of the character's inner state. |
| `Entity/GriefEntity.cs` | The presence wearing the partner's face. It approaches to embrace, not to attack. The higher the grief, the faster it closes in — so running literally makes it gain on you. |

> Language: code comments are intentionally in English (common convention and
> convenient if the repo ever goes public / collaborative). Change anything to
> suit your workflow.

---

## Folder structure

```
GriefHorror/
├── .gitignore
├── README.md
└── Assets/
    └── Scripts/
        ├── Player/   FirstPersonController.cs
        ├── Systems/  GriefMeter.cs, GameManager.cs
        ├── World/    Interactable.cs, MemoryObject.cs, RoomAtmosphere.cs
        └── Entity/   GriefEntity.cs
```

Built for **Unity 2021 LTS or newer** (2022 LTS recommended), the **Built-in
Render Pipeline**, and the legacy **Input Manager** (so no extra packages are
needed to get started).

---

## Unity setup (~5 minutes)

1. Create a new 3D Unity project, then copy the `Assets/Scripts` folder into it.
   Let Unity compile.
2. **Player:** create an empty GameObject named `Player`.
   - Add a **Character Controller** component.
   - Add the **First Person Controller** script.
   - Set its **Tag** to `Player` (the Tag dropdown at the top of the Inspector —
     this tag ships with Unity by default). This is how the presence finds you.
   - Create a **Camera** as a child of `Player`, positioned around `y = 1.6`.
     Drag that camera into the **Camera Transform** slot on the script.
3. **Floor:** create a `3D Object > Plane` to stand on (a Plane already has a
   collider).
4. **Systems:** create an empty GameObject `Systems` and add both **Grief
   Meter** and **Game Manager** to it.
5. **Memory:** create a `3D Object > Cube` (treat it as a photo/cup), add the
   **Memory Object** script, and fill in its *Memory Line*. Make several; the
   count should match *Truths To Face For Ending* on the Game Manager (default 5).
6. **Room:** create a `Light` (Point/Spot) and add the **Room Atmosphere**
   script — it automatically uses the Light on the same object.
7. **Presence:** create a `3D Object > Capsule` (swap in a model later) and add
   the **Grief Entity** script. It will find `Player` via the tag.
8. Press **Play.** Walk around, press E on the Cube to face a memory, and watch
   the presence close in faster when you run.

Keep an eye on the **Console** — for now memories, the voicemail, and the
embrace "speak" through `Debug.Log` (ready for you to replace with
subtitles/UI/audio later).

> If the room keeps getting darker on its own while you stand still, that's the
> *ambient grief pressure*. Set `Ambient Rise Per Second` on the Grief Meter to
> `0` while you test movement.

---

## Controls

| Action | Key |
|---|---|
| Walk | WASD |
| Run (raises grief) | Left Shift |
| Look | Mouse |
| Interact / face a memory | E |

---

## Architecture philosophy (to keep it aligned with theme)

- **The horror instinct, inverted.** The genre trains players to run. Here,
  running (`GriefMeter.ReportFleeing`) is what makes everything worse. The only
  way forward is to stop and face things (`GrantRelief`).
- **The house as an emotion graph.** `RoomAtmosphere` reads the `GriefMeter` so
  the world literally warms as you heal and cools as you flee.
- **One thread ties it all together.** `GameManager` holds the voicemail
  progress. Each memory faced unlocks one more piece — a mystery progress bar
  and an emotional knife at once.
- **Loose and event-driven.** Systems communicate through singletons + events
  (`OnGriefChanged`, `OnVoicemailProgressed`, `OnEmbrace`, `OnConfronted`), so
  UI, audio, and visuals can simply "listen" without being tightly coupled.

---

## Roadmap (ideas for upcoming commits)

- [ ] UI: a subtle bar/vignette for grief, a "Press E" prompt, subtitles.
- [ ] Audio: a real voicemail system that unlocks second by second.
- [ ] Replace the memory `Debug.Log` with subtitles + voice-over.
- [ ] A `NavMeshAgent` for the presence so it can navigate around the house.
- [ ] A room system that decays / closes in when avoided.
- [ ] An opening scene that teaches the player "running is safe" — before
      turning it against them.
- [ ] A locked door at the center of the house + a closing scene (the message
      whole, the embrace, two cups becoming one, the front door finally opening).

---

## A note on subject matter

This game touches on loss and suicide. A few things worth protecting from the
start:

- [ ] A content warning on the opening screen.
- [ ] The story arc always lands on **"you couldn't have prevented it"** — the
      protagonist's guilt is irrational, not the real cause. Never frame the
      death as "caused" by a single person.
- [ ] Never depict a method.
- [ ] Provide help resources (e.g., Into The Light / LISA in Indonesia, or the
      988 Suicide & Crisis Lifeline) in a menu or the credits.

---

## Git — first commit

From inside the project folder:

```bash
git init
git add .
git commit -m "Foundation: grief meter, first-person controller, memories & presence"
```

That's the bone structure standing. 🖤
