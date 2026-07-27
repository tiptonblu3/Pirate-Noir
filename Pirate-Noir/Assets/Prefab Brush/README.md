# Unity 6.4 Grass Prefab Brush

This project contains an editor tool that lets you paint grass prefabs directly in Scene view.

## Setup

1. Open this folder as a Unity project (Unity 6.4).
2. Put your grass prefabs anywhere in `Assets`.
3. Open `Tools > Grass Prefab Brush`.
4. Add one or more grass prefabs to the list.
5. (Optional) Create an empty GameObject and assign it as **Parent** so all painted grass stays organized.
6. Make sure the surface you paint on has colliders and is included in the **Paint Mask**.

## Painting

- Toggle **Paint Mode** and left click or drag in Scene view to paint.
- Use brush settings for radius, density, spacing, random rotation, and random scale.
- Choose a **Brush Type**:
  - `Circle`: even distribution across the full brush.
  - `Ring`: concentrates spawns near the edge of the brush.
  - `Center`: concentrates spawns near the center of the brush.
  - `Grid`: places in a grid-like distribution.
  - `Line`: paints in a strip.
  - `Spiral`: paints in a spiral path.
- Scene view now shows pattern preview points for the active brush type.
- Use **Paint Groups** to choose which groups are allowed to paint (single or multiple at once).
- Enable **Align To Normal** for sloped terrain.
- Prefabs are organized into compact **group boxes**.
- Each group has a shared **Weight**; higher group weight spawns that group more often.
- Within a selected group, prefabs are chosen randomly from that group.
- Default groups start as **Grass** and **Flowers**, and you can add your own groups.

## Erasing

- Toggle **Erase Mode** and left click/drag to remove grass instances in range.
- If **Parent** is assigned, erase only checks children under that parent.
- If **Parent** is empty, erase removes instances matching selected source prefabs.
