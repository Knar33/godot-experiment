---
name: do-a-task
description: >-
  Reads the project tasks folder, selects the next most urgent unchecked work items (as many as the context window can reasonly hold), implements it using design and architecture
  docs, updates task files and tests. Use when the user asks to do a task, pick
  up work from tasks, work through the backlog, or complete items from the
  tasks folder.
---

# Do a task

## When this applies

The user wants work driven from repository task files—not ad-hoc features without consulting `tasks/`.

## Before coding

1. **List and read** all markdown under `tasks/` (project root). If `tasks/` is missing or empty, say so and offer to create the first task file from `design/` / `architecture/` or the user’s stated goal—do not invent tasks without a source.

2. **Read planning context** used by this project:
   - `design/` (topic markdown) and/or root `design.md` if that is the active design doc
   - `architecture/` for implementation constraints

3. **Choose what to do**
   - Prefer **unchecked** items at the **top** of each file (tasks are ordered by urgency; completed work is checked and moved to the **bottom**).
   - If the user names a task or file, honor that.
   - Otherwise pick the **largest number of work items** at the top that can be finished in one pass.
   - **Batch** multiple changes as much as possible.

4. **Scope check** — If the selected item is vague, depends on missing design/architecture, or is too large, narrow it: implement one clear sub-part, or add a short clarification note in the task file and proceed on the unblocked portion.

## Implementation

- Match existing code style, patterns, and Godot/C# conventions in the repo.
- Add or extend **unit tests** when the change affects testable logic (per project rules).
- Run relevant tests or builds after changes; fix failures before claiming done.

## After completing work

1. In each touched task file:
   - Mark finished items **checked** (e.g. `- [x]`).
   - **Move** completed blocks to the **bottom** of the file, preserving a single ordered backlog at the top.
2. If technical behavior or decisions changed, update the relevant `architecture/` topic file(s)—not game design prose unless the user explicitly changed design.
3. Summarize for the user: what was selected, what shipped, what remains at the top of the backlog.

## Anti-patterns

- Skipping `tasks/` and implementing whatever seems interesting.
- “Drive-by” edits outside the selected task(s) without user ask.
- Leaving task files stale (unchecked items that are already done).
